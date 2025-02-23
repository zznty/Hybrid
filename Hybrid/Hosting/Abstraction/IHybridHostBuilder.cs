using Microsoft.Extensions.Hosting;

namespace Hybrid.Hosting.Abstraction;

public interface IHybridHostBuilder : IHostBuilder
{
    IHybridHostBuilder ConfigureHybridHostOptions(Action<HybridHostOptionsBuilder> configureDelegate);
    IHybridHostBuilder ConfigureWindowOptions(Action<WindowCreateOptionsBuilder> configureDelegate);
}