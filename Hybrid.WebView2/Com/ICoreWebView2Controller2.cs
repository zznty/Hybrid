using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("c979903e-d4ca-4228-92eb-47ee3fa96eab")]
public partial interface ICoreWebView2Controller2 : ICoreWebView2Controller
{
    int DefaultBackgroundColor { get; set; }
}