using Silk.NET.Windowing;

namespace Hybrid.Hosting;

public sealed record HybridHostOptions(string ContentRoot, string Url, WindowOptions WindowOptions)
{
    public static HybridHostOptions Default => new(AppContext.BaseDirectory, "http://localhost:5173", WindowOptions.Default with
    {
        API = GraphicsAPI.None
    });
}