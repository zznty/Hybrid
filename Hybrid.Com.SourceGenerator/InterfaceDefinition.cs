using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public record InterfaceDefinition(ITypeSymbol Type, Guid Iid, int VTableOffset, ImmutableArray<InterfaceMember> Members, bool EmitDispatchInfo, InterfaceDefinition? Parent)
{
    public static InterfaceDefinition Empty => new(null!, Guid.Empty, 0, ImmutableArray<InterfaceMember>.Empty, false, null);

    public override int GetHashCode()
    {
        var hashCode = Iid.GetHashCode();
        hashCode.CombineHashCode(VTableOffset);
        hashCode.CombineHashCode(EmitDispatchInfo.GetHashCode());
        if (Parent is not null) hashCode.CombineHashCode(Parent.GetHashCode());
        foreach (var member in Members)
        {
            hashCode.CombineHashCode(member.GetHashCode());
        }
        
        return hashCode;
    }

    public virtual bool Equals(InterfaceDefinition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Iid == other.Iid && VTableOffset == other.VTableOffset && Members.SequenceEqual(other.Members) &&
               EmitDispatchInfo == other.EmitDispatchInfo;
    }
}