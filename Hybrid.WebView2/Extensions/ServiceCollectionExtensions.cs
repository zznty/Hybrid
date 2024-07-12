using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Hybrid.WebView2.Extensions;

public static class ServiceCollectionExtensions
{
    public static IHybridSandboxBuilder AddWebView2(this IServiceCollection services)
    {
        services.AddHostedService<WebView2HostedService>();
        
        return new WebView2Builder(services);
    }
}