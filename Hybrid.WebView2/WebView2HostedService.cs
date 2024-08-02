using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;
using Hybrid.Hosting;
using Hybrid.Hosting.Abstraction;
using Silk.NET.Windowing;

namespace Hybrid.WebView2;

public partial class WebView2HostedService(HybridHostOptions hostOptions, IEnumerable<IHostObjectFactory> hostObjectFactories, IEnumerable<IStartupScriptProvider> startupScriptProviders) : IHybridHostedService
{
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Marshal.ThrowExceptionForHR(CloseWebView());
        return Task.CompletedTask;
    }

    public Task StartAsync(IWindow window, CancellationToken cancellationToken)
    {
        if (window.Native?.Win32?.Hwnd is not { } hWnd)
            throw new PlatformNotSupportedException("WebView2 is supported only for Win32 windowing");
        
        window.Resize += _ => Marshal.ThrowExceptionForHR(ResizeWebView(hWnd));
        
        Marshal.ThrowExceptionForHR(CreateWebView(hWnd, hostOptions.Url, hostOptions.ContentRoot, () =>
        {
            foreach (var provider in startupScriptProviders)
            {
                Marshal.ThrowExceptionForHR(AddStartupScript(provider.Script));
            }

            foreach (var hostObjectFactory in hostObjectFactories)
            {
                unsafe
                {
                    var hostObject = (nint)ComInterfaceMarshaller<IDispatch>.ConvertToUnmanaged((IDispatch?)hostObjectFactory.Create());
            
                    Marshal.ThrowExceptionForHR(AddHostObject(hostObjectFactory.Name, hostObject));
                }
            }
        }));
        
        return Task.CompletedTask;
    }

    [LibraryImport("Hybrid.WebView2.Native", EntryPoint = "Create", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int CreateWebView(nint hWnd, string uri, string userDataFolder, Action initializedCallback);
    
    [LibraryImport("Hybrid.WebView2.Native", EntryPoint = "AddHostObject", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int AddHostObject(string hostObjectName, nint hostObject);
    
    [LibraryImport("Hybrid.WebView2.Native", EntryPoint = "Resize", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int ResizeWebView(nint hWnd);
    
    [LibraryImport("Hybrid.WebView2.Native", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int AddStartupScript(string script);
    
    [LibraryImport("Hybrid.WebView2.Native", EntryPoint = "Close", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int CloseWebView();
}