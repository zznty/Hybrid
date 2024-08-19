using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("b96d755e-0319-4e92-a296-23436f46a1fc")]
public partial interface ICoreWebView2Environment
{
    void CreateCoreWebView2Controller(nint parentWindow,
        ICoreWebView2CreateCoreWebView2ControllerCompletedHandler handler);

    // todo IStream definition
    ICoreWebView2WebResourceResponse CreateWebResourceResponse(object content, int statusCode,
        [EmitAs(UnmanagedType.LPWStr)] string reasonPhrase, [EmitAs(UnmanagedType.LPWStr)] string headers);
    
    [EmitAs(UnmanagedType.LPWStr)] string BrowserVersionString { get; }
    
    // todo event NewBrowserVersionAvailable (source gen doesnt support EventRegistrationToken and mapping event keyword)
}