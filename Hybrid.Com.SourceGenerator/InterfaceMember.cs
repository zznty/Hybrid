using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public record InterfaceMember(string Name, InterfaceMemberType MemberType, ISymbol Member, EmitInformation? ReturnInformation, ImmutableArray<EmitInformation> Parameters)
{
    public string AbiName => $"{MemberType switch {
        InterfaceMemberType.PropertyGet => "Get",
        InterfaceMemberType.PropertyPut or InterfaceMemberType.PropertyPutRef => "Put",
        _ => string.Empty
    }}{Name}";

    public bool IsPreserveSig { get; } = Member.GetAttributes()
        .Any(b => b.AttributeClass?.ToDisplayString() == typeof(PreserveSigAttribute).FullName);

    public override int GetHashCode() => AbiName.GetHashCode();

    public virtual bool Equals(InterfaceMember? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return AbiName == other.AbiName;
    }
}