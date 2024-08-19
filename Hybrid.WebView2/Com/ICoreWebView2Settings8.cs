using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("9e6b0e8f-86ad-4e81-8147-a9b5edb68650")]
public partial interface ICoreWebView2Settings8 : ICoreWebView2Settings7
{
    bool IsReputationCheckingRequired { get; set; }
}