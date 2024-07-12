using Microsoft.Extensions.Hosting;
using Silk.NET.Windowing;

namespace Hybrid.Hosting.Abstraction;

public interface IHybridHostedService : IHostedService
{
    Task StartAsync(IWindow window, CancellationToken cancellationToken);
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}