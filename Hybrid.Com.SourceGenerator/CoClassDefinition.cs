using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public record CoClassDefinition(ITypeSymbol Type, Guid Iid, ImmutableArray<string> InterfaceNames)
{
    public static CoClassDefinition Empty => new(null!, Guid.Empty, ImmutableArray<string>.Empty);

    public override int GetHashCode()
    {
        var hashCode = Iid.GetHashCode();
        foreach (var name in InterfaceNames)
        {
            hashCode.CombineHashCode(name.GetHashCode());
        }
        
        return hashCode;
    }

    public virtual bool Equals(CoClassDefinition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Iid == other.Iid && InterfaceNames.SequenceEqual(other.InterfaceNames);
    }
}