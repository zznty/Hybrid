using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;

namespace Hybrid.Com;

public static partial class ComMarshalSupport
{                                                                                                                                          
    public static readonly ComWrappers Wrapper = new StrategyBasedComWrappers();
    
    public static unsafe void WriteVariant<T>(this ref PropVariant variant, T value)
    {
        switch (value)
        {
            case not null when typeof(T).IsInterface:
                variant.VarType = VarEnum.VT_UNKNOWN;
                variant.Value = (nint)ComInterfaceMarshaller<T>.ConvertToUnmanaged(value);
                break;
            case byte b:
                variant.VarType = VarEnum.VT_UI1;
                variant.UInt32Value = b;
                break;
            case short s:
                variant.VarType = VarEnum.VT_I2;
                variant.Int32Value = s;
                break;
            case int i:
                variant.VarType = VarEnum.VT_I4;
                variant.Int32Value = i;
                break;
            case long l:
                variant.VarType = VarEnum.VT_I8;
                variant.Int64Value = l;
                break;
            case float f:
                variant.VarType = VarEnum.VT_R4;
                variant.Int32Value = Unsafe.BitCast<float, int>(f);
                break;
            case double d:
                variant.VarType = VarEnum.VT_R8;
                variant.Int64Value = Unsafe.BitCast<double, long>(d);
                break;
            case string s:
                variant.VarType = VarEnum.VT_BSTR;
                variant.Value = Marshal.StringToBSTR(s);
                break;
            case bool b:
                variant.VarType = VarEnum.VT_BOOL;
                variant.Int32Value = b ? 1 : 0;
                break;
            default:
                throw new InvalidCastException($"{typeof(T)} is not supported as a variant type.");
        }
    }
    
    public static unsafe T? As<T>(this ref PropVariant value)
    {
        return value.VarType switch
        {
            VarEnum.VT_UNKNOWN => ComInterfaceMarshaller<T>.ConvertToManaged((void*)value.Value),
            VarEnum.VT_DISPATCH => Query<T>(value.Value),
            VarEnum.VT_UI1 => Unsafe.As<uint, T>(ref value.UInt32Value),
            VarEnum.VT_I2 => Unsafe.As<int, T>(ref value.Int32Value),
            VarEnum.VT_I4 => Unsafe.As<int, T>(ref value.Int32Value),
            VarEnum.VT_I8 => Unsafe.As<long, T>(ref value.Int64Value),
            VarEnum.VT_R4 => Unsafe.As<int, T>(ref value.Int32Value),
            VarEnum.VT_R8 => Unsafe.As<long, T>(ref value.Int64Value),
            VarEnum.VT_BSTR => (T)(object)Marshal.PtrToStringBSTR(value.Value),
            VarEnum.VT_BOOL => Unsafe.As<int, T>(ref value.Int32Value),
            VarEnum.VT_NULL => default,
            VarEnum.VT_EMPTY => default,
#if DEBUG
            _ => throw new InvalidCastException($"{value.VarType} is not supported as a variant type.")
#else
            _ => default
#endif
        };
    }

    public static unsafe T? Query<T>(nint ptr)
    {
        var guid = typeof(T).GUID;
        Marshal.QueryInterface(ptr, ref guid, out var result);

        return ComInterfaceMarshaller<T>.ConvertToManaged((void*)result);
    }
    
    public static unsafe T? GetArg<T>(this ref DISPPARAMS parameters, int index) =>
        Unsafe.Add(ref Unsafe.AsRef<PropVariant>(parameters.rgvarg), index).As<T>();
    
    public static unsafe void WriteTo<T>(T value, nint p)
    {
        if (p == 0)
            throw new COMException("", unchecked((int)0x8002000F)); // DISP_E_PARAMNOTOPTIONAL

        VariantInit(p);
        
        ref var variant = ref *(PropVariant*)p;
        variant.WriteVariant(value);
    }

    public static void InvokeAsCallback(this IDispatch callback)
    {
        DISPPARAMS dispParams = default;
        DispatchCallback(callback, ref dispParams);
    }
    
    public static unsafe void InvokeAsCallback<T0>(this IDispatch callback, T0 arg0)
    {
        var variantArgs = stackalloc PropVariant[1];
        
        VariantInit((nint)variantArgs);
        
        variantArgs[0].WriteVariant(arg0);
        
        DISPPARAMS dispParams = new()
        {
            cArgs = 1,
            rgvarg = variantArgs
        };
        DispatchCallback(callback, ref dispParams);
    }
    
    public static unsafe void InvokeAsCallback<T0, T1>(this IDispatch callback, T0 arg0, T1 arg1)
    {
        var variantArgs = stackalloc PropVariant[2];
        
        VariantInit((nint)variantArgs);
        VariantInit((nint)Unsafe.Add<PropVariant>(variantArgs, 1));

        // args for DISPPARAMS are in reverse order
        variantArgs[0].WriteVariant(arg1);
        variantArgs[1].WriteVariant(arg0);

        DISPPARAMS dispParams = new()
        {
            cArgs = 2,
            rgvarg = variantArgs
        };
        DispatchCallback(callback, ref dispParams);
    }

    private static unsafe void DispatchCallback(IDispatch callback, ref DISPPARAMS dispParams)
    {
        PropVariant result;
        EXCEPINFO excepInfo;

        callback.Invoke(0, default, 1024, InvokeFlags.DispatchMethod, ref dispParams, (nint)(&result), (nint)(&excepInfo), default);
            
        Marshal.ThrowExceptionForHR(excepInfo.scode);
    }

    [LibraryImport("oleaut32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial void VariantInit(nint pVarg);
}