using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("ee9a0f68-f46c-4e32-ac23-ef8cac224d2a")]
public partial interface ICoreWebView2Settings2 : ICoreWebView2Settings
{
    [EmitAs(UnmanagedType.LPWStr)] string UserAgent { get; set; }
}