using Microsoft.Extensions.Hosting;

namespace Hybrid.Hosting.Abstraction;

public interface IHybridApplicationLifetime : IHostApplicationLifetime
{
    void MinimizeApplication();
}