using Hybrid.Hosting;
using Hybrid.WebView2.Extensions;
using Microsoft.Extensions.Hosting;
using Test.HostObjects;

namespace Test;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        var builder = new HybridHostBuilder();

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddWebView2()
                .AddHostObject<ICalculator, Calculator>("calculator")
                .AddStartupScript("chrome.webview.hostObjects.calculator.Add(1, 2).then(sum => alert(`1 + 2 is equal to ${sum}!`)).catch(console.error);");
        });

        builder.Build().Run();
    }
}