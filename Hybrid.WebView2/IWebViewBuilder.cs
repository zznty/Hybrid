using Hybrid.Common;
using Hybrid.Hosting.Abstraction;
using Hybrid.WebView2.Com;
using Microsoft.Extensions.DependencyInjection;

namespace Hybrid.WebView2;

public interface IWebViewBuilder : IHybridSandboxBuilder
{
    void Configure(Action<WebViewOptions> configure);
}

public class WebViewBuilder(IServiceCollection serviceCollection) : CommonBuilder(serviceCollection), IWebViewBuilder
{
    public void Configure(Action<WebViewOptions> configure)
    {
        serviceCollection.PostConfigure(configure);
    }
}

public class WebViewOptions
{
    public bool UseCssTitleBar { get; set; } = true;
    
    public event Action<ICoreWebView2Settings>? ConfigureWebView;
    
    internal void OnConfigureWebView(ICoreWebView2Settings settings)
    {
        ConfigureWebView?.Invoke(settings);
    }
}