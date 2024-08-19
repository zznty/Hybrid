using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("cb56846c-4168-4d53-b04f-03b6d6796ff2")]
public partial interface ICoreWebView2Settings4 : ICoreWebView2Settings3
{
    bool IsPasswordAutosaveEnabled { get; set; }
    bool IsGeneralAutofillEnabled { get; set; }
}