using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ELEMDESC
{
    public TYPEDESC tdesc;

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct DESCUNION
    {
        [FieldOffset(0)] public IDLDESC idldesc;
        [FieldOffset(0)] public PARAMDESC paramdesc;
    }

    public DESCUNION desc;
}