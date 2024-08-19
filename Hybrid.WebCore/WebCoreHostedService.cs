using Hybrid.Hosting.Abstraction;
using Silk.NET.Windowing;

namespace Hybrid.WebCore;

public abstract class WebCoreHostedService : IHybridHostedService
{
    public abstract Task StopAsync(CancellationToken cancellationToken);
    public abstract Task StartAsync(IWindow window, CancellationToken cancellationToken);
}