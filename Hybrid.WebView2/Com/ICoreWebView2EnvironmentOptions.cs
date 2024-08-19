using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("2fde08a8-1e9a-4766-8c05-95a9ceb9d1c5")]
public partial interface ICoreWebView2EnvironmentOptions
{
    [EmitAs(UnmanagedType.LPWStr)] string AdditionalBrowserArguments { get; set; }
    [EmitAs(UnmanagedType.LPWStr)] string Language { get; set; }
    [EmitAs(UnmanagedType.LPWStr)] string TargetCompatibleBrowserVersion { get; set; }
    bool AllowSingleSignOnUsingOsPrimaryAccount { get; set; }
}