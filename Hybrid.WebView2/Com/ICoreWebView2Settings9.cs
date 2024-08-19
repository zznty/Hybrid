using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("0528A73B-E92D-49F4-927A-E547DDDAA37D")]
public partial interface ICoreWebView2Settings9 : ICoreWebView2Settings8
{
    bool IsNonClientRegionSupportEnabled { get; set; }
}