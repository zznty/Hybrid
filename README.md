# Hybrid (.NET AOT + WebViews

## The problem

As many of you know, there is no ultimate desktop (mobile in the future?) cross-platform framework.

Qt, Uno, Avalonia,
MAUI are all falling into a developer UX pit where just developing for the chosen platform is already full of troubles,
limitations from the lack of cross-platform support
(Media, Effects, Storage) and if you would even think about shipping an AOTed application,
sometimes almost impossible.

## The solution

I wanted to create a framework that would let me overcome these limitations (or most of them) with minimal effort.

As the trend for electron applications is raising,
HTML/JS/CSS becomes the new standard for cross-platform apps that are straightforward to design and develop,
I have decided to go with platform-provided or shipped webview implementations (Chromium/Edge, WebKit),
but also utilize c# as a language for the platform code (which is not sandboxed like the rest of the frontend).

Instead of creating some obscure wrappers for interop between .NET and JS,
I wrote a hybrid framework that uses direct interop provided by specific webview implementation:
- In the case of WebView2, it is DCOM (OLE Automation)
- For Ultralight and other webkit-based implementations, it is Direct JavaScriptCore Calls

Both of these technologies would provide you with a rich set of features for native OOP patterns such as Reference Tracking and Garbage Collection.

Writing your api is as simple as creating an interface then implementation it

```csharp
public interface ICalculator
{
    int Add(int a, int b);
}

public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

And in js you call the `Add` method from the instance of `ICalculator` from DI container (or other sources):
```js
chrome.webview.hostObjects.calculator.add(1, 2) // returns 3
```

## Roadmap

- [ ] Emit typescript definitions
- [ ] WebViews
  - [x] WebView2 (Windows only)
  - [ ] WebKit webview (OSX only)
  - [ ] Ultralight (Windows/OSX)
- [ ] COM
  - [ ] Get rid of cpp wrapper for COM stuff (write ComSourceGenerator-compatible C# interfaces)
  - [ ] Get rid of asm dyncalls for OLE Dispatch
  - [ ] Add support for events marshaling
  - [ ] Add support for callbacks marshaling
  - [ ] Provide built-in marshal of managed Task to js Promise
- [ ] Ultralight
  - [ ] Add marshaller WebCore->OLE Dispatch (think about a portable portion of COM/OLE Dispatch)
    - ComWrappers api is intended to provide portable COM support 
  - [ ] GL or Vk renderer
- [ ] Static link into a single binary
  - [ ] WebView2Loader (msft provides .lib)
  - [ ] cpp wrapper lib
  - [ ] Ultralight libs
  - [ ] glfw from Silk.NET