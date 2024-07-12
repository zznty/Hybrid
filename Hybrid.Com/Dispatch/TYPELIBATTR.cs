using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TYPELIBATTR
{
    public Guid guid;
    public int lcid;
    public SYSKIND syskind;
    public short wMajorVerNum;
    public short wMinorVerNum;
    public LIBFLAGS wLibFlags;
}