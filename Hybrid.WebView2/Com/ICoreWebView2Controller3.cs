using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("f9614724-5d2b-41dc-aef7-73d62b51543b")]
public partial interface ICoreWebView2Controller3 : ICoreWebView2Controller2
{
    double RasterizationScale { get; set; }
    bool ShouldDetectMonitorScaleChanges { get; set; }

    void add_RasterizationScaleChanged();
    void remove_RasterizationScaleChanged();
    
    int BoundsMode { get; set; }
}