using System.Drawing;
using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NWindows;

namespace Hybrid.Hosting;

public sealed class HybridHostBuilder : IHybridHostBuilder
{
    private readonly ConfigurationManager _hostConfiguration = new();
    private readonly IConfigurationBuilder _appConfigurationBuilder = new ConfigurationBuilder();
    private readonly HostBuilderContext _hostBuilderContext;
    private readonly IServiceCollection _services = new ServiceCollection();

    private readonly HybridHostOptionsBuilder _optionsBuilder = new();
    private readonly WindowCreateOptionsBuilder _windowOptionsBuilder = new();

    public HybridHostBuilder()
    {
        _services.AddLogging();
        
        _hostBuilderContext = new HostBuilderContext(Properties)
        {
            Configuration = _hostConfiguration
        };

        _windowOptionsBuilder
            .WithResizable(true)
            .WithMinimizable(true)
            .WithShowInTaskBar(true)
            .WithDefaultSizeFactor(new PointF(.6f, .6f))
            .WithEnableComposition(true)
            .WithManualDpi(Dpi.Default)
            .WithEvents(new WindowEventHub());
        
        var defaultOptions = HybridHostOptions.Default;
        
        _optionsBuilder.WithWindowOptions(_windowOptionsBuilder.Build)
            .WithApplicationName(defaultOptions.ApplicationName)
            .WithEnvironmentName(defaultOptions.EnvironmentName)
            .WithContentRoot(defaultOptions.ContentRoot)
            .WithUrl(defaultOptions.Url);
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
        _services.TryAddSingleton(_optionsBuilder.Build());
        _services.TryAddSingleton(_appConfigurationBuilder.Build());
        _services.TryAddSingleton<HybridHostLifetime>();
        _services.TryAddSingleton<IHostLifetime>(s => s.GetRequiredService<HybridHostLifetime>());
        _services.TryAddSingleton<IHostApplicationLifetime>(s => s.GetRequiredService<HybridHostLifetime>());
        _services.TryAddSingleton<IHybridApplicationLifetime>(s => s.GetRequiredService<HybridHostLifetime>());
        _services.TryAddSingleton<IHostEnvironment, HybridHostEnvironment>();
        
        return new HybridHost(_services.BuildServiceProvider());
    }

    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    public IHybridHostBuilder ConfigureHybridHostOptions(Action<HybridHostOptionsBuilder> configureDelegate)
    {
        configureDelegate(_optionsBuilder);
        
        return this;
    }

    public IHybridHostBuilder ConfigureWindowOptions(Action<WindowCreateOptionsBuilder> configureDelegate)
    {
        configureDelegate(_windowOptionsBuilder);
        
        return this;
    }
}