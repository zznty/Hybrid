using Hybrid.Common;
using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Hybrid.WebView2.Extensions;

public static class ServiceCollectionExtensions
{
    public static IWebViewBuilder AddWebView2(this IServiceCollection services)
    {
        services.AddHostedService<WebView2HostedService>();
        services.AddOptions<WebViewOptions>();
        
        return new WebViewBuilder(services);
    }
}