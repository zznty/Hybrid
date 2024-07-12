using Hybrid.Common;

namespace Hybrid.Hosting.Abstraction;

public interface IHybridSandboxBuilder
{
    IHybridSandboxBuilder AddStartupScript(string script);
    IHybridSandboxBuilder AddHostObject<TDefinition, TImplementation>(string name) where TImplementation : class, TDefinition, ISharedHostObject where TDefinition : class;
}