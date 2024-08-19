using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("b99369f3-9b11-47b5-bc6f-8e7895fcea17")]
public partial interface ICoreWebView2AddScriptToExecuteOnDocumentCreatedCompletedHandler
{
    void Invoke(int errorCode, [EmitAs(UnmanagedType.LPWStr)] string id);
}