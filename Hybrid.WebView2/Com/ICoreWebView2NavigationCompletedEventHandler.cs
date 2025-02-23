using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("D33A35BF-1C49-4F98-93AB-006E0533FE1C")]
public partial interface ICoreWebView2NavigationCompletedEventHandler
{
    void Invoke(ICoreWebView2 sender, ICoreWebView2NavigationCompletedEventArgs args);
}