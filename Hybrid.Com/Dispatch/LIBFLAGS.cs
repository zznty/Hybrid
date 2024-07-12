using System.ComponentModel;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[Flags]
public enum LIBFLAGS : short
{
    LIBFLAG_FRESTRICTED = 0x1,
    LIBFLAG_FCONTROL = 0x2,
    LIBFLAG_FHIDDEN = 0x4,
    LIBFLAG_FHASDISKIMAGE = 0x8
}