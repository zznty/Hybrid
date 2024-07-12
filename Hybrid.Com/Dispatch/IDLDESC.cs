using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct IDLDESC
{
    public IntPtr dwReserved;
    public IDLFLAG wIDLFlags;
}