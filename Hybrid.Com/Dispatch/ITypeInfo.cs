using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;

namespace Hybrid.Com.Dispatch;

[GeneratedComInterface(StringMarshallingCustomType = typeof(BStrStringMarshaller))]
[Guid("00020401-0000-0000-C000-000000000046")]
[ComVisible(true)]
public partial interface ITypeInfo
{
    unsafe TYPEATTR* GetTypeAttr();
    ITypeComp GetTypeComp();
    unsafe FUNCDESC* GetFuncDesc(int index);
    unsafe VARDESC* GetVarDesc(int index);

    void GetNames(int memid, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] string[] rgBstrNames,
        int cMaxNames, out int pcNames);

    void GetRefTypeOfImplType(int index, out nint href);
    IMPLTYPEFLAGS GetImplTypeFlags(int index);

    void GetIDsOfNames(
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1), In]
        string[] rgszNames, 
        int cNames, 
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] 
        int[] pMemId);

    [PreserveSig]
    int Invoke(nint pvInstance, 
        int memid, 
        InvokeFlags wFlags,
        ref DISPPARAMS pDispParams, 
        nint pVarResult,
        nint pExcepInfo,
        nint puArgErr);

    void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext,
        out string strHelpFile);

    void GetDllEntry(int memid, InvokeFlags invKind, [MarshalAs(UnmanagedType.BStr)] out string pBstrDllName, [MarshalAs(UnmanagedType.BStr)] out string pBstrName, out short pwOrdinal);
    ITypeInfo GetRefTypeInfo(int hRef);
    nint AddressOfMember(int memid, InvokeFlags invKind);

    nint CreateInstance(nint pUnkOuter, in Guid riid);

    [return: MarshalAs(UnmanagedType.BStr)] string? GetMops(int memid);
    void GetContainingTypeLib(out ITypeLib ppTLB, out int pIndex);

    [PreserveSig]
    unsafe void ReleaseTypeAttr(TYPEATTR* pTypeAttr);

    [PreserveSig]
    unsafe void ReleaseFuncDesc(FUNCDESC* pFuncDesc);

    [PreserveSig]
    unsafe void ReleaseVarDesc(VARDESC* pVarDesc);
}