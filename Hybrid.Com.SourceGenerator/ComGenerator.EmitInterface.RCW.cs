using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public partial class ComGenerator
{
    private static string EmitDynamicCastableInterface(InterfaceDefinition interfaceDefinition)
    {
        // we don't need RCW proxy for interfaces with dispatch info because they are not intended to be implemented by an unmanaged side
        // instead dynamic activation of an IDispatch inheritor should be used
        if (interfaceDefinition.EmitDispatchInfo)
            return string.Empty;

        return $$"""
                 [global::System.Runtime.InteropServices.DynamicInterfaceCastableImplementationAttribute]
                 file unsafe partial interface InterfaceImplementation
                 {
                    {{string.Join(Environment.NewLine, SelectAllInterfaceMembers(interfaceDefinition)
                        .GroupBy(b => b.member.Name)
                        .Select(g =>
                        {
                            var (definition, member, vtableIndex) = g.First();
                            return $$"""
                                     {{member.Member switch {
                                         IPropertySymbol propertySymbol => propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                         IMethodSymbol methodSymbol => methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                                         _ => throw new NotSupportedException($"Symbol of type {member.Member.GetType()} is not supported.")
                                     }}} {{definition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}.{{member.Name}}{{(
                                     member.MemberType == InterfaceMemberType.Method ?
                                         $"({string.Join(", ", ((IMethodSymbol)member.Member).Parameters.Zip(member.Parameters, (symbol, second) => (symbol, info: second))
                                             .Select(b =>
                                             $"{EmitRefKindExpand(b.symbol.RefKind)}{b.info.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {b.info.Name}"))})" :
                                         string.Empty
                                 )}}
                                     {
                                        {{
                                            (member.MemberType == InterfaceMemberType.Method ?
                                                EmitDynamicCastableMemberBody(interfaceDefinition, member, vtableIndex) :
                                                string.Join(Environment.NewLine, g.Select(b =>
                                                    $$"""
                                                      {{(b.member.MemberType == InterfaceMemberType.PropertyGet ? "get" : "set")}}
                                                      {
                                                         {{EmitDynamicCastableMemberBody(interfaceDefinition, b.member, b.vtableIndex)}}
                                                      }
                                                      """)))
                                        }}
                                     }
                                     """;
                        }))}}
                 }
                 """;
    }

    private static IEnumerable<(InterfaceDefinition definition, InterfaceMember member, int vtableIndex)> SelectAllInterfaceMembers(InterfaceDefinition interfaceDefinition)
    {
        var members =  interfaceDefinition.Members
            .Select((b, i) => (interfaceDefinition, b, i + interfaceDefinition.VTableOffset));
        
        return interfaceDefinition.Parent is null ? members : SelectAllInterfaceMembers(interfaceDefinition.Parent).Concat(members);
    }

    private static string EmitDynamicCastableMemberBody(InterfaceDefinition definition, InterfaceMember member,
        int vtableIndex)
    {
        var parametersInfos =
            member.MemberType is InterfaceMemberType.PropertyPut or InterfaceMemberType.PropertyPutRef &&
            member.ReturnInformation is not null
                ? member.Parameters.Add(member.ReturnInformation)
                : member.Parameters;

        var parameters = (member.Member is IMethodSymbol methodSymbol
            ? methodSymbol.Parameters
                .Zip<IParameterSymbol, EmitInformation, (IParameterSymbol? symbol, EmitInformation info)>(
                    member.Parameters, (symbol, info) => (symbol, info))
            : parametersInfos.Select<EmitInformation, (IParameterSymbol? symbol, EmitInformation info)>(info =>
                (null, info))).ToArray();
        
        var returnInformation =
            member.MemberType is InterfaceMemberType.PropertyPut or InterfaceMemberType.PropertyPutRef &&
            !member.IsPreserveSig
                ? null
                : member.ReturnInformation;

        return $$"""
                 var (__this, __vtable_native) = ((global::System.Runtime.InteropServices.Marshalling.IUnmanagedVirtualMethodTableProvider)this).GetVirtualMethodTableInfoForKey(typeof({{definition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}));
                 bool __invokeSucceeded = default;
                 {{(returnInformation is null ? string.Empty : $"{returnInformation.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} __retVal = default;")}}
                 {{(returnInformation is null ? string.Empty : $"{MarshalType(returnInformation, RefKind.None)} __retVal_native = default;")}}
                 {{(member.IsPreserveSig ? string.Empty : "int __invokeRetVal = default;")}}
                 {{string.Join(Environment.NewLine, parameters.Select(b =>
                     $"{MarshalType(b.info, b.symbol?.RefKind ?? RefKind.None)} __{b.info.Name}_native = default;"))}}
                 try
                 {
                    {{string.Join(Environment.NewLine, parameters.Select(b =>
                        $"__{b.info.Name}_native = {EmitManagedToUnmanagedMarshal(b.info.Name, b.info, b.symbol?.RefKind ?? RefKind.None)};"))}}
                    {
                        {{(member.IsPreserveSig ? returnInformation is null ? string.Empty : "__retVal_native = " : "__invokeRetVal = ")}}((delegate* unmanaged[MemberFunction]<void*{{(parameters.Any() ? ", " : string.Empty)}}{{string.Join(", ", parameters.Select(b =>
                            MarshalType(b.info, b.symbol?.RefKind ?? RefKind.None)))}}, {{(member.IsPreserveSig ?
                            returnInformation is null ?
                                "void" :
                                MarshalType(returnInformation, RefKind.None) :
                            returnInformation is null ?
                                "int" :
                                $"{MarshalType(returnInformation, RefKind.Out)}, int")}}> )__vtable_native[{{vtableIndex}}])(__this{{(parameters.Any() || (!member.IsPreserveSig && returnInformation is not null) ? ", " : string.Empty)}}{{string.Join(", ", parameters.Select(b => $"__{b.info.Name}_native").Concat(member.IsPreserveSig || returnInformation is null ? [] : ["&__retVal_native"]))}});
                    }
                    
                    {{(member.IsPreserveSig ? string.Empty : "global::System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(__invokeRetVal);")}}
                    __invokeSucceeded = true;
                    global::System.GC.KeepAlive(this);
                    {{(returnInformation is null ? string.Empty : $"__retVal = {EmitUnmanagedToManagedMarshal("__retVal_native", returnInformation)};")}}
                 }
                 finally
                 {
                    if (__invokeSucceeded)
                    {
                        {{(returnInformation is null ? string.Empty : EmitManagedToUnmanagedFinalize("__retVal_native", returnInformation) + ";")}}
                        {{string.Join(Environment.NewLine, parameters.Select(b => EmitManagedToUnmanagedFinalize($"__{b.info.Name}_native", b.info) + ";"))}}
                    }
                 }

                 {{(returnInformation is null ? string.Empty : "return __retVal;")}}
                 """;
    }
}