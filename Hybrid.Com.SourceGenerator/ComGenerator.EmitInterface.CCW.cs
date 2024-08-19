﻿using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public partial class ComGenerator
{
    private static void EmitInterfaceSource(SourceProductionContext context, InterfaceDefinition interfaceDefinition)
    {
        context.AddSource($"{interfaceDefinition.Type.ToDisplayString()}.g.cs",
            $$"""
              // <auto-generated />
              #pragma warning disable CS0612, CS0618
              file sealed unsafe class InterfaceInformation : global::System.Runtime.InteropServices.Marshalling.IIUnknownInterfaceType
              {
                  public static global::System.Guid Iid { get; } = new("{{interfaceDefinition.Iid}}");
              
                  private static void** _vtable;
                  public static void** ManagedVirtualMethodTable => _vtable != null ? _vtable : (_vtable = InterfaceImplementation.CreateManagedVirtualFunctionTable());
              }
              {{EmitDynamicCastableInterface(interfaceDefinition)}}
              file unsafe partial interface InterfaceImplementation : {{interfaceDefinition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}
              {
                  internal static void** CreateManagedVirtualFunctionTable()
                  {
                      void** vtable = (void**)global::System.Runtime.CompilerServices.RuntimeHelpers.AllocateTypeAssociatedMemory(typeof({{interfaceDefinition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}), sizeof(void*) * {{interfaceDefinition.VTableOffset + interfaceDefinition.Members.Length}});
                      {
                          {{(interfaceDefinition.Type.Interfaces.SingleOrDefault(IsTypeComVisible) is { } parent ?
                              $"global::System.Runtime.InteropServices.NativeMemory.Copy(global::System.Runtime.InteropServices.Marshalling.StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy.GetIUnknownDerivedDetails(typeof({parent.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}).TypeHandle).ManagedVirtualMethodTable, vtable, (nuint)(sizeof(void*) * {interfaceDefinition.VTableOffset}));" :
                              """
                              nint v0, v1, v2;
                              global::System.Runtime.InteropServices.ComWrappers.GetIUnknownImpl(out v0, out v1, out v2);
                              vtable[0] = (void*)v0;
                              vtable[1] = (void*)v1;
                              vtable[2] = (void*)v2;
                              """)}}
                      }
                      
                      {
                          {{string.Join(Environment.NewLine, interfaceDefinition.Members.Select((member, i) =>
                              $"vtable[{i + interfaceDefinition.VTableOffset}] = (void*)(delegate* unmanaged[MemberFunction]<{string.Join(", ", new[] { "global::System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch*" }.Concat(member.Member switch {
                                  IMethodSymbol methodSymbol => methodSymbol.Parameters
                                      .Zip(member.Parameters, (symbol, second) => (symbol, info: second))
                                      .Select(b => MarshalType(b.info, b.symbol.RefKind)) is { } parameters ?
                                      methodSymbol.ReturnsVoid ? parameters : parameters.Append($"{MarshalType(member.ReturnInformation!, methodSymbol.RefKind)}*") : [],
                                  IPropertySymbol propertySymbol when member.MemberType == InterfaceMemberType.PropertyGet => [$"{MarshalType(member.ReturnInformation!, propertySymbol.RefKind)}*"],
                                  IPropertySymbol propertySymbol => [MarshalType(member.ReturnInformation!, propertySymbol.RefKind)],
                                  _ => throw new NotSupportedException()
                              }).Concat(member switch {
                                  { IsPreserveSig: false } => ["int"],
                                  { Member: IMethodSymbol methodSymbol } => [MarshalType(member.ReturnInformation!, methodSymbol.RefKind)],
                                  { Member: IPropertySymbol propertySymbol, MemberType: InterfaceMemberType.PropertyGet } => [MarshalType(member.ReturnInformation!, propertySymbol.RefKind)],
                                  _ => []
                              }))}>)&ABI_{member.AbiName};"))}}
                      }
                      
                      return vtable;
                  }
                  
                  {{string.Join(Environment.NewLine, interfaceDefinition.Members.Select(member =>
                      $$"""
                        [global::System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvMemberFunction)])]
                        internal static {{(member.IsPreserveSig ? member.Member switch {
                            IMethodSymbol methodSymbol => methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            IPropertySymbol propertySymbol => propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            _ => throw new NotSupportedException($"{member.Member} not supported for preserve sig")
                        } : "int")}} ABI_{{member.AbiName}}({{string.Join(", ", new[] { "global::System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch* __this_unmanaged" }.Concat(member.Member switch {
                        IMethodSymbol methodSymbol => methodSymbol.Parameters
                            .Zip(member.Parameters, (symbol, second) => (symbol, info: second))
                            .Select(b => $"{MarshalType(b.info, b.symbol.RefKind)} __{b.symbol.Name}__unmanaged") is { } parameters ?
                            methodSymbol.ReturnsVoid ? parameters : parameters.Append($"{MarshalType(member.ReturnInformation!, methodSymbol.RefKind)}* __retValPtr") : [],
                        IPropertySymbol propertySymbol when member.MemberType == InterfaceMemberType.PropertyGet => [$"{MarshalType(member.ReturnInformation!, propertySymbol.RefKind)}* __retValPtr"],
                        IPropertySymbol propertySymbol => [$"{MarshalType(member.ReturnInformation!, propertySymbol.RefKind)} __value__unmanaged"],
                        _ => throw new NotSupportedException()
                    }))}})
                        {
                            {{member.Member switch {
                                IMethodSymbol { ReturnsVoid: false } or IPropertySymbol when member.MemberType is InterfaceMemberType.Method or InterfaceMemberType.PropertyGet && !member.IsPreserveSig =>
                                    "ref var __retValUnmanaged = ref *__retValPtr;",
                                _ => string.Empty
                            }}}
                            
                            try
                            {
                                {{member.Member switch {
                                    IMethodSymbol methodSymbol => string.Join(Environment.NewLine, methodSymbol.Parameters
                                        .Zip(member.Parameters, (symbol, second) => (symbol, info: second)).Select(b =>
                                            $"var __arg{b.symbol.Name} = {EmitUnmanagedToManagedMarshal(b.symbol.RefKind == RefKind.None ? $"__{b.symbol.Name}__unmanaged" : $"*__{b.symbol.Name}__unmanaged", b.info)};")),
                                    IPropertySymbol when member.MemberType is not InterfaceMemberType.PropertyGet =>
                                        $"var __value = {EmitUnmanagedToManagedMarshal(member.MemberType == InterfaceMemberType.PropertyPutRef ? "*__value__unmanaged" : "__value__unmanaged", member.ReturnInformation!)};",
                                    _ => string.Empty
                                }}}
                            
                                var @this = global::System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch.GetInstance<{{interfaceDefinition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}>(__this_unmanaged);
                                
                                {{member.Member switch {
                                    IMethodSymbol methodSymbol =>
                                        $"{(methodSymbol.ReturnsVoid ? string.Empty : "var __retVal = ")}@this.{member.Name}({string.Join(", ", methodSymbol.Parameters.Select(b => $"{EmitRefKindExpand(b.RefKind)}__arg{b.Name}"))});",
                                    IPropertySymbol when member.MemberType == InterfaceMemberType.PropertyGet => $"var __retVal = @this.{member.Name};",
                                    IPropertySymbol => $"@this.{member.Name} = __value;",
                                    _ => throw new NotSupportedException()
                                }}}
                                
                                {{member.Member switch {
                                    IMethodSymbol { ReturnsVoid: false } =>
                                        $"{(member.IsPreserveSig ? "return " : "__retValUnmanaged =")} {EmitManagedToUnmanagedMarshal("__retVal", member.ReturnInformation!)};",
                                    IPropertySymbol when member.MemberType is InterfaceMemberType.Method or InterfaceMemberType.PropertyGet =>
                                        $"{(member.IsPreserveSig ? "return " : "__retValUnmanaged =")} {EmitManagedToUnmanagedMarshal("__retVal", member.ReturnInformation!)};",
                                    _ => string.Empty
                                }}}
                            }
                            catch (global::System.Exception __exception)
                            {
                                global::Hybrid.Com.ComMarshalSupport.LastException.Value = __exception;
                                {{(member.IsPreserveSig ? "throw" : "return global::System.Runtime.InteropServices.Marshalling.ExceptionAsHResultMarshaller<int>.ConvertToUnmanaged(__exception)")}};
                            }
                            
                            return default;
                        }
                        """
                  ))}}
              }
              namespace {{interfaceDefinition.Type.ContainingNamespace.ToDisplayString()}}
              {
                  [global::System.Runtime.InteropServices.Marshalling.IUnknownDerivedAttribute<InterfaceInformation, InterfaceImplementation>]
                  public partial interface {{interfaceDefinition.Type.Name}}
                  {
                  }    
              }
              """);
    }
}