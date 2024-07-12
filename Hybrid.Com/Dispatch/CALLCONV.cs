using System.ComponentModel;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
public enum CALLCONV
{
    CC_CDECL = 1,
    CC_MSCPASCAL = 2,
    CC_PASCAL = CC_MSCPASCAL,
    CC_MACPASCAL = 3,
    CC_STDCALL = 4,
    CC_RESERVED = 5,
    CC_SYSCALL = 6,
    CC_MPWCDECL = 7,
    CC_MPWPASCAL = 8,
    CC_MAX = 9
}