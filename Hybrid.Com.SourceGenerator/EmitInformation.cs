using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public record EmitInformation(string Name, ITypeSymbol Type, UnmanagedType? UnmanagedTypeOverride)
{
    public override int GetHashCode()
    {
        var hashCode = Name.GetHashCode();
        hashCode.CombineHashCode(UnmanagedTypeOverride?.GetHashCode() ?? 0);
        return hashCode;
    }

    public virtual bool Equals(EmitInformation? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && UnmanagedTypeOverride == other.UnmanagedTypeOverride;
    }
}