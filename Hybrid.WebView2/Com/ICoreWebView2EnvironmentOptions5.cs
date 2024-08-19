using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("0AE35D64-C47F-4464-814E-259C345D1501")]
public partial interface ICoreWebView2EnvironmentOptions5
{
    bool EnableTrackingPrevention { get; set; }
}