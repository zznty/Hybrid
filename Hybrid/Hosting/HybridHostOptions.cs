using System.Diagnostics;
using NWindows;

namespace Hybrid.Hosting;

public sealed class HybridHostOptions
{
    public string ApplicationName { get; init; } = "Hybrid Application";
    public string EnvironmentName { get; init; } = Debugger.IsAttached ? "Development" : "Production";
    public string ContentRoot { get; init; } = AppContext.BaseDirectory;
    public string Url { get; init; } = "http://localhost:5173";
    public WindowCreateOptions? WindowOptions { get; init; }
    
    internal static HybridHostOptions Default => new();
}