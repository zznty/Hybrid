using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("183e7052-1d03-43a0-ab99-98e043b66b39")]
public partial interface ICoreWebView2Settings5 : ICoreWebView2Settings4
{
    bool IsPinchZoomEnabled { get; set; }
}