using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;
using Hybrid.Common;
using Hybrid.Hosting;
using Hybrid.Hosting.Abstraction;
using Hybrid.WebView2.Com;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NWindows;
using NWindows.Events;

namespace Hybrid.WebView2;

[SharedHostObject<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler, WebView2HostedService>]
[ComVisible(true)]
[Guid("f563d3f4-9ba9-4d95-8c28-821f3ee050ab")]
public partial class WebView2HostedService(
    HybridHostOptions hostOptions,
    IOptions<WebViewOptions> options,
    IEnumerable<IHostObjectFactory> hostObjectFactories,
    IEnumerable<IStartupScriptProvider> startupScriptProviders)
    : IHybridHostedService, 
        ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler,
        ICoreWebView2CreateCoreWebView2ControllerCompletedHandler,
        ICoreWebView2NavigationCompletedEventHandler
{
    private HWND _hWnd;
    private Window? _window;
    private ICoreWebView2Controller? _controller;
    private long _windowShowToken;

    private static event Action? ImmersiveModeChanged;
    
    private static nint _origProc;
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _controller?.Close();
        _controller = null;
        _window = null;
        return Task.CompletedTask;
    }

    public Task StartAsync(Window window, CancellationToken cancellationToken)
    {
        _hWnd = (HWND)window.Handle;
        _window = window;

        var hr = PInvoke.CreateCoreWebView2EnvironmentWithOptions(null, hostOptions.ContentRoot, null, this);
        
        Marshal.ThrowExceptionForHR(hr);
        
        return Task.CompletedTask;
    }

    public void Invoke(int errorCode, ICoreWebView2Environment createdEnvironment)
    {
        Marshal.ThrowExceptionForHR(errorCode);
        
        createdEnvironment.CreateCoreWebView2Controller(_hWnd, this);
    }

    public void Invoke(int errorCode, ICoreWebView2Controller createdController)
    {
        Marshal.ThrowExceptionForHR(errorCode);
        
        _controller = createdController;
        
        ConfigureTransparency(createdController);

        var webView = createdController.CoreWebView2;

        var settings = webView.Settings;

        ConsiderNonClientAreaSupport(settings);

        settings.IsStatusBarEnabled = false;
        settings.IsZoomControlEnabled = false;
        
        options.Value.OnConfigureWebView(settings);

        ImmersiveModeChanged += ConsiderImmersiveMode;

        _window!.Events.Frame += (w, e) =>
        {
            if (_controller is null) return;
            
            var moved = e.ChangeKind is FrameChangeKind.PositionAndSizeChanged or FrameChangeKind.Moved;
            var resized = e.ChangeKind is FrameChangeKind.PositionAndSizeChanged or FrameChangeKind.Resized;

            if (e.ChangeKind is FrameChangeKind.Restored or FrameChangeKind.Shown)
            {
                _controller.IsVisible = true;
            }
            else if (e.ChangeKind is FrameChangeKind.Minimized or FrameChangeKind.Hidden)
                _controller.IsVisible = false;
            
            if (e.ChangeKind == FrameChangeKind.Shown) HookWindowProc();
            
            if (moved)
                _controller.NotifyParentWindowPositionChanged();

            if (!resized) return;
            
            if (_controller is null) return;
            _controller.Bounds = new()
            {
                Left = -1,
                Top = -1,
                Right = w.SizeInPixels.Width,
                Bottom = w.SizeInPixels.Height
            };
        };
        
        foreach (var factory in hostObjectFactories)
            webView.AddHostObjectToScript(factory.Name, factory.Create());
        
        foreach (var provider in startupScriptProviders)
            webView.AddScriptToExecuteOnDocumentCreated(provider.Script, null);

        createdController.Bounds = new()
        {
            Left = -1,
            Top = -1,
            Right = _window!.SizeInPixels.Width,
            Bottom = _window.SizeInPixels.Height
        };
        
        webView.Navigate(hostOptions.Url);
        
        webView.add_NavigationCompleted(this, ref _windowShowToken);
    }

    private unsafe void HookWindowProc()
    {
        _origProc = PInvoke.GetWindowLongPtr(_hWnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC);

        var ptr = (nint)(delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)&WindowProc;

        PInvoke.SetWindowLongPtr(_hWnd, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, ptr);
        
        // hack around webview eating window border region in it's wndproc
        // so we trigger recalculate of non-client area
        if (!PInvoke.SetWindowPos(_hWnd, HWND.Null, 0, 0, _window!.SizeInPixels.Width, _window.SizeInPixels.Height,
                SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED))
            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
    }
    
    private unsafe void ConsiderImmersiveMode()
    {
        var value = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")
            ?.GetValue("AppsUseLightTheme") as int? ?? 0;
        
        value = value == 0 ? 1 : 0;
        
        PInvoke.DwmSetWindowAttribute(_hWnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, &value,
            sizeof(int)).ThrowOnFailure();
    }

    public void Invoke(ICoreWebView2 sender, ICoreWebView2NavigationCompletedEventArgs args)
    {
        _controller!.CoreWebView2.remove_NavigationCompleted(_windowShowToken);

        _window!.Visible = true;
        _window.Activate();
    }

    private void ConsiderNonClientAreaSupport(ICoreWebView2Settings settings)
    {
        if (settings is not ICoreWebView2Settings9 settings9 || !options.Value.UseCssTitleBar)
        {
            _window!.Decorations = true;
            options.Value.UseCssTitleBar = false;
            return;
        }
        
        settings9.IsNonClientRegionSupportEnabled = true;
    }

    private unsafe void ConfigureTransparency(ICoreWebView2Controller controller)
    {
        var captionColor = 0xFFFFFFFE;
        PInvoke.DwmSetWindowAttribute(_hWnd, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, &captionColor, sizeof(int))
            .ThrowOnFailure();

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, build: 22000) ||
            controller is not ICoreWebView2Controller2 controller2)
        {
            return;
        }

        controller2.DefaultBackgroundColor = 0;
        
        /*var cornerPreference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        PInvoke.DwmSetWindowAttribute(_hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, &cornerPreference,
            sizeof(DWM_WINDOW_CORNER_PREFERENCE)).ThrowOnFailure();

        var value = 1;
        PInvoke.DwmSetWindowAttribute(_hWnd, DWMWINDOWATTRIBUTE.DWMWA_USE_HOSTBACKDROPBRUSH, &value,
            sizeof(int)).ThrowOnFailure();*/

        if (OperatingSystem.IsWindowsVersionAtLeast(10, build: 22621))
        {
            var backdropType = DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW;
            PInvoke.DwmSetWindowAttribute(_hWnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, &backdropType,
                sizeof(DWM_SYSTEMBACKDROP_TYPE)).ThrowOnFailure();
        }
    }
    
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case PInvoke.WM_NCCALCSIZE:
            {
                if (wParam.Value == 0 || lParam.Value == 0) return default;
                
                var calcSizeParams = (NCCALCSIZE_PARAMS*)lParam.Value;
                
                calcSizeParams->rgrc._0.top += 1;
                calcSizeParams->rgrc._0.right -= 2;
                calcSizeParams->rgrc._0.bottom -= 2;
                calcSizeParams->rgrc._0.left += 2;
                return default;
            }
            case PInvoke.WM_NCHITTEST:
            {
                const int borderWidth = 8;

                var mousePos = *(POINTS*)&lParam.Value;
                var clientMousePos = new Point(mousePos.x, mousePos.y);
                
                PInvoke.ScreenToClient(hWnd, ref clientMousePos);
                PInvoke.GetClientRect(hWnd, out var windowRect);
                
                if (clientMousePos.Y >= windowRect.bottom - borderWidth)
                {
                    if (clientMousePos.X <= borderWidth)
                        return new((nint)PInvoke.HTBOTTOMLEFT);
                    if (clientMousePos.X >= windowRect.right - borderWidth)
                        return new((nint)PInvoke.HTBOTTOMRIGHT);
                    return new((nint)PInvoke.HTBOTTOM);
                }

                if (clientMousePos.Y <= borderWidth)
                {
                    if (clientMousePos.X <= borderWidth)
                        return new((nint)PInvoke.HTTOPLEFT);
                    if (clientMousePos.X >= windowRect.right - borderWidth)
                        return new((nint)PInvoke.HTTOPRIGHT);
                    return new((nint)PInvoke.HTTOP);
                }

                if (clientMousePos.X <= borderWidth)
                {
                    return new((nint)PInvoke.HTLEFT);
                }

                if (clientMousePos.X >= windowRect.right - borderWidth)
                {
                    return new((nint)PInvoke.HTRIGHT);
                }
                break;
            }
            case PInvoke.WM_SETTINGCHANGE:
            {
                var str = Marshal.PtrToStringUTF8(lParam.Value);
                
                if (str is "ImmersiveColorSet")
                    ImmersiveModeChanged?.Invoke();
                
                break;
            }
        }
        
        return PInvoke.CallWindowProc(_origProc, hWnd, uMsg, wParam, lParam);
    }
}