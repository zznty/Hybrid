using Hybrid.Hosting.Abstraction;

namespace Hybrid.Hosting;

public class StartupScriptProvider(string script) : IStartupScriptProvider
{
    public string Script { get; } = script;
}