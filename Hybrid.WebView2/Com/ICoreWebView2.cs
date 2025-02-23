using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Hybrid.WebView2.Com;

[SharedHostObjectDefinition(EmitDispatchInformation = false)]
[ComVisible(true)]
[Guid("76eceacb-0462-4d94-ac83-423a6793775e")]
public partial interface ICoreWebView2
{
    ICoreWebView2Settings Settings { get; }
    [EmitAs(UnmanagedType.LPWStr)] string Source { get; }

    void Navigate([EmitAs(UnmanagedType.LPWStr)] string uri);
    void NavigateToString([EmitAs(UnmanagedType.LPWStr)] string htmlContent);

    void add_NavigationStarting();
    void remove_NavigationStarting();
    void add_ContentLoading();
    void remove_ContentLoading();
    void add_SourceChanged();
    void remove_SourceChanged();
    void add_HistoryChanged();
    void remove_HistoryChanged();
    void add_NavigationCompleted(ICoreWebView2NavigationCompletedEventHandler handler, ref long token);
    void remove_NavigationCompleted(long token);
    void add_FrameNavigationStarting();
    void remove_FrameNavigationStarting();
    void add_FrameNavigationCompleted();
    void remove_FrameNavigationCompleted();
    void add_ScriptDialogOpening();
    void remove_ScriptDialogOpening();
    void add_PermissionRequested();
    void remove_PermissionRequested();
    void add_ProcessFailed();
    void remove_ProcessFailed();

    void AddScriptToExecuteOnDocumentCreated([EmitAs(UnmanagedType.LPWStr)] string javaScript,
        ICoreWebView2AddScriptToExecuteOnDocumentCreatedCompletedHandler? handler);
    void RemoveScriptToExecuteOnDocumentCreated([EmitAs(UnmanagedType.LPWStr)] string id);

    void ExecuteScript([EmitAs(UnmanagedType.LPWStr)] string javaScript,
        ICoreWebView2ExecuteScriptCompletedHandler handler);

    void CapturePreview();

    void Reload();
    
    void PostWebMessageAsJson([EmitAs(UnmanagedType.LPWStr)] string webMessageAsJson);
    void PostWebMessageAsString([EmitAs(UnmanagedType.LPWStr)] string webMessageAsString);
    void add_WebMessageReceived();
    void remove_WebMessageReceived();
    void CallDevToolsProtocolMethod();
    
    uint BrowserProcessId { get; }
    
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    void GoBack();
    void GoForward();
    void GetDevToolsProtocolEventReceiver();
    void Stop();
    void add_NewWindowRequested();
    void remove_NewWindowRequested();
    void add_DocumentTitleChanged();
    void remove_DocumentTitleChanged();
    
    [EmitAs(UnmanagedType.LPWStr)] string DocumentTitle { get; }

    void AddHostObjectToScript([EmitAs(UnmanagedType.LPWStr)] string name, object obj);
    void RemoveHostObjectFromScript([EmitAs(UnmanagedType.LPWStr)] string name);
    void OpenDevToolsWindow();
    
    // todo implement rest of definition
}