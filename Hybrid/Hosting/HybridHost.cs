using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NWindows;
using NWindows.Threading;

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
        var window = Window.Create(Services.GetRequiredService<HybridHostOptions>().WindowOptions ?? new());

        Dispatcher.Current.InvokeAsyncAndForget(() =>
        {
            if (Services.GetRequiredService<IHostLifetime>() is not HybridHostLifetime hostLifetime) return;
            
            hostLifetime.ConfigureWindow(window);
            hostLifetime.WindowOnLoad();
        });
        
        Dispatcher.Current.Run();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public IServiceProvider Services { get; }
}