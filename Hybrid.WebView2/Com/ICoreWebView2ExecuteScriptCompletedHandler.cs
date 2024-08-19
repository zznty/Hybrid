using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("49511172-cc67-4bca-9923-137112f4c4cc")]
public partial interface ICoreWebView2ExecuteScriptCompletedHandler
{
    void Invoke(int errorCode, [EmitAs(UnmanagedType.LPWStr)] string resultObjectAsJson);
}