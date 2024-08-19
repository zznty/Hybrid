using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32;

internal static partial class PInvoke
{
    public static nint SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, nint dwNewLong)
    {
        return SetWindowLongPtr(hWnd, (int)nIndex, dwNewLong);
    }

    public static nint GetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        return GetWindowLongPtr(hWnd, (int)nIndex);
    }
    
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrA", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrA", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial nint GetWindowLongPtr(nint hWnd, int nIndex);
    
    [DllImport("USER32.dll", ExactSpelling = true, EntryPoint = "CallWindowProcW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern LRESULT CallWindowProc(nint lpPrevWndFunc, HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);
}