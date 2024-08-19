using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.WebView2.Com;

namespace Windows.Win32;

internal static partial class PInvoke
{
    [LibraryImport("WebView2Loader", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int CreateCoreWebView2EnvironmentWithOptions(string? browserExecutableFolder,
        string? userDataFolder,
        [MarshalUsing(typeof(UniqueComInterfaceMarshaller<ICoreWebView2EnvironmentOptions>))]
        ICoreWebView2EnvironmentOptions? environmentOptions,
        [MarshalUsing(typeof(UniqueComInterfaceMarshaller<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler>))]
        ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler environmentCreatedHandler);
}