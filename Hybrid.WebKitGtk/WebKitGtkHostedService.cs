using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hybrid.WebCore;
using Hybrid.WebKitGtk.Interop;
using Silk.NET.Windowing;

namespace Hybrid.WebKitGtk;

public class WebKitGtkHostedService : WebCoreHostedService
{
    private bool _running;
    
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return Task.CompletedTask;
    }

    public override unsafe Task StartAsync(IWindow window, CancellationToken cancellationToken)
    {
        if (window.Native?.X11?.Window is not { } xWindow)
            throw new InvalidOperationException("Window is not an X11 window."); 
        
        Gtk.Init(0, null);

        var gDisplay = Gtk.GetDefaultDisplay();

        var gdkWindow = Gtk.NewForeignWindowForDisplay(gDisplay, xWindow);

        var gtkWidget = Gtk.NewWidget(Gtk.GetWindowWidgetType(), null);
        
        delegate* unmanaged[Cdecl]<nint, nint, void> realizeHandler = &OnGtkRealize;
        Gtk.ConnectSignal(gtkWidget, "realize", (nint)realizeHandler, gdkWindow);
        
        Gtk.SetHasWindow(gtkWidget, true);
        Gtk.Realize(gtkWidget);

        _running = true;
        
        new Thread(GtkThread)
        {
            Name = "GTK Events Thread",
            IsBackground = true
        }.Start();
        
        return Task.CompletedTask; 
    }

    private void GtkThread()
    {
        while (_running)
        {
            if (Gtk.HasEventsPending())
                Gtk.MainIteration();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnGtkRealize(nint widget, nint user)
    {
        Gtk.SetWindow(widget, user);
    }
}