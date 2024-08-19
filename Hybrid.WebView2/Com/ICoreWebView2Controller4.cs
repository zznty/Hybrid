using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("97d418d5-a426-4e49-a151-e1a10f327d9e")]
public partial interface ICoreWebView2Controller4 : ICoreWebView2Controller3
{
    bool AllowExternalDrop { get; set; }
}