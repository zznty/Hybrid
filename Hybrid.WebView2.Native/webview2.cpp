#include <windows.h>
#include <string>
#include <tchar.h>
#include <wrl.h>
#include <wil/com.h>
// <IncludeHeader>
// include WebView2 header
#include "WebView2.h"
// </IncludeHeader>

using namespace Microsoft::WRL;

// Pointer to WebViewController
static wil::com_ptr<ICoreWebView2Controller> webviewController;

// Pointer to WebView window
static wil::com_ptr<ICoreWebView2> webview;

extern "C" {
    __declspec(dllexport) HRESULT Create(HWND hWnd, LPCWSTR uri, LPCWSTR userDataFolder, void(*initializedCallback)())
    {
        return CreateCoreWebView2EnvironmentWithOptions(nullptr, userDataFolder, nullptr,
			Callback<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler>(
				[hWnd, uri, userDataFolder, initializedCallback](HRESULT result, ICoreWebView2Environment* env) -> HRESULT {

					RETURN_IF_FAILED(result);

					// Create a CoreWebView2Controller and get the associated CoreWebView2 whose parent is the main window hWnd
					env->CreateCoreWebView2Controller(hWnd, Callback<ICoreWebView2CreateCoreWebView2ControllerCompletedHandler>(
						[hWnd, uri, userDataFolder, initializedCallback](HRESULT result, ICoreWebView2Controller* controller) -> HRESULT {

							RETURN_IF_FAILED(result);
							
							if (controller != nullptr) {
								webviewController = controller;
								webviewController->get_CoreWebView2(&webview);
							}

							wil::com_ptr<ICoreWebView2Settings> settings;
							webview->get_Settings(&settings);
							settings->put_IsStatusBarEnabled(FALSE);
							settings->put_IsZoomControlEnabled(FALSE);


							// Resize WebView to fit the bounds of the parent window
							RECT bounds;
							GetClientRect(hWnd, &bounds);
							webviewController->put_Bounds(bounds);

							webview->Navigate(uri);

							initializedCallback();

							return S_OK;
						}).Get());
					return S_OK;
				}).Get());
    }

	__declspec(dllexport) HRESULT Resize(const HWND hWnd)
	{
    	if (!webviewController) return S_OK;
    	
		RECT bounds;
		GetClientRect(hWnd, &bounds);
		return webviewController->put_Bounds(bounds);
	}

	__declspec(dllexport) HRESULT AddHostObject(LPCWSTR hostObjectName, IUnknown* hostObject)
	{
		if (!webview) return E_NOT_VALID_STATE;
		
		VARIANT hostObjectVariant;
    	VariantInit(&hostObjectVariant);

		hostObjectVariant.vt = VT_UNKNOWN;
		hostObjectVariant.punkVal = hostObject;
    	
		return webview->AddHostObjectToScript(hostObjectName, &hostObjectVariant);
	}

	__declspec(dllexport) HRESULT AddStartupScript(const LPCWSTR script)
	{
		if (!webview) return E_NOT_VALID_STATE;
		
		return webview->AddScriptToExecuteOnDocumentCreated(script, nullptr);
	}
}