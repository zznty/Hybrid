using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VARDESC
{
    public int memid;
    public nint lpstrSchema;

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct DESCUNION
    {
        [FieldOffset(0)] public int oInst;
        [FieldOffset(0)] public IntPtr lpvarValue;
    }

    public DESCUNION desc;

    public ELEMDESC elemdescVar;
    public short wVarFlags;
    public VARKIND varkind;
}