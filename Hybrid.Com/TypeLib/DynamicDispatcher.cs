using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hybrid.Com.Dispatch;
using Hybrid.Com.DynCall;

namespace Hybrid.Com.TypeLib;

public static unsafe class DynamicDispatcher
{
    public static int Dispatch(nint instance, nint vtableEntry,
        ReadOnlySpan<DispatchParameterDetails> dispatchDetails, ref DISPPARAMS dispatchParams, ref PropVariant result)
    {
        ArgumentNullException.ThrowIfNull((void*)instance);
        ArgumentNullException.ThrowIfNull((void*)vtableEntry);

        using var callVm = new DynamicCallVm();
        
        var args = new ReadOnlySpan<PropVariant>(dispatchParams.rgvarg, dispatchParams.cArgs);

        try
        {
            callVm.Mode = DynamicCallMode.C_X64_WIN64;
            callVm.Reset();
        
            callVm.PushArg(instance);

            for (var i = args.Length - 1; i >= 0; i--)
            {
                var paramVarType = dispatchDetails[i + 1].Type;
                var arg = args[i];

                if (arg.VarType != paramVarType && paramVarType == VarEnum.VT_VARIANT && arg.VarType != VarEnum.VT_VARIANT)
                {
                    var variant = (PropVariant*)Marshal.AllocCoTaskMem(sizeof(PropVariant));

                    variant->VarType = arg.VarType;
                    variant->Value = arg.Value;

                    arg.VarType = VarEnum.VT_VARIANT;
                    arg.Value = (nint)variant;
                }
                else if (arg.VarType != paramVarType)
                    return unchecked((int)0x80020008); // DISP_E_BADVARTYPE
            
                switch (arg.VarType)
                {
                    case VarEnum.VT_I1 or VarEnum.VT_UI1:
                        callVm.PushArg((byte)arg.Int32Value);
                        break;
                    case VarEnum.VT_I2 or VarEnum.VT_UI2:
                        callVm.PushArg((ushort)arg.Int32Value);
                        break;
                    case VarEnum.VT_I4 or VarEnum.VT_UI4 or VarEnum.VT_HRESULT:
                        callVm.PushArg(arg.Int32Value);
                        break;
                    case VarEnum.VT_I8 or VarEnum.VT_UI8:
                        callVm.PushArg(arg.Int64Value);
                        break;
                    case VarEnum.VT_R4:
                        callVm.PushArg(Unsafe.BitCast<int, float>(arg.Int32Value));
                        break;
                    case VarEnum.VT_R8:
                        callVm.PushArg(Unsafe.BitCast<nint, double>(arg.Value));
                        break;
                    case VarEnum.VT_BOOL:
                        callVm.PushArg(arg.Int32Value != 0);
                        break;
                    default:
                        callVm.PushArg(arg.Value);
                        break;
                }
            }
        
            result = default;

            if (dispatchDetails.Length - 1 > args.Length && dispatchDetails[0].Type == VarEnum.VT_HRESULT &&
                dispatchDetails[^1].Flags.HasFlag(DispatchParameterFlags.Out))
            {
                nint resultPtr = default;
                callVm.PushArg((nint)(&resultPtr));

                var hr = callVm.CallInt(vtableEntry);
                
                if (hr != 0)
                    return hr;

                result.VarType = dispatchDetails[^1].Type;
                result.Value = resultPtr;

                return 0; // OK
            }

            result.VarType = dispatchDetails[0].Type; // return type
            
            switch (result.VarType)
            {
                case VarEnum.VT_I4 or VarEnum.VT_UI4 or VarEnum.VT_HRESULT:
                    result.Int32Value = callVm.CallInt(vtableEntry);
                    break;
                case VarEnum.VT_I8 or VarEnum.VT_UI8:
                    result.Int64Value = callVm.CallLong(vtableEntry);
                    break;
                case VarEnum.VT_R4:
                    result.Int32Value = Unsafe.BitCast<float, int>(callVm.CallFloat(vtableEntry));
                    break;
                case VarEnum.VT_R8:
                    result.Value = Unsafe.BitCast<double, nint>(callVm.CallDouble(vtableEntry));
                    break;
                case VarEnum.VT_BOOL:
                    result.Int32Value = callVm.CallBool(vtableEntry) ? -1 : 0;
                    break;
                case VarEnum.VT_BSTR:
                case VarEnum.VT_PTR:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                case VarEnum.VT_VARIANT:
                case VarEnum.VT_SAFEARRAY:
                case VarEnum.VT_CARRAY:
                case VarEnum.VT_USERDEFINED:
                case VarEnum.VT_BYREF:
                case VarEnum.VT_RECORD:
                case VarEnum.VT_VOID:
                case VarEnum.VT_ERROR:
                    result.Value = callVm.CallPointer(vtableEntry);
                    break;
                default:
                    callVm.Call(vtableEntry);
                    break;
            }

            return 0; // OK
        }
        finally
        {
            foreach (var variant in args)
            {
                if (variant.VarType == VarEnum.VT_VARIANT) 
                    Marshal.FreeCoTaskMem(variant.Value);
            }
            
            UnwrapVariant(ref result);
        }
    }

    private static void UnwrapVariant(ref PropVariant variant)
    {
        if (variant.VarType != VarEnum.VT_VARIANT) return;
        
        var innerVariant = (PropVariant*)variant.Value;
        
        variant.VarType = innerVariant->VarType;
        variant.Value = innerVariant->Value;
        
        Marshal.FreeCoTaskMem((nint)innerVariant);
    }
}