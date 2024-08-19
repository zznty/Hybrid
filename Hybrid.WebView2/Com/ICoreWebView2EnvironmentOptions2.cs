using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("FF85C98A-1BA7-4A6B-90C8-2B752C89E9E2")]
public partial interface ICoreWebView2EnvironmentOptions2
{
    bool ExclusiveUserDataFolderAccess { get; set; }
}