using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct EXCEPINFO
{
    public short wCode;
    public short wReserved;
    public nint bstrSource;
    public nint bstrDescription;
    public nint bstrHelpFile;
    public int dwHelpContext;
    public nint pvReserved;
    public nint pfnDeferredFillIn;
    public int scode;
}