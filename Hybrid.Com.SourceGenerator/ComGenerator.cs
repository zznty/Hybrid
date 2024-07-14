﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hybrid.Com.SourceGenerator;

[Generator(LanguageNames.CSharp)]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1036:Specify analyzer banned API enforcement setting")]
public class ComGenerator : IIncrementalGenerator
{
    private const string SharedHostObjectAttribute = "Hybrid.Common.SharedHostObjectAttribute`2";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName(SharedHostObjectAttribute, (node, _) => node is ClassDeclarationSyntax,
                (syntaxContext, _) =>
                {
                    var coClassType = syntaxContext.Attributes[0].AttributeClass!.TypeArguments[1];

                    if (coClassType.GetAttributes().FirstOrDefault(b =>
                                b.AttributeClass?.ToDisplayString() == typeof(GuidAttribute).FullName)?.ConstructorArguments[0]
                            .Value is not string coClassIidStr || !Guid.TryParse(coClassIidStr, out var coClassIid))
                        return CoClassDefinition.Empty;

                    return new(coClassType, coClassIid,
                        coClassType.AllInterfaces.Where(IsTypeComVisible)
                        .Select(b => b.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                        .ToImmutableArray());
                })
            .Where(b => b != CoClassDefinition.Empty), EmitCoClassSource);
        context.RegisterSourceOutput(context.SyntaxProvider.ForAttributeWithMetadataName(
            SharedHostObjectAttribute, (node, _) => node is ClassDeclarationSyntax,
            (syntaxContext, _) =>
            {
                var interfaceType = syntaxContext.Attributes[0].AttributeClass!.TypeArguments[0];
                var coClassType = syntaxContext.Attributes[0].AttributeClass!.TypeArguments[1];
                
                if (interfaceType.GetAttributes().FirstOrDefault(b =>
                            b.AttributeClass?.ToDisplayString() == typeof(GuidAttribute).FullName)?.ConstructorArguments[0]
                        .Value is not string interfaceIidStr || !Guid.TryParse(interfaceIidStr, out var interfaceIid))
                    return InterfaceDefinition.Empty;

                bool IsComVisible(ISymbol symbol) => symbol.GetAttributes().All(b =>
                    b.AttributeClass?.ToDisplayString() != typeof(ComVisibleAttribute).FullName ||
                    b.ConstructorArguments[0].Value is true);

                return new(interfaceType, coClassType, interfaceIid,
                    3 + interfaceType.AllInterfaces.Where(IsTypeComVisible)
                        .SelectMany(b => ExpandMembers(b.GetMembers().Where(IsComVisible))).Count(),
                    ExpandMembers(interfaceType.GetMembers().Where(IsComVisible)).ToImmutableArray());
            })
            .Where(b => b != InterfaceDefinition.Empty), EmitInterfaceSource);
    }

    private static bool IsTypeComVisible(ITypeSymbol symbol) =>
        symbol.GetAttributes().Any(b =>
            b.AttributeClass?.ToDisplayString() == typeof(ComVisibleAttribute).FullName &&
            b.ConstructorArguments[0].Value is true);

    private static void EmitInterfaceSource(SourceProductionContext context, InterfaceDefinition interfaceDefinition)
    {
        bool MemberDoesNotNeedResultPtr(InterfaceMember member) => member.IsPreserveSig ||
                                                                   member is {
                                                                           MemberType: InterfaceMemberType.PropertyPut
                                                                           or InterfaceMemberType.PropertyPutRef
                                                                       } or
                                                                       {
                                                                           MemberType: InterfaceMemberType.Method,
                                                                           Member: IMethodSymbol { ReturnsVoid: true }
                                                                       };

        context.AddSource($"{interfaceDefinition.CoClassType.ToDisplayString()}.Dispatch.g.cs",
            $$"""
// <auto-generated />
file sealed unsafe class ComDispatchInformation : global::Hybrid.Com.TypeLib.IComInterfaceDispatch
{
    private static global::Hybrid.Com.TypeLib.ComInterfaceDispatchDetails* _details;
    
    public static unsafe global::Hybrid.Com.TypeLib.ComInterfaceDispatchDetails* GetComInterfaceDispatchDetails(out int count)
    {
        count = {{interfaceDefinition.VTableOffset + interfaceDefinition.Members.Length}};
        
        if (_details == null)
        {
            _details = (global::Hybrid.Com.TypeLib.ComInterfaceDispatchDetails*)global::System.Runtime.CompilerServices.RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ComDispatchInformation), sizeof(global::Hybrid.Com.TypeLib.ComInterfaceDispatchDetails) * count);
            {
                {{(interfaceDefinition.Type.Interfaces.SingleOrDefault(IsTypeComVisible) is { } dispatchParent ? 
                    $"global::System.Runtime.InteropServices.NativeMemory.Copy(global::Hybrid.Com.TypeLib.IComInterfaceDispatchDetails.GetComInterfaceDispatchDetailsFromTypeHandle(typeof({dispatchParent.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}).TypeHandle, out _), _details, (nuint)(sizeof(void*) * {interfaceDefinition.VTableOffset}));" : 
                    """
                    _details[0].MemberName = global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.ConvertToUnmanaged("QueryInterface");
                    _details[0].Flags = global::Hybrid.Com.Dispatch.InvokeFlags.DispatchMethod;
                    _details[0].ParameterCount = 3;
                    _details[0].Parameters = (global::Hybrid.Com.TypeLib.DispatchParameterDetails*)global::System.Runtime.CompilerServices.RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ComDispatchInformation), sizeof(global::Hybrid.Com.TypeLib.DispatchParameterDetails) * 3);
                    _details[0].Parameters[0].Type = global::System.Runtime.InteropServices.VarEnum.VT_HRESULT;
                    _details[0].Parameters[1].Type = global::System.Runtime.InteropServices.VarEnum.VT_CLSID;
                    _details[0].Parameters[2].Type = global::System.Runtime.InteropServices.VarEnum.VT_UNKNOWN;
                    _details[0].Parameters[1].Flags = global::Hybrid.Com.TypeLib.DispatchParameterFlags.In;
                    _details[0].Parameters[2].Flags = global::Hybrid.Com.TypeLib.DispatchParameterFlags.Out;
                    _details[1].MemberName = global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.ConvertToUnmanaged("AddRef");
                    _details[1].Flags = global::Hybrid.Com.Dispatch.InvokeFlags.DispatchMethod;
                    _details[1].ParameterCount = 1;
                    _details[1].Parameters = (global::Hybrid.Com.TypeLib.DispatchParameterDetails*)global::System.Runtime.CompilerServices.RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ComDispatchInformation), sizeof(global::Hybrid.Com.TypeLib.DispatchParameterDetails) * 1);
                    _details[1].Parameters[0].Type = global::System.Runtime.InteropServices.VarEnum.VT_UI4;
                    _details[2].MemberName = global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.ConvertToUnmanaged("Release");
                    _details[2].Flags = global::Hybrid.Com.Dispatch.InvokeFlags.DispatchMethod;
                    _details[2].ParameterCount = 1;
                    _details[2].Parameters = (global::Hybrid.Com.TypeLib.DispatchParameterDetails*)global::System.Runtime.CompilerServices.RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ComDispatchInformation), sizeof(global::Hybrid.Com.TypeLib.DispatchParameterDetails) * 1);
                    _details[2].Parameters[0].Type = global::System.Runtime.InteropServices.VarEnum.VT_UI4;
                    """)}}
            }
            
            {
                {{string.Join(Environment.NewLine, interfaceDefinition.Members.Select((member, i) =>
                        $"""
                        _details[{interfaceDefinition.VTableOffset + i}].MemberName = global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.ConvertToUnmanaged("{member.Name}");
                        _details[{interfaceDefinition.VTableOffset + i}].Flags = global::Hybrid.Com.Dispatch.InvokeFlags.{member.MemberType switch {
                                InterfaceMemberType.Method => "DispatchMethod",
                                InterfaceMemberType.PropertyGet => "DispatchPropertyGet",
                                InterfaceMemberType.PropertyPut => "DispatchPropertyPut",
                                InterfaceMemberType.PropertyPutRef => "DispatchPropertyPutRef",
                                _ => throw new ArgumentOutOfRangeException()
                        }};
                        _details[{interfaceDefinition.VTableOffset + i}].Parameters = (global::Hybrid.Com.TypeLib.DispatchParameterDetails*)global::System.Runtime.CompilerServices.RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ComDispatchInformation), sizeof(global::Hybrid.Com.TypeLib.DispatchParameterDetails) * (_details[{interfaceDefinition.VTableOffset + i}].ParameterCount = {1 + member switch {
                            { MemberType: InterfaceMemberType.PropertyGet or InterfaceMemberType.PropertyPut or InterfaceMemberType.PropertyPutRef } => 1,
                            { MemberType: InterfaceMemberType.Method, Member: IMethodSymbol methodSymbol } => methodSymbol.Parameters.Length + (MemberDoesNotNeedResultPtr(member) ? 0 : 1), // +1 for the return value
                            _ => 0
                        }}));
                        _details[{interfaceDefinition.VTableOffset + i}].Parameters[0].Type = global::System.Runtime.InteropServices.VarEnum.{member.Member switch {
                            IMethodSymbol or IPropertySymbol when !member.IsPreserveSig => "VT_HRESULT",
                            IMethodSymbol methodSymbol => MarshalTypeToVariant(methodSymbol.ReturnType),
                            _ => throw new NotSupportedException()
                        }};
                        {string.Join(Environment.NewLine, (member.Member switch {
                            IPropertySymbol propertySymbol when member.MemberType != InterfaceMemberType.PropertyGet => [propertySymbol.Type],
                            IMethodSymbol methodSymbol when member.IsPreserveSig => methodSymbol.Parameters.Select(b => b.Type),
                            IMethodSymbol methodSymbol when MemberDoesNotNeedResultPtr(member) => methodSymbol.Parameters.Select(b => b.Type),
                            IMethodSymbol methodSymbol => methodSymbol.Parameters.Select(b => b.Type).Append(methodSymbol.ReturnType),
                            _ => []
                        }).Select((type, paramIdx) => $"_details[{interfaceDefinition.VTableOffset + i}].Parameters[{paramIdx + 1}].Type = global::System.Runtime.InteropServices.VarEnum.{MarshalTypeToVariant(type)};"))}
                        {member.MemberType switch {
                            InterfaceMemberType.PropertyGet => $"_details[{interfaceDefinition.VTableOffset + i}].Parameters[1].Flags = global::Hybrid.Com.TypeLib.DispatchParameterFlags.Out;",
                            InterfaceMemberType.PropertyPutRef => $"_details[{interfaceDefinition.VTableOffset + i}].Parameters[1].Flags = global::Hybrid.Com.TypeLib.DispatchParameterFlags.In;",
                            InterfaceMemberType.Method when member.Member is IMethodSymbol methodSymbol => string.Join(Environment.NewLine, methodSymbol.Parameters.Select((b, paramIdx) => $"_details[{interfaceDefinition.VTableOffset + i}].Parameters[{paramIdx + 1}].Flags = {b.RefKind switch {
                                RefKind.None => "global::Hybrid.Com.TypeLib.DispatchParameterFlags.None",
                                RefKind.In => "global::Hybrid.Com.TypeLib.DispatchParameterFlags.In",
                                RefKind.Out => "global::Hybrid.Com.TypeLib.DispatchParameterFlags.Out",
                                RefKind.Ref or RefKind.RefReadOnly => "global::Hybrid.Com.TypeLib.DispatchParameterFlags.In | global::Hybrid.Com.TypeLib.DispatchParameterFlags.Out",
                                _ => throw new ArgumentOutOfRangeException()
                            }};")),
                            _ => string.Empty
                        }}
                        {(MemberDoesNotNeedResultPtr(member) ? string.Empty : $"_details[{interfaceDefinition.VTableOffset + i}].Parameters[{((IMethodSymbol)member.Member).Parameters.Length + 1}].Flags = global::Hybrid.Com.TypeLib.DispatchParameterFlags.Out;")}
                        """
                ))}}
            }
        }
        
        return _details;
    }
}
namespace {{interfaceDefinition.Type.ContainingNamespace.ToDisplayString()}}
{
    [global::Hybrid.Com.TypeLib.ComInterfaceDispatchDetailsAttribute<ComDispatchInformation>]
    public partial interface {{interfaceDefinition.Type.Name}}
    {
    }    
}
""");
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
file unsafe interface InterfaceImplementation : {{interfaceDefinition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}
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
                $"vtable[{i + interfaceDefinition.VTableOffset}] = (void*)(delegate* unmanaged[MemberFunction]<{string.Join(", ", new [] {"global::System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch*"}.Concat(member.Member switch {
                    IMethodSymbol methodSymbol => methodSymbol.Parameters.Select(b => MarshalType(b.Type, b.RefKind)) is { } parameters ? 
                        methodSymbol.ReturnsVoid ? parameters : parameters.Append($"{MarshalType(methodSymbol.ReturnType, methodSymbol.RefKind)}*") : [],
                    IPropertySymbol propertySymbol when member.MemberType == InterfaceMemberType.PropertyGet => [$"{MarshalType(propertySymbol.Type, propertySymbol.RefKind)}*"],
                    IPropertySymbol propertySymbol => [MarshalType(propertySymbol.Type, propertySymbol.RefKind)],
                    _ => throw new NotSupportedException()
                }).Concat(member switch {
                    { IsPreserveSig: false } => ["int"],
                    { Member: IMethodSymbol methodSymbol } => [MarshalType(methodSymbol.ReturnType, methodSymbol.RefKind)],
                    { Member: IPropertySymbol propertySymbol, MemberType: InterfaceMemberType.PropertyGet } => [MarshalType(propertySymbol.Type, propertySymbol.RefKind)],
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
        } : "int")}} ABI_{{member.AbiName}}({{string.Join(", ", new [] {"global::System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch* __this_unmanaged"}.Concat(member.Member switch {
                IMethodSymbol methodSymbol => methodSymbol.Parameters.Select(b => $"{MarshalType(b.Type, b.RefKind)} __{b.Name}__unmanaged") is { } parameters ? 
                    methodSymbol.ReturnsVoid ? parameters : parameters.Append($"{MarshalType(methodSymbol.ReturnType, methodSymbol.RefKind)}* __retValPtr") : [],
                IPropertySymbol propertySymbol when member.MemberType == InterfaceMemberType.PropertyGet => [$"{MarshalType(propertySymbol.Type, propertySymbol.RefKind)}* __retValPtr"],
                IPropertySymbol propertySymbol => [$"{MarshalType(propertySymbol.Type, propertySymbol.RefKind)} __value__unmanaged"],
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
                    IMethodSymbol methodSymbol => string.Join(Environment.NewLine, methodSymbol.Parameters.Select(b => 
                        $"var __arg{b.Name} = {EmitUnmanagedToManagedMarshal(b.RefKind == RefKind.None ? $"__{b.Name}__unmanaged" : $"*__{b.Name}__unmanaged", b.Type)};")),
                    IPropertySymbol propertySymbol when member.MemberType is not InterfaceMemberType.PropertyGet => 
                        $"var __value = {EmitUnmanagedToManagedMarshal(member.MemberType == InterfaceMemberType.PropertyPutRef ? "*__value__unmanaged" : "__value__unmanaged", propertySymbol.Type)};",
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
                    IMethodSymbol { ReturnsVoid: false } methodSymbol => 
                        $"{(member.IsPreserveSig ? "return " : "__retValUnmanaged =")} {EmitManagedToUnmanagedMarshal("__retVal", methodSymbol.ReturnType)};",
                    IPropertySymbol propertySymbol when member.MemberType is InterfaceMemberType.Method or InterfaceMemberType.PropertyGet =>
                        $"{(member.IsPreserveSig ? "return " : "__retValUnmanaged =")} {EmitManagedToUnmanagedMarshal("__retVal", propertySymbol.Type)};",
                    _ => string.Empty
                }}}
            }
            catch (global::System.Exception __exception)
            {
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

    private static string MarshalTypeToVariant(ITypeSymbol type)
    {
        return type.ToDisplayString() switch
        {
            "string" => "VT_BSTR",
            "bool" => "VT_BOOL",
            "int" => "VT_I4",
            "long" => "VT_I8",
            "float" => "VT_R4",
            "double" => "VT_R8",
            _ when type.TypeKind == TypeKind.Interface => "VT_UNKNOWN",
            _ when type.TypeKind == TypeKind.Delegate => "VT_DISPATCH",
            "object" => "VT_UNKNOWN",
            "void" => "VT_EMPTY",
            "nint" or "IntPtr" => "VT_PTR",
            "sbyte" => "VT_I1",
            "byte" => "VT_UI1",
            "short" => "VT_I2",
            "ushort" => "VT_UI2",
            "uint" => "VT_UI4",
            "ulong" => "VT_UI8",
            "char" => "VT_UI2",
            _ => throw new NotSupportedException()
        };
    }

    private static string EmitRefKindExpand(RefKind refKind)
    {
        return refKind switch
        {
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            RefKind.In or RefKind.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(refKind), refKind, null)
        };
    }

    private static string EmitManagedToUnmanagedMarshal(string managedName, ITypeSymbol type)
    {
        if (type.ToDisplayString() == typeof(bool).FullName)
            return $"({managedName} ? 1 : 0)";
        if (type.IsUnmanagedType)
            return managedName;
        if (type.ToDisplayString() == typeof(string).FullName)
            return $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.ConvertToUnmanaged({managedName})";
        
        return $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>.ConvertToUnmanaged({managedName})";
    }

    private static string EmitUnmanagedToManagedMarshal(string unmanagedName, ITypeSymbol type)
    {
        if (type.ToDisplayString() == typeof(bool).FullName)
            return $"({unmanagedName} != 0)";
        if (type.IsUnmanagedType)
            return unmanagedName;
        if (type.ToDisplayString() == typeof(string).FullName)
            return $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.ConvertToManaged({unmanagedName})";
        if (type.TypeKind == TypeKind.Delegate)
        {
            var invoke = type.GetMembers("Invoke").OfType<IMethodSymbol>().Single();

            if (!invoke.ReturnsVoid)
                return "<UNSUPPORTED>";
            
            return $$"""
                   ({{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}})(({{string.Join(", ", invoke.Parameters.Select(b => $"__{b.Name}"))}}) => {
                        var __dispatch = global::System.Runtime.InteropServices.Marshalling.UniqueComInterfaceMarshaller<global::Hybrid.Com.Dispatch.IDispatch>.ConvertToManaged({{unmanagedName}});
                        
                        global::Hybrid.Com.ComMarshalSupport.InvokeAsValue(__dispatch{{(invoke.Parameters.Any() ? ", " : string.Empty)}}{{string.Join(", ", invoke.Parameters.Select(b => $"__{b.Name}"))}});
                   })
                   """;
        }
        
        return $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>.ConvertToManaged({unmanagedName})";
    }

    private static string MarshalType(ITypeSymbol type, RefKind refKind)
    {
        string typeName;
        if (type.ToDisplayString() == typeof(bool).FullName)
            typeName = "int";
        else if (type.IsUnmanagedType)
            typeName =  type.ToDisplayString();
        else if (type.ToDisplayString() == typeof(string).FullName)
            typeName =  "ushort*";
        else
            typeName =  "void*";
        
        return refKind is not RefKind.None ? $"{typeName}*" : typeName;
    }

    private static IEnumerable<InterfaceMember> ExpandMembers(IEnumerable<ISymbol> members)
    {
        return members.SelectMany<ISymbol, InterfaceMember>(b => b switch
        {
            IMethodSymbol => [new InterfaceMember(b.Name, InterfaceMemberType.Method, b)],
            IPropertySymbol { IsReadOnly: true } => [new InterfaceMember(b.Name, InterfaceMemberType.PropertyGet, b)],
            IPropertySymbol { IsWriteOnly: true } propertySymbol => [new InterfaceMember(b.Name, IsPrimitiveType(propertySymbol.Type) ? InterfaceMemberType.PropertyPut : InterfaceMemberType.PropertyPutRef, b)],
            IPropertySymbol propertySymbol => [
                new InterfaceMember(b.Name, InterfaceMemberType.PropertyGet, b), 
                new InterfaceMember(b.Name, IsPrimitiveType(propertySymbol.Type) ? InterfaceMemberType.PropertyPut : InterfaceMemberType.PropertyPutRef, b)
            ],
            _ => []
        });
    }

    private static bool IsPrimitiveType(ITypeSymbol symbol) =>
        symbol.IsUnmanagedType || symbol.ToDisplayString() == typeof(string).FullName;

    private static void EmitCoClassSource(SourceProductionContext context, CoClassDefinition classDefinition)
    {
        context.AddSource($"{classDefinition.Type.ToDisplayString()}.g.cs", 
$$"""
// <auto-generated />
file sealed unsafe class ComClassInformation : global::System.Runtime.InteropServices.Marshalling.IComExposedClass
{
    private static volatile global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry* s_vtables;

    public static global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry* GetComInterfaceEntries(out int count)
    {
        count = {{classDefinition.InterfaceNames.Length}};
        if (s_vtables == null)
        {
            global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry* vtables = (global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry*)global::System.Runtime.CompilerServices.RuntimeHelpers
                .AllocateTypeAssociatedMemory(typeof(ComClassInformation), sizeof(global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry) * count);
            global::System.Runtime.InteropServices.Marshalling.IIUnknownDerivedDetails details;
            {{string.Join(Environment.NewLine, classDefinition.InterfaceNames.Select((name, i) => $$"""
                  details = global::System.Runtime.InteropServices.Marshalling.StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy
                     .GetIUnknownDerivedDetails(typeof({{name}}).TypeHandle);
                  vtables[{{i}}] = new()
                  {
                      IID = details.Iid,
                      Vtable = (nint)details.ManagedVirtualMethodTable
                  };
                  """))}}
                                                                                     
            s_vtables = vtables;
        }
        
        return s_vtables;
    }
}

namespace {{classDefinition.Type.ContainingNamespace.ToDisplayString()}}
{
    [global::System.Runtime.InteropServices.Marshalling.ComExposedClass<ComClassInformation>]
    public partial class {{classDefinition.Type.Name}}
    {
        static {{classDefinition.Type.Name}}()
        {
            {{string.Join(Environment.NewLine, classDefinition.InterfaceNames.Select(b => 
                $"global::Hybrid.Com.TypeLib.HybridTypeLib.Global.RegisterComInterface<{b}, {classDefinition.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();"
            ))}}
        }
    }
}
""");
    }
}

public record CoClassDefinition(ITypeSymbol Type, Guid Iid, ImmutableArray<string> InterfaceNames)
{
    public static CoClassDefinition Empty => new(null!, Guid.Empty, ImmutableArray<string>.Empty);
}

public record InterfaceDefinition(ITypeSymbol Type, ITypeSymbol CoClassType, Guid Iid, int VTableOffset, ImmutableArray<InterfaceMember> Members)
{
    public static InterfaceDefinition Empty => new(null!, null!, Guid.Empty, 0, ImmutableArray<InterfaceMember>.Empty);
}

public record InterfaceMember(string Name, InterfaceMemberType MemberType, ISymbol Member)
{
    public string AbiName => $"{MemberType switch {
        InterfaceMemberType.PropertyGet => "Get",
        InterfaceMemberType.PropertyPut or InterfaceMemberType.PropertyPutRef => "Put",
        _ => string.Empty
    }}{Name}";
    
    public bool IsPreserveSig => Member.GetAttributes().Any(b => b.AttributeClass?.ToDisplayString() == typeof(PreserveSigAttribute).FullName);
}

public enum InterfaceMemberType
{
    Method,
    PropertyGet,
    PropertyPut,
    PropertyPutRef
}