using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Hybrid.Hosting;

public sealed class HybridHostBuilder : IHostBuilder
{
    private readonly ConfigurationManager _hostConfiguration = new();
    private readonly IConfigurationBuilder _appConfigurationBuilder = new ConfigurationBuilder();
    private readonly HostBuilderContext _hostBuilderContext;
    private readonly IServiceCollection _services = new ServiceCollection();

    public HybridHostBuilder(HybridHostOptions? hostOptions = null)
    {
        _services.AddSingleton(hostOptions ?? HybridHostOptions.Default);
        _services.AddLogging();
        
        _hostBuilderContext = new HostBuilderContext(Properties)
        {
            Configuration = _hostConfiguration
        };
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        configureDelegate(_hostConfiguration);

        return this;
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        configureDelegate(_hostBuilderContext, _appConfigurationBuilder);
        
        return this;
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        configureDelegate(_hostBuilderContext, _services);
        
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        throw new NotSupportedException();
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        throw new NotSupportedException();
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        throw new NotSupportedException();
    }

    public IHost Build()
    {
        _services.TryAddSingleton(_appConfigurationBuilder.Build());
        _services.TryAddSingleton<HybridHostLifetime>();
        _services.TryAddSingleton<IHostLifetime>(s => s.GetRequiredService<HybridHostLifetime>());
        _services.TryAddSingleton<IHostApplicationLifetime>(s => s.GetRequiredService<HybridHostLifetime>());
        _services.TryAddSingleton<IHostEnvironment, HybridHostEnvironment>();
        
        return new HybridHost(_services.BuildServiceProvider());
    }

    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
}