using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[StructLayout(LayoutKind.Sequential)]
public struct PropArray
{
    readonly uint _cElems;
    readonly IntPtr _pElems;
}