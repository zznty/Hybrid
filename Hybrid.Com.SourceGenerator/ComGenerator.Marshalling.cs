using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Hybrid.Com.SourceGenerator;

public partial class ComGenerator
{
    private static string MarshalTypeToVariant(ITypeSymbol type)
    {
        return
            $"(global::System.Runtime.InteropServices.VarEnum)(global::System.Runtime.InteropServices.VarEnum.{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) switch
            {
                "string" => "VT_BSTR",
                "bool" => "VT_BOOL",
                "int" => "VT_I4",
                "long" => "VT_I8",
                "float" => "VT_R4",
                "double" => "VT_R8",
                _ when type.TypeKind == TypeKind.Interface => "VT_UNKNOWN",
                _ when type.TypeKind == TypeKind.Delegate => "VT_DISPATCH",
                _ when type is IArrayTypeSymbol { TypeKind: TypeKind.Array } arrayType => $"VT_ARRAY + (int){MarshalTypeToVariant(arrayType.ElementType)}",
                _ when type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "object" => "VT_VARIANT",
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
                _ => throw new NotSupportedException(
                    $"{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} is not supported for variant marshalling")
            }})";
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

    private static string EmitManagedToUnmanagedFinalize(string unmanagedName, EmitInformation information)
    {
        if (information.UnmanagedTypeOverride == null)
        {
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "string")
                return
                    $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.Free({unmanagedName})";
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "object")
                return $"global::System.Runtime.InteropServices.Marshal.FreeCoTaskMem((nint){unmanagedName})";
            if (information.Type.TypeKind == TypeKind.Array)
                return $"global::Hybrid.Com.ComMarshalSupport.DestroySafeArray({unmanagedName})";
            if (information.Type.IsUnmanagedType)
                return string.Empty;
            if (information.Type.TypeKind == TypeKind.Delegate)
                return
                    $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<global::Hybrid.Com.Dispatch.IDispatch>.Free({unmanagedName})";
            return
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<{information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>.Free({unmanagedName})";
        }

        return information.UnmanagedTypeOverride.Value switch
        {
            UnmanagedType.BStr =>
                $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.Free({unmanagedName})",
            UnmanagedType.IUnknown =>
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<object>.Free({unmanagedName})",
            UnmanagedType.IDispatch =>
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<global::Hybrid.Com.Dispatch.IDispatch>.Free({unmanagedName})",
            UnmanagedType.LPStr =>
                $"global::System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller.Free({unmanagedName})",
            UnmanagedType.LPWStr =>
                $"global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.Free({unmanagedName})",
            UnmanagedType.Bool or UnmanagedType.Error or UnmanagedType.I1
                or UnmanagedType.I2 or UnmanagedType.I4 or UnmanagedType.I8
                or UnmanagedType.U1 or UnmanagedType.U2 or UnmanagedType.U4
                or UnmanagedType.U8 or UnmanagedType.R4 or UnmanagedType.R8
                or UnmanagedType.SysInt or UnmanagedType.SysUInt => string.Empty,
            _ => throw new NotSupportedException($"{information.UnmanagedTypeOverride} is not supported")
        };
    }

    private static string EmitManagedToUnmanagedMarshal(string managedName, EmitInformation information)
    {
        if (information.UnmanagedTypeOverride == null)
        {
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "bool")
                return $"({managedName} ? -1 : 0)";
            if (information.Type.IsUnmanagedType)
                return managedName;
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "string")
                return
                    $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.ConvertToUnmanaged({managedName})";
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "object")
                return $"global::Hybrid.Com.ComMarshalSupport.CreateVariant({managedName})";
            if (information.Type.TypeKind == TypeKind.Array)
                return $"global::Hybrid.Com.ComMarshalSupport.ToSafeArray({managedName})";

            return
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<{information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>.ConvertToUnmanaged({managedName})";
        }

        return information.UnmanagedTypeOverride.Value switch
        {
            UnmanagedType.Bool => $"{managedName} ? -1 : 0",
            UnmanagedType.BStr =>
                $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.ConvertToUnmanaged({managedName})",
            UnmanagedType.IUnknown =>
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<object>.ConvertToUnmanaged({managedName})",
            UnmanagedType.IDispatch =>
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<global::Hybrid.Com.Dispatch.IDispatch>.ConvertToUnmanaged({managedName})",
            UnmanagedType.LPStr =>
                $"global::System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller.ConvertToUnmanaged({managedName})",
            UnmanagedType.LPWStr =>
                $"global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.ConvertToUnmanaged({managedName})",
            UnmanagedType.Error or UnmanagedType.I1 or UnmanagedType.I2 or UnmanagedType.I4 or UnmanagedType.I8
                or UnmanagedType.U1 or UnmanagedType.U2 or UnmanagedType.U4
                or UnmanagedType.U8 or UnmanagedType.R4 or UnmanagedType.R8 or UnmanagedType.SysInt
                or UnmanagedType.SysUInt => $"({MarshalType(information, RefKind.None)}){managedName}",
            _ => throw new NotSupportedException($"{information.UnmanagedTypeOverride} is not supported")
        };
    }

    private static string EmitUnmanagedToManagedMarshal(string unmanagedName, EmitInformation information)
    {
        if (information.UnmanagedTypeOverride == null)
        {
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "bool")
                return $"({unmanagedName} < 0)";
            if (information.Type.IsUnmanagedType)
                return unmanagedName;
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "string")
                return
                    $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.ConvertToManaged({unmanagedName})";
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "object")
                return $"(*{unmanagedName}).Object";
            if (information.Type.TypeKind == TypeKind.Delegate)
            {
                var invoke = information.Type.GetMembers("Invoke").OfType<IMethodSymbol>().Single();

                if (!invoke.ReturnsVoid)
                    throw new NotSupportedException(
                        $"delegate {information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} should not return a value");

                return $$"""
                         (global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<global::Hybrid.Com.Dispatch.IDispatch>.ConvertToManaged({{unmanagedName}}) is { } __dispatch ? ({{information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}})(({{string.Join(", ", invoke.Parameters.Select(b => $"__{b.Name}"))}}) => {
                              global::Hybrid.Com.ComMarshalSupport.InvokeAsValue(__dispatch{{(invoke.Parameters.Any() ? ", " : string.Empty)}}{{string.Join(", ", invoke.Parameters.Select(b => $"__{b.Name}"))}});
                         }) : throw new global::System.NotImplementedException())
                         """;
            }

            if (information.Type.TypeKind == TypeKind.Array)
                return
                    $"global::Hybrid.Com.ComMarshalSupport.FromSafeArray<{information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>({unmanagedName})";

            return
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<{information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>.ConvertToManaged({unmanagedName})";
        }

        return information.UnmanagedTypeOverride.Value switch
        {
            UnmanagedType.Bool => $"{unmanagedName} != 0",
            UnmanagedType.BStr =>
                $"global::System.Runtime.InteropServices.Marshalling.BStrStringMarshaller.ConvertToManaged({unmanagedName})",
            UnmanagedType.IUnknown =>
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<object>.ConvertToManaged({unmanagedName})",
            UnmanagedType.IDispatch =>
                $"global::System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<global::Hybrid.Com.Dispatch.IDispatch>.ConvertToManaged({unmanagedName})",
            UnmanagedType.LPStr =>
                $"global::System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller.ConvertToManaged({unmanagedName})",
            UnmanagedType.LPWStr =>
                $"global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.ConvertToManaged({unmanagedName})",
            UnmanagedType.Error or UnmanagedType.I1 or UnmanagedType.I2 or UnmanagedType.I4 or UnmanagedType.I8
                or UnmanagedType.U1 or UnmanagedType.U2 or UnmanagedType.U4
                or UnmanagedType.U8 or UnmanagedType.R4 or UnmanagedType.R8 or UnmanagedType.SysInt
                or UnmanagedType.SysUInt => $"({MarshalType(information, RefKind.None)}){unmanagedName}",
            _ => throw new NotSupportedException($"{information.UnmanagedTypeOverride} is not supported")
        };
    }

    private static string MarshalType(EmitInformation information, RefKind refKind)
    {
        string typeName;
        if (information.UnmanagedTypeOverride.HasValue)
        {
            // todo Function ptr
            switch (information.UnmanagedTypeOverride.Value)
            {
                case UnmanagedType.Bool:
                    typeName = "int";
                    break;
                case UnmanagedType.BStr:
                    typeName = "ushort*";
                    break;
                case UnmanagedType.Error:
                    typeName = "int"; // HRESULT
                    break;
                case UnmanagedType.I1:
                    typeName = "sbyte";
                    break;
                case UnmanagedType.I2:
                    typeName = "short";
                    break;
                case UnmanagedType.I4:
                    typeName = "int";
                    break;
                case UnmanagedType.I8:
                    typeName = "long";
                    break;
                case UnmanagedType.IUnknown or UnmanagedType.IDispatch:
                    typeName = "void*";
                    break;
                case UnmanagedType.LPStr:
                    typeName = "byte*";
                    break;
                case UnmanagedType.LPWStr:
                    typeName = "ushort*";
                    break;
                case UnmanagedType.R4:
                    typeName = "float";
                    break;
                case UnmanagedType.R8:
                    typeName = "double";
                    break;
                case UnmanagedType.SysInt:
                    typeName = "nint";
                    break;
                case UnmanagedType.SysUInt:
                    typeName = "nuint";
                    break;
                case UnmanagedType.U1:
                    typeName = "byte";
                    break;
                case UnmanagedType.U2:
                    typeName = "ushort";
                    break;
                case UnmanagedType.U4:
                    typeName = "uint";
                    break;
                case UnmanagedType.U8:
                    typeName = "ulong";
                    break;
                default:
                    throw new NotSupportedException($"{information.UnmanagedTypeOverride} is not supported");
            }
        }
        else
        {
            if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "bool")
                typeName = "int";
            else if (information.Type.IsUnmanagedType)
                typeName = information.Type.ToDisplayString();
            else if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "string")
                typeName = "ushort*";
            else if (information.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "object")
                typeName = "global::Hybrid.Com.Dispatch.PropVariant*";
            else if (information.Type.TypeKind == TypeKind.Array)
                typeName = "global::Hybrid.Com.Dispatch.SafeArray*";
            else
                typeName = "void*";
        }

        return refKind is not RefKind.None ? $"{typeName}*" : typeName;
    }
}