using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hybrid.Com.SourceGenerator;

[Generator(LanguageNames.CSharp)]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1036:Specify analyzer banned API enforcement setting")]
public partial class ComGenerator : IIncrementalGenerator
{
    private const string SharedHostObjectAttribute = "Hybrid.Common.SharedHostObjectAttribute`2";
    private const string SharedHostObjectDefinitionAttribute = "Hybrid.Common.SharedHostObjectDefinitionAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName(SharedHostObjectAttribute,
                (node, _) => node is ClassDeclarationSyntax,
                (syntaxContext, _) =>
                {
                    var coClassType = syntaxContext.Attributes[0].AttributeClass!.TypeArguments[1];

                    if (coClassType.GetAttributes().FirstOrDefault(b =>
                                b.AttributeClass?.ToDisplayString() == typeof(GuidAttribute).FullName)
                            ?.ConstructorArguments[0]
                            .Value is not string coClassIidStr || !Guid.TryParse(coClassIidStr, out var coClassIid))
                        return CoClassDefinition.Empty;

                    return new(coClassType, coClassIid,
                        coClassType.AllInterfaces.Where(IsTypeComVisible)
                            .Select(b => b.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                            .Reverse() // reverse interfaces to be ordered base -> derived instead of default derived -> base
                            .ToImmutableArray());
                })
            .Where(b => b != CoClassDefinition.Empty), EmitCoClassSource);


        InterfaceDefinition Transform(ITypeSymbol interfaceType, CancellationToken _)
        {
            if (interfaceType.GetAttributes()
                    .FirstOrDefault(b => b.AttributeClass?.ToDisplayString() == typeof(GuidAttribute).FullName)
                    ?.ConstructorArguments[0].Value is not string interfaceIidStr || !Guid.TryParse(interfaceIidStr, out var interfaceIid))
                return InterfaceDefinition.Empty;

            bool IsComVisible(ISymbol symbol) => !symbol.IsImplicitlyDeclared && symbol is not IMethodSymbol { MethodKind: not MethodKind.Ordinary } && symbol.GetAttributes().All(b => b.AttributeClass?.ToDisplayString() != typeof(ComVisibleAttribute).FullName || b.ConstructorArguments[0].Value is true);

            return new(interfaceType, interfaceIid, 3 + interfaceType.AllInterfaces.Where(IsTypeComVisible)
                    .SelectMany(b => ExpandMembers(b.GetMembers().Where(IsComVisible)))
                    .Count(), ExpandMembers(interfaceType.GetMembers().Where(IsComVisible)).ToImmutableArray(),
                interfaceType.GetAttributes()
                    .First(attr => attr.AttributeClass?.ToDisplayString() == SharedHostObjectDefinitionAttribute)
                    .NamedArguments.All(pair => pair.Key != "EmitDispatchInformation" || pair.Value.Value is true),
                interfaceType.Interfaces.FirstOrDefault() is { } parentInterfaceType &&
                IsTypeComVisible(parentInterfaceType)
                    ? Transform(parentInterfaceType, default)
                    : default);
        }

        var sharedObjectDefinitionProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                SharedHostObjectDefinitionAttribute, (node, _) => node is InterfaceDeclarationSyntax,
                (c, _) => (ITypeSymbol)c.TargetSymbol)
            .Select(Transform)
            .Where(b => b != InterfaceDefinition.Empty);
        
        context.RegisterSourceOutput(sharedObjectDefinitionProvider, EmitInterfaceSource);
        context.RegisterSourceOutput(sharedObjectDefinitionProvider
            .Where(b => b.EmitDispatchInfo),
            EmitDispatchInformationSource);
    }

    private static bool IsTypeComVisible(ITypeSymbol symbol) =>
        !symbol.IsImplicitlyDeclared &&
        symbol.GetAttributes().Any(b =>
            b.AttributeClass?.ToDisplayString() == typeof(ComVisibleAttribute).FullName &&
            b.ConstructorArguments[0].Value is true);

    private static bool MemberDoesNotNeedResultPtr(InterfaceMember member) =>
        member.IsPreserveSig ||
        member is {
                MemberType: InterfaceMemberType.PropertyPut
                or InterfaceMemberType.PropertyPutRef
            } or
            {
                MemberType: InterfaceMemberType.Method,
                Member: IMethodSymbol { ReturnsVoid: true }
            };

    private static IEnumerable<InterfaceMember> ExpandMembers(IEnumerable<ISymbol> members)
    {
        return members.SelectMany<ISymbol, InterfaceMember>(b => b switch
        {
            IMethodSymbol methodSymbol =>
            [
                new InterfaceMember(b.Name, InterfaceMemberType.Method, b,
                    methodSymbol.ReturnsVoid ? null : GetEmitInformation(methodSymbol.ReturnType, methodSymbol),
                    methodSymbol.Parameters.Select(p => GetEmitInformation(p.Type, p)).ToImmutableArray())
            ],
            IPropertySymbol { IsReadOnly: true } propertySymbol =>
            [
                new InterfaceMember(b.Name, InterfaceMemberType.PropertyGet, b,
                    GetEmitInformation(propertySymbol.Type, propertySymbol), ImmutableArray<EmitInformation>.Empty)
            ],
            IPropertySymbol { IsWriteOnly: true } propertySymbol =>
            [
                new InterfaceMember(b.Name, InterfaceMemberType.PropertyPut, b,
                    GetEmitInformation(propertySymbol.Type, propertySymbol), ImmutableArray<EmitInformation>.Empty)
            ],
            IPropertySymbol propertySymbol =>
            [
                new InterfaceMember(b.Name, InterfaceMemberType.PropertyGet, b,
                    GetEmitInformation(propertySymbol.Type, propertySymbol), ImmutableArray<EmitInformation>.Empty),
                new InterfaceMember(b.Name, InterfaceMemberType.PropertyPut, b,
                    GetEmitInformation(propertySymbol.Type, propertySymbol), ImmutableArray<EmitInformation>.Empty)
            ],
            _ => []
        });
    }

    private static EmitInformation GetEmitInformation(ITypeSymbol type, ISymbol attributeSource)
    {
        var name = attributeSource switch
        {
            IPropertySymbol => "value",
            IParameterSymbol parameterSymbol => parameterSymbol.Name,
            IMethodSymbol => "__retVal",
            _ => throw new NotSupportedException(
                $"Type {attributeSource.GetType()} is not supported as an attribute source")
        };

        var emitTypeAttribute = attributeSource.GetAttributes()
            .SingleOrDefault(b => b.AttributeClass?.ToDisplayString() == "Hybrid.Com.EmitAsAttribute");

        var value = emitTypeAttribute?.ConstructorArguments[0].Value as int?;
        
        return new(name, type, value.HasValue ? (UnmanagedType)value.Value : null);
    }
}