using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PARAMDESC
{
    public IntPtr lpVarValue;
    public PARAMFLAG wParamFlags;
}