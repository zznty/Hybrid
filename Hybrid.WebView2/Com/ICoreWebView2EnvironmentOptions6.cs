using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("57D29CC3-C84F-42A0-B0E2-EFFBD5E179DE")]
public partial interface ICoreWebView2EnvironmentOptions6
{
    bool AreBrowserExtensionsEnabled { get; set; }
}