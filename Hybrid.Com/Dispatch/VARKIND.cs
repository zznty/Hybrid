using System.ComponentModel;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
public enum VARKIND
{
    VAR_PERINSTANCE = 0x0,
    VAR_STATIC = 0x1,
    VAR_CONST = 0x2,
    VAR_DISPATCH = 0x3
}