using System.ComponentModel;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
public enum FUNCKIND
{
    FUNC_VIRTUAL = 0,
    FUNC_PUREVIRTUAL = 1,
    FUNC_NONVIRTUAL = 2,
    FUNC_STATIC = 3,
    FUNC_DISPATCH = 4
}