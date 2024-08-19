using Hybrid.Common;
using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Hybrid.WebKitGtk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IHybridSandboxBuilder AddWebKitGtk(this IServiceCollection services)
    {
        services.AddHostedService<WebKitGtkHostedService>();
        
        return new CommonBuilder(services);
    }
}