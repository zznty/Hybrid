using Hybrid.Hosting.Abstraction;
using NWindows;

namespace Hybrid.WebCore;

public abstract class WebCoreHostedService : IHybridHostedService
{
    public abstract Task StopAsync(CancellationToken cancellationToken);
    public abstract Task StartAsync(Window window, CancellationToken cancellationToken);
}