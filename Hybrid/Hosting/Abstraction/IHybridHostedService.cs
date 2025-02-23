using Microsoft.Extensions.Hosting;
using NWindows;

namespace Hybrid.Hosting.Abstraction;

public interface IHybridHostedService : IHostedService
{
    Task StartAsync(Window window, CancellationToken cancellationToken);
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}