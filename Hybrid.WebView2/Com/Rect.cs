using System.Runtime.InteropServices;

namespace Hybrid.WebView2.Com;

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}