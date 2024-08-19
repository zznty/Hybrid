using System.Runtime.InteropServices;

namespace Hybrid.WebKitGtk.Interop;

internal static partial class Gtk
{
    private const string GtkName = "gtk-3";
    private const string GdkName = "gdk-3";
    private const string GObjectName = "gobject-2.0";

    [LibraryImport(GtkName, EntryPoint = "gtk_init", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Init(in int argc, in string[]? argv);

    [LibraryImport(GdkName, EntryPoint = "gdk_display_get_default", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint GetDefaultDisplay();

    [LibraryImport(GdkName, EntryPoint = "gdk_x11_window_foreign_new_for_display", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint NewForeignWindowForDisplay(nint gdkDisplay, nuint xWindow);

    [LibraryImport(GtkName, EntryPoint = "gtk_window_get_type", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int GetWindowWidgetType();

    [LibraryImport(GtkName, EntryPoint = "gtk_widget_new", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint NewWidget(int type, string? firstPropertyName, nint value = 0);
    
    [LibraryImport(GObjectName, EntryPoint = "g_signal_connect_data", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong ConnectSignal(nint instance, string detailedSignal, nint handler, nint data, nint destroyData = 0, int flags = 0);

    [LibraryImport(GtkName, EntryPoint = "gtk_widget_set_has_window", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetHasWindow(nint widget, [MarshalAs(UnmanagedType.Bool)] bool value);
    
    [LibraryImport(GtkName, EntryPoint = "gtk_widget_set_window", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetWindow(nint widget, nint window);
    
    [LibraryImport(GtkName, EntryPoint = "gtk_widget_realize", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Realize(nint widget);

    [LibraryImport(GtkName, EntryPoint = "gtk_main_iteration", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void MainIteration();
    
    [LibraryImport(GtkName, EntryPoint = "gtk_events_pending", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool HasEventsPending();
}