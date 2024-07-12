using Hybrid.Common;
using Hybrid.Hosting.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Hybrid.Hosting;

public class GenericHostObjectFactory<TDefinition, TImplementation>(string name, IServiceProvider serviceProvider)
    : IHostObjectFactory where TImplementation : class, TDefinition, ISharedHostObject where TDefinition : notnull
{
    public string Name { get; } = name;
    public object Create()
    {
        return (TImplementation)serviceProvider.GetRequiredService<TDefinition>();
    }
}