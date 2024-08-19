using Hybrid.Hosting;
using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hybrid.Common;

public class CommonBuilder(IServiceCollection serviceCollection) : IHybridSandboxBuilder
{
    public IHybridSandboxBuilder AddStartupScript(string script)
    {
        serviceCollection.AddSingleton<IStartupScriptProvider>(new StartupScriptProvider(script));
        return this;
    }

    public IHybridSandboxBuilder AddHostObject<TDefinition, TImplementation>(string name) where TImplementation : class, TDefinition, ISharedHostObject where TDefinition : class
    {
        serviceCollection.TryAddSingleton<TDefinition, TImplementation>();
        serviceCollection.AddSingleton<IHostObjectFactory>(s => new GenericHostObjectFactory<TDefinition, TImplementation>(name, s));
        return this;
    }
}