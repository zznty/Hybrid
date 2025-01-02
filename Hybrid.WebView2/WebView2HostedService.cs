using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.WindowsAndMessaging;
using Hybrid.Common;
using Hybrid.Hosting;
using Hybrid.Hosting.Abstraction;
using Hybrid.WebView2.Com;
using Microsoft.Extensions.Options;
using Silk.NET.Windowing;

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
        ICoreWebView2CreateCoreWebView2ControllerCompletedHandler
{
    private nint _hWnd;
    private IWindow? _window;
    private ICoreWebView2Controller? _controller;
    
    private static nint _origProc;
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _controller?.Close();
        _controller = null;
        _window = null;
        return Task.CompletedTask;
    }

    public Task StartAsync(IWindow window, CancellationToken cancellationToken)
    {
        if (window.Native?.Win32?.Hwnd is not { } hWnd)
            throw new PlatformNotSupportedException("WebView2 is supported only for Win32 windowing");
        
        _hWnd = hWnd;
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

        if (createdController is ICoreWebView2Controller2 controller2)
            controller2.DefaultBackgroundColor = 0;

        var webView = createdController.CoreWebView2;

        var settings = webView.Settings;

        if (settings is ICoreWebView2Settings9 settings9 && options.Value.UseCssTitleBar)
        {
            settings9.IsNonClientRegionSupportEnabled = true;

            var hWnd = (HWND)_hWnd;
            
            unsafe
            {
                var captionColor = 0xFFFFFFFE;
                PInvoke.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, &captionColor, sizeof(int)).ThrowOnFailure();
            }

            PInvoke.DwmExtendFrameIntoClientArea(hWnd, new MARGINS
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1
            }).ThrowOnFailure();

            var style = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

            style &= ~(int)WINDOW_STYLE.WS_SYSMENU;

            PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

            _origProc = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC);

            unsafe
            {
                var ptr = (nint)(delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)&WindowProc;

                PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, ptr);
            }
            
            if (!PInvoke.SetWindowPos(hWnd, HWND.Null, 0, 0, _window!.Size.X, _window.Size.Y,
                    SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED))
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            if (OperatingSystem.IsWindowsVersionAtLeast(10, build: 22000))
                unsafe
                {
                    var cornerPreference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                    PInvoke.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, &cornerPreference, sizeof(int)).ThrowOnFailure();
                    
                    var value = 1;
                    PInvoke.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_USE_HOSTBACKDROPBRUSH, &value,
                        sizeof(int)).ThrowOnFailure();

                    var backdropType = DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW;
                    PInvoke.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, &backdropType,
                        sizeof(DWM_SYSTEMBACKDROP_TYPE)).ThrowOnFailure();
                }
        }

        settings.IsStatusBarEnabled = false;
        settings.IsZoomControlEnabled = false;
        
        options.Value.OnConfigureWebView(settings);

        _window!.Move += _ => createdController.NotifyParentWindowPositionChanged();
        _window.Resize += size =>
        {
            if (_controller is null) return;
            _controller.Bounds = new()
            {
                Left = -1,
                Top = -1,
                Right = size.X,
                Bottom = size.Y
            };
        };
        
        foreach (var factory in hostObjectFactories)
            webView.AddHostObjectToScript(factory.Name, factory.Create());
        
        foreach (var provider in startupScriptProviders)
            webView.AddScriptToExecuteOnDocumentCreated(provider.Script, null);

        createdController.Bounds = new()
        {
            Left = 0,
            Top = 0,
            Right = _window.Size.X,
            Bottom = _window.Size.Y
        };
        
        webView.Navigate(hostOptions.Url);
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
        }
        
        return PInvoke.CallWindowProc(_origProc, hWnd, uMsg, wParam, lParam);
    }
}