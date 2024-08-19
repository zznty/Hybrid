using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("e562e4f0-d7fa-43ac-8d71-c05150499f00")]
public partial interface ICoreWebView2Settings
{
    bool IsScriptEnabled { get; set; }
    bool IsWebMessageEnabled { get; set; }
    bool AreDefaultScriptDialogsEnabled { get; set; }
    bool IsStatusBarEnabled { get; set; }
    bool AreDevToolsEnabled { get; set; }
    bool AreDefaultContextMenusEnabled { get; set; }
    bool AreHostObjectsAllowed { get; set; }
    bool IsZoomControlEnabled { get; set; }
    bool IsBuiltInErrorPageEnabled { get; set; }
}