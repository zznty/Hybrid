using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("488dc902-35ef-42d2-bc7d-94b65c4bc49c")]
public partial interface ICoreWebView2Settings7 : ICoreWebView2Settings6
{
    int HiddenPdfToolbarItems { get; set; }
}