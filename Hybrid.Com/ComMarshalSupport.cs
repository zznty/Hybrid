using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;

namespace Hybrid.Com;

public static partial class ComMarshalSupport
{
    public static readonly ComWrappers Wrapper = new StrategyBasedComWrappers();

    public static unsafe void WriteVariant<T>(this ref PropVariant variant, T? value)
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
                variant.Int32Value = b ? -1 : 0;
                break;
            case null:
                variant.VarType = VarEnum.VT_EMPTY;
                variant.Value = 0;
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
            VarEnum.VT_BOOL => (T)(object)(value.Int32Value < 0),
            VarEnum.VT_NULL => default,
            VarEnum.VT_EMPTY => default,
#if DEBUG
            _ => throw new InvalidCastException($"{value.VarType} is not supported as a variant type.")
#else
            _ => default
#endif
        };
    }

    public static VarEnum GetVarType(Type type)
    {
        if (type.IsInterface)
            return VarEnum.VT_UNKNOWN;
        if (type == typeof(string))
            return VarEnum.VT_BSTR;
        if (type == typeof(bool))
            return VarEnum.VT_BOOL;
        if (type == typeof(byte))
            return VarEnum.VT_UI1;
        if (type == typeof(short))
            return VarEnum.VT_I2;
        if (type == typeof(int))
            return VarEnum.VT_I4;
        if (type == typeof(long))
            return VarEnum.VT_I8;
        if (type == typeof(ushort))
            return VarEnum.VT_UI2;
        if (type == typeof(uint))
            return VarEnum.VT_UI4;
        if (type == typeof(ulong))
            return VarEnum.VT_UI8;
        if (type == typeof(float))
            return VarEnum.VT_R4;
        if (type == typeof(double))
            return VarEnum.VT_R8;
        if (type == typeof(DateTime))
            return VarEnum.VT_DATE;
        if (type.IsAssignableTo(typeof(Delegate)))
            return VarEnum.VT_DISPATCH;
        if (type.IsArray)
            return VarEnum.VT_ARRAY + (int)GetVarType(type.GetElementType()!);
        
        throw new NotSupportedException($"{type} is not supported as a variant type.");
    }

    public static Type GuessTypeFromVarType(VarEnum vt)
    {
        return vt switch
        {
            VarEnum.VT_UNKNOWN or VarEnum.VT_NULL or VarEnum.VT_EMPTY or VarEnum.VT_VARIANT => typeof(object),
            VarEnum.VT_DISPATCH => typeof(IDispatch),
            VarEnum.VT_UI1 => typeof(byte),
            VarEnum.VT_I2 => typeof(short),
            VarEnum.VT_I4 => typeof(int),
            VarEnum.VT_I8 => typeof(long),
            VarEnum.VT_R4 => typeof(float),
            VarEnum.VT_R8 => typeof(double),
            VarEnum.VT_BSTR => typeof(string),
            VarEnum.VT_BOOL => typeof(bool),
            VarEnum.VT_HRESULT => typeof(int),
            VarEnum.VT_FILETIME or VarEnum.VT_DATE => typeof(DateTime),
            > VarEnum.VT_SAFEARRAY => GuessTypeFromVarType((VarEnum)(vt - VarEnum.VT_ARRAY)).MakeArrayType(),
            _ => throw new NotSupportedException($"{vt} is not supported as a variant type."),
        };
    }

    public static unsafe T? Query<T>(nint ptr)
    {
        var guid = typeof(T).GUID;
        Marshal.QueryInterface(ptr, ref guid, out var result);

        return ComInterfaceMarshaller<T>.ConvertToManaged((void*)result);
    }

    public static void InvokeAsValue(this IDispatch callback)
    {
        DISPPARAMS dispParams = default;
        DispatchAsValue(callback, ref dispParams);
    }

    public static unsafe void InvokeAsValue<T0>(this IDispatch callback, T0 arg0)
    {
        var variantArgs = stackalloc PropVariant[1];

        VariantInit((nint)variantArgs);

        variantArgs[0].WriteVariant(arg0);

        DISPPARAMS dispParams = new()
        {
            cArgs = 1,
            rgvarg = variantArgs
        };
        DispatchAsValue(callback, ref dispParams);
    }

    public static unsafe void InvokeAsValue<T0, T1>(this IDispatch callback, T0 arg0, T1 arg1)
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
        DispatchAsValue(callback, ref dispParams);
    }

    private static unsafe void DispatchAsValue(IDispatch callback, ref DISPPARAMS dispParams)
    {
        PropVariant result;
        EXCEPINFO excepInfo;

        callback.Invoke(0, default, 1024, InvokeFlags.DispatchMethod, ref dispParams, (nint)(&result),
            (nint)(&excepInfo), default);

        Marshal.ThrowExceptionForHR(excepInfo.scode);
    }

    public static object? AsObject(this in PropVariant variant)
    {
        switch (variant.VarType)
        {
            case VarEnum.VT_BSTR:
                return Marshal.PtrToStringBSTR(variant.Value);
            case VarEnum.VT_EMPTY:
                return null;
            case VarEnum.VT_FILETIME:
                try
                {
                    return DateTime.FromFileTime(variant.Int64Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return DateTime.MinValue;
                }
            case VarEnum.VT_NULL:
                return null;
            case VarEnum.VT_I2:
                return (short)variant.Int32Value;
            case VarEnum.VT_I4:
                return variant.Int32Value;
            case VarEnum.VT_R4:
                return Unsafe.BitCast<int, float>(variant.Int32Value);
            case VarEnum.VT_R8:
                return Unsafe.BitCast<long, double>(variant.Int64Value);
            case VarEnum.VT_DATE:
                return DateTime.FromOADate(Unsafe.BitCast<long, double>(variant.Int64Value));
            case VarEnum.VT_BOOL:
                return Unsafe.BitCast<int, bool>(variant.Int32Value);
            case VarEnum.VT_DISPATCH:
            case VarEnum.VT_UNKNOWN:
                return ComWrappers.TryGetObject(variant.Value, out var instance)
                    ? instance
                    : Wrapper.GetOrCreateObjectForComInstance(variant.Value, CreateObjectFlags.None);
            case VarEnum.VT_I1:
                return Unsafe.BitCast<int, sbyte>(variant.Int32Value);
            case VarEnum.VT_UI1:
                return Unsafe.BitCast<int, byte>(variant.Int32Value);
            case VarEnum.VT_UI2:
                return Unsafe.BitCast<uint, ushort>(variant.UInt32Value);
            case VarEnum.VT_UI4:
                return variant.UInt32Value;
            case VarEnum.VT_I8:
                return variant.Int64Value;
            case VarEnum.VT_UI8:
                return variant.UInt64Value;
            case VarEnum.VT_INT:
                return variant.Int32Value;
            case VarEnum.VT_UINT:
                return variant.UInt32Value;
            case VarEnum.VT_VARIANT:
                unsafe
                {
                    ref var unwrappedVariant = ref *(PropVariant*)variant.Value;
                    
                    return unwrappedVariant.AsObject();
                }
            case > VarEnum.VT_ARRAY:
                var arrayElementType = (VarEnum)(variant.VarType - VarEnum.VT_ARRAY);
                unsafe
                {
                    var safeArray = (SafeArray*)variant.Value;
                    var variantArray = new VariantArray(arrayElementType, safeArray);
                    
                    var array = Array.CreateInstance(GuessTypeFromVarType(arrayElementType), safeArray->rgsabound.cElements);

                    using var dataHandle = variantArray.AccessData<nint>();
                    for (var i = 0; i < safeArray->rgsabound.cElements; i++)
                    {
                        var elementVariant = new PropVariant
                        {
                            VarType = arrayElementType,
                            Value = dataHandle.Data[i]
                        };
                        
                        array.SetValue(elementVariant.Object, i);
                    }
                    
                    return array;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void SetObject(this ref PropVariant variant, object? value)
    {
        switch (variant.VarType)
        {
            case VarEnum.VT_EMPTY or VarEnum.VT_NULL:
                break;
            case { } when value is null:
                variant.VarType = VarEnum.VT_EMPTY;
                variant.Value = nint.Zero;
                break;
            case VarEnum.VT_BSTR:
                variant.Value = Marshal.StringToBSTR((string)value);
                break;
            case VarEnum.VT_FILETIME:
                variant.Int64Value = ((DateTime)value).ToFileTime();
                break;
            case VarEnum.VT_I2:
                variant.Int32Value = (short)value;
                break;
            case VarEnum.VT_I4:
                variant.Int32Value = (int)value;
                break;
            case VarEnum.VT_R4:
                variant.Int32Value = Unsafe.BitCast<float, int>((float)value);
                break;
            case VarEnum.VT_R8:
                variant.Int64Value = Unsafe.BitCast<double, long>((double)value);
                break;
            case VarEnum.VT_DATE:
                variant.Int64Value = Unsafe.BitCast<double, long>(((DateTime)value).ToOADate());
                break;
            case VarEnum.VT_BOOL:
                variant.Int32Value = (bool)value ? -1 : 0;
                break;
            case VarEnum.VT_I1:
                variant.Int32Value = (sbyte)value;
                break;
            case VarEnum.VT_UI1:
                variant.Int32Value = (byte)value;
                break;
            case VarEnum.VT_UI2:
                variant.UInt32Value = (ushort)value;
                break;
            case VarEnum.VT_UI4:
                variant.UInt32Value = (uint)value;
                break;
            case VarEnum.VT_I8:
                variant.Int64Value = (long)value;
                break;
            case VarEnum.VT_UI8:
                variant.UInt64Value = (ulong)value;
                break;
            case VarEnum.VT_INT:
                variant.Int32Value = (int)value;
                break;
            case VarEnum.VT_UINT:
                variant.UInt32Value = (uint)value;
                break;
            case > VarEnum.VT_ARRAY:
                var array = (Array)value;
                unsafe
                {
                    var elementType = (VarEnum)(variant.VarType - VarEnum.VT_ARRAY);
                    var safeArrayPtr = SafeArrayCreate((ushort)elementType, 1, [
                        new SafeArrayBounds
                        {
                            cElements = (uint)array.Length
                        }
                    ]);

                    try
                    {
                        Marshal.ThrowExceptionForHR(VariantArray.SafeArrayAccessData(safeArrayPtr, out var data));

                        var pData = (void**)data;

                        for (var i = 0; i < array.Length; i++)
                        {
                            var elementVariant = new PropVariant
                            {
                                VarType = elementType,
                            };
                            elementVariant.SetObject(array.GetValue(i));
                            
                            if (elementType != elementVariant.VarType)
                                throw new SafeArrayTypeMismatchException($"Element at index {i} has type {variant.VarType} but the array expects {elementType}");
                            
                            pData[i] = (void*)elementVariant.Value;
                        }
                    }
                    finally
                    {
                        Marshal.ThrowExceptionForHR(VariantArray.SafeArrayUnaccessData(safeArrayPtr));
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static unsafe SafeArray* ToSafeArray<T>(this IEnumerable<T> enumerable)
    {
        var dataSpan = enumerable switch
        {
            T[] array => array,
            List<T> list => CollectionsMarshal.AsSpan(list),
            _ => enumerable.ToArray()
        };

        var elementType = GetVarType(typeof(T));
        var safeArrayPtr = SafeArrayCreate((ushort)elementType, 1, [
            new SafeArrayBounds
            {
                cElements = (uint)dataSpan.Length
            }
        ]);

        try
        {
            Marshal.ThrowExceptionForHR(VariantArray.SafeArrayAccessData(safeArrayPtr, out var data));

            var pData = (void**)data;

            for (var i = 0; i < dataSpan.Length; i++)
            {
                var variant = new PropVariant();
                variant.WriteVariant(dataSpan[i]);
                
                if (elementType != variant.VarType)
                    throw new SafeArrayTypeMismatchException($"Element at index {i} has type {variant.VarType} but the array expects {elementType}");
                
                pData[i] = (void*)variant.Value;
            }
        }
        finally
        {
            Marshal.ThrowExceptionForHR(VariantArray.SafeArrayUnaccessData(safeArrayPtr));
        }
        
        return safeArrayPtr;
    }

    public static unsafe T?[] FromSafeArray<T>(SafeArray* safeArray)
    {
        var variantArray = new VariantArray(GetVarType(typeof(T)), safeArray);
        using var dataHandle = variantArray.AccessData<T>();
        return dataHandle.Data;
    }
    
    public static unsafe PropVariant* CreateVariant(object? value, VarEnum? vt = null)
    {
        vt ??= value is null ? VarEnum.VT_EMPTY : GetVarType(value.GetType());

        var ptr = (PropVariant*)Marshal.AllocCoTaskMem(sizeof(PropVariant));

        ptr->VarType = vt.Value;
        ptr->Object = value;
        
        return ptr;
    }

    [LibraryImport("oleaut32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial void VariantInit(nint pVarg);

    [LibraryImport("oleaut32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial SafeArray* SafeArrayCreate(ushort vt, uint cDims,
        ReadOnlySpan<SafeArrayBounds> rgsabound);
}