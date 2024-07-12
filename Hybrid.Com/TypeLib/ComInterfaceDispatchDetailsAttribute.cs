using System.Reflection;
using System.Runtime.InteropServices;
using Hybrid.Com.Dispatch;

namespace Hybrid.Com.TypeLib;

[AttributeUsage(AttributeTargets.Interface)]
public class ComInterfaceDispatchDetailsAttribute<TInformation> : Attribute, IComInterfaceDispatchDetails where TInformation : IComInterfaceDispatch
{
    public unsafe ComInterfaceDispatchDetails* GetComInterfaceDispatchDetails(out int count)
    {
        return TInformation.GetComInterfaceDispatchDetails(out count);
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ComInterfaceDispatchDetails
{
    public ushort* MemberName;
    public InvokeFlags Flags;
    public DispatchParameterDetails* Parameters;
    public int ParameterCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct DispatchParameterDetails
{
    public ushort _type;
    public DispatchParameterFlags Flags;
    public VarEnum Type { get => (VarEnum)_type; set => _type = (ushort)value; }
}

[Flags]
public enum DispatchParameterFlags : byte
{
    None = 0,
    In = 1,
    Out = 2
}

public interface IComInterfaceDispatch
{
    static abstract unsafe ComInterfaceDispatchDetails* GetComInterfaceDispatchDetails(out int count);
}

public interface IComInterfaceDispatchDetails
{
    unsafe ComInterfaceDispatchDetails* GetComInterfaceDispatchDetails(out int count);
    
    public static unsafe ComInterfaceDispatchDetails* GetComInterfaceDispatchDetailsFromTypeHandle(RuntimeTypeHandle typeHandle, out int count)
    {
        count = 0;
        if (Type.GetTypeFromHandle(typeHandle) is not { } type)
            return null;

        if ((IComInterfaceDispatchDetails?)type.GetCustomAttribute(typeof(ComInterfaceDispatchDetailsAttribute<>)) is
            { } dispatchDetails) 
            return dispatchDetails.GetComInterfaceDispatchDetails(out count);
        
        return null;
    }
}