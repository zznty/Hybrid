using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Hybrid.Hosting;

public class HybridHostEnvironment(HybridHostOptions hostOptions) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = hostOptions.EnvironmentName;
    public string ApplicationName { get; set; } = hostOptions.ApplicationName;
    public string ContentRootPath { get; set; } = hostOptions.ContentRoot;
    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(hostOptions.ContentRoot);
}