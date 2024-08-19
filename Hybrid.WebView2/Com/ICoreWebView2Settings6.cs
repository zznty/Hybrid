using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("11cb3acd-9bc8-43b8-83bf-f40753714f87")]
public partial interface ICoreWebView2Settings6 : ICoreWebView2Settings5
{
    bool IsSwipeNavigationEnabled { get; set; }
}