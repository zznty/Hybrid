using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silk.NET.Windowing;

namespace Hybrid.Hosting;

public sealed class HybridHost : IHost
{
    internal HybridHost(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
    }

    public void Dispose()
    {
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var window = Window.Create(Services.GetRequiredService<HybridHostOptions>().WindowOptions);

        (Services.GetRequiredService<IHostLifetime>() as HybridHostLifetime)?.SetWindow(window);
        
        // this is like that because on some platforms there is a requirement for windowing apis to be invoked from the first thread
        window.Run();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public IServiceProvider Services { get; }
}