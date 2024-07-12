using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hybrid.Com.Dispatch;

[GeneratedComInterface(StringMarshallingCustomType = typeof(BStrStringMarshaller))]
[Guid("00020400-0000-0000-C000-000000000046")]
[ComVisible(true)]
public partial interface IDispatch
{
    int GetTypeInfoCount();
    
    ITypeInfo GetTypeInfo(int iTInfo, int lcid);

    void GetIDsOfNames(in Guid riid,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2), In]
        string[] rgszNames, 
        int cNames, 
        int lcid, 
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] 
        int[] rgdispid);

    [PreserveSig]
    int Invoke(int dispIdMember,
        in Guid riid,
        int lcid,
        InvokeFlags wFlags,
        ref DISPPARAMS pDispParams,
        nint pVarResult,
        nint pExcepInfo,
        nint puArgErr);
}