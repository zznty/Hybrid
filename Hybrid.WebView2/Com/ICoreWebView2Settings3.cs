using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("fdb5ab74-af33-4854-84f0-0a631deb5eba")]
public partial interface ICoreWebView2Settings3 : ICoreWebView2Settings2
{
    bool AreBrowserAcceleratorKeysEnabled { get; set; }
}