using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hybrid.Com.Dispatch;

[GeneratedComInterface(StringMarshallingCustomType = typeof(BStrStringMarshaller))]
[Guid("00020402-0000-0000-C000-000000000046")]
[ComVisible(true)]
public partial interface ITypeLib
{
    [PreserveSig]
    int GetTypeInfoCount();

    ITypeInfo GetTypeInfo(int index);
    TYPEKIND GetTypeInfoType(int index);
    ITypeInfo GetTypeInfoOfGuid(ref Guid guid);
    unsafe TYPELIBATTR* GetLibAttr();
    ITypeComp GetTypeComp();

    void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext,
        out string strHelpFile);

    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsName([MarshalAs(UnmanagedType.LPWStr)] ref string szNameBuf, int lHashVal);

    void FindName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal,
        [MarshalAs(UnmanagedType.LPArray), Out] ITypeInfo[] ppTInfo,
        [MarshalAs(UnmanagedType.LPArray), Out] int[] rgMemId, ref short pcFound);

    [PreserveSig]
    unsafe void ReleaseTLibAttr(TYPELIBATTR* pTLibAttr);
}