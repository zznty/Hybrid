using System.Runtime.InteropServices;

namespace Hybrid.Com;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property |
                AttributeTargets.ReturnValue)]
public class EmitAsAttribute(UnmanagedType unmanagedType) : Attribute
{
    public UnmanagedType UnmanagedType { get; } = unmanagedType;
}