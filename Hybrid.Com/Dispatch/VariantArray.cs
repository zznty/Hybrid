using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hybrid.Com.Dispatch;

public unsafe partial class VariantArray(VarEnum elementType, SafeArray* safeArray)
{
    public VarEnum ElementType => elementType;
    public SafeArray* SafeArray => safeArray;
    
    public unsafe VariantArrayDataHandle<T?> AccessData<T>()
    {
        if (typeof(T).IsClass)
            throw new ArgumentException($"{typeof(T)} must be a value type or an interface.");
        if (!typeof(T).IsInterface)
            return new(this,
                new ReadOnlySpan<T?>((void*)safeArray->pvData, (int)safeArray->rgsabound.cElements)
                    .ToArray());
        
        var elements = new T?[(int)safeArray->rgsabound.cElements];
        for (var i = 0; i < elements.Length; i++)
        {
            var element = ((void**)safeArray->pvData)[i];
            elements[i] = ComInterfaceMarshaller<T>.ConvertToManaged(element);
        }

        return new(this, elements);
    }
    
    [LibraryImport("oleaut32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int SafeArrayAccessData(SafeArray* safeArray, out nint pvData);
    
    [LibraryImport("oleaut32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int SafeArrayUnaccessData(SafeArray* safeArray);

    public sealed class VariantArrayDataHandle<T>(VariantArray variantArray, T?[] data) : IDisposable
    {
        public T?[] Data { get; } = data;
        
        private void ReleaseUnmanagedResources()
        {
            Marshal.ThrowExceptionForHR(SafeArrayUnaccessData(variantArray.SafeArray));
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~VariantArrayDataHandle()
        {
            ReleaseUnmanagedResources();
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SafeArray
{
    public ushort cDims;
    public ushort fFeatures;
    public uint cbElements;
    public uint cLocks;
    public nint pvData;
    public SafeArrayBounds rgsabound;
}

[StructLayout(LayoutKind.Sequential)]
public struct SafeArrayBounds
{
    public uint cElements;
    public uint lLbound;
}