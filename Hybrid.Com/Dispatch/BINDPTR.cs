using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct BINDPTR
{
    [FieldOffset(0)] public IntPtr lpfuncdesc;
    [FieldOffset(0)] public IntPtr lpvardesc;
    [FieldOffset(0)] public IntPtr lptcomp;
}