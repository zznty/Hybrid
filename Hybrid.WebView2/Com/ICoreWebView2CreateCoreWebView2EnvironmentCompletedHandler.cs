using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("4e8a3389-c9d8-4bd2-b6b5-124fee6cc14d")]
public partial interface ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler
{
    void Invoke(int errorCode, ICoreWebView2Environment createdEnvironment);
}