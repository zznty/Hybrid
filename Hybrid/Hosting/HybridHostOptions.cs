using System.Diagnostics;
using Silk.NET.Windowing;

namespace Hybrid.Hosting;

public sealed record HybridHostOptions(string ApplicationName, string EnvironmentName, string ContentRoot, string Url, WindowOptions WindowOptions)
{
    public static HybridHostOptions Default => new("Hybrid Application",
        Debugger.IsAttached ? "Development" : "Production",
        AppContext.BaseDirectory,
        "http://localhost:5173",
        WindowOptions.Default with
        {
            API = GraphicsAPI.None
        });
}