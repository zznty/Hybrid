using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("4A5C436E-A9E3-4A2E-89C3-910D3513F5CC")]
public partial interface ICoreWebView2EnvironmentOptions3
{
    bool IsCustomCrashReportingEnabled { get; set; }
}