using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hybrid.Com.Dispatch;

[GeneratedComInterface(StringMarshallingCustomType = typeof(BStrStringMarshaller))]
[Guid("00020403-0000-0000-C000-000000000046")]
[ComVisible(true)]
public partial interface ITypeComp
{
    void Bind([MarshalAs(UnmanagedType.LPWStr)] string szName, int lHashVal, short wFlags, out ITypeInfo ppTInfo,
        out DESCKIND pDescKind, out BINDPTR pBindPtr);

    void BindType([MarshalAs(UnmanagedType.LPWStr)] string szName, int lHashVal, out ITypeInfo ppTInfo,
        out ITypeComp ppTComp);
}