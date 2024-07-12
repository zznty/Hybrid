using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential)]
public struct FUNCDESC
{
    public int memid; // MEMBERID memid;
    public IntPtr lprgscode; // /* [size_is(cScodes)] */ SCODE RPC_FAR *lprgscode;
    public IntPtr lprgelemdescParam; // /* [size_is(cParams)] */ ELEMDESC __RPC_FAR *lprgelemdescParam;
    public FUNCKIND funckind; // FUNCKIND funckind;
    public InvokeFlags invkind; // INVOKEKIND invkind;
    public CALLCONV callconv; // CALLCONV callconv;
    public short cParams; // short cParams;
    public short cParamsOpt; // short cParamsOpt;
    public short oVft; // short oVft;
    public short cScodes; // short cScodes;
    public ELEMDESC elemdescFunc; // ELEMDESC elemdescFunc;
    public short wFuncFlags; // WORD wFuncFlags;
}