using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public unsafe struct DISPPARAMS
{
    public PropVariant* rgvarg;
    public uint* rgdispidNamedArgs;
    public int cArgs;
    public int cNamedArgs;
}