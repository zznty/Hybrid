using System.ComponentModel;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
public enum SYSKIND
{
    SYS_WIN16 = 0,
    SYS_WIN32 = SYS_WIN16 + 1,
    SYS_MAC = SYS_WIN32 + 1,
    SYS_WIN64 = SYS_MAC + 1
}