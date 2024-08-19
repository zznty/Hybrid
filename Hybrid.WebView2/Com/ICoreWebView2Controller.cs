using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("4d00c0d1-9434-4eb6-8078-8697a560334f")]
public partial interface ICoreWebView2Controller
{
    bool IsVisible { get; set; }
    
    // todo struct marshalling
    Rect Bounds { get; set; }
    
    double ZoomFactor { get; set; }

    void add_ZoomFactorChanged();
    void remove_ZoomFactorChanged();

    void SetBoundsAndZoomFactor(Rect bounds, double zoomFactor);
    
    void MoveFocus(int reason);

    void add_MoveFocusRequested();
    void remove_MoveFocusRequested();

    void add_GotFocus();
    void remove_GotFocus();

    void add_LostFocus();
    void remove_LostFocus();

    void add_AcceleratorKeyPressed();
    void remove_AcceleratorKeyPressed();
    
    nint ParentWindow { get; set; }

    void NotifyParentWindowPositionChanged();

    void Close();
    
    ICoreWebView2 CoreWebView2 { get; }
}