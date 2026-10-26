using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.FileProviders;
using WebKit;

namespace BlazorHybridApp.MacOS.Controls.BlazorWebView;

internal sealed class BlazorWebView : IAsyncDisposable
{
    private const string Scheme = "app";
    private const string MessageHandlerName = "blazorwebview";

    private const string ExternalBridgeScript
        = $$"""
            window.__receiveMessageCallbacks = [];
            window.__dispatchMessageCallback = function (message) {
                window.__receiveMessageCallbacks.forEach(function (callback) { callback(message); });
            };
            window.external = {
                sendMessage: function (message) {
                    window.webkit.messageHandlers.{{MessageHandlerName}}.postMessage(message);
                },
                receiveMessage: function (callback) {
                    window.__receiveMessageCallbacks.push(callback);
                }
            };
            """;

    // WKWebView on macOS 15 and later rejects 0.0.0.0 as the app host (DevToys issue #1279, dotnet/maui#23390).
    internal static readonly Uri AppOriginUri
        = new($"{Scheme}://{(OperatingSystem.IsMacOSVersionAtLeast(15) ? "localhost" : "0.0.0.0")}/");

    private readonly BlazorWebViewManager _webViewManager;

    internal BlazorWebView(IServiceProvider serviceProvider, string hostPage, Type rootComponentType, string rootComponentSelector)
    {
        View = CreateWebView();

        string contentRootDir = Path.GetDirectoryName(hostPage) ?? string.Empty;
        string hostPageRelativePath = Path.GetRelativePath(contentRootDir, hostPage);
        var fileProvider = new PhysicalFileProvider(Path.Combine(NSBundle.MainBundle.ResourcePath, contentRootDir));

        _webViewManager = new BlazorWebViewManager(View, serviceProvider, new AppKitDispatcher(), AppOriginUri, fileProvider, hostPageRelativePath);
        _ = _webViewManager.AddRootComponentAsync(rootComponentType, rootComponentSelector, ParameterView.Empty);
    }

    internal WKWebView View { get; }

    internal void Start()
    {
        _webViewManager.Navigate("/");
    }

    internal bool TryGetResponse(
        string uri,
        bool allowFallbackOnHostPage,
        out int statusCode,
        out string statusMessage,
        out Stream content,
        out IDictionary<string, string> headers)
    {
        return _webViewManager.TryGetResponse(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
    }

    public async ValueTask DisposeAsync()
    {
        await _webViewManager.DisposeAsync();
    }

    private WKWebView CreateWebView()
    {
        var configuration = new WKWebViewConfiguration();
        configuration.UserContentController.AddScriptMessageHandler(new ScriptMessageHandler(OnScriptMessageReceived), MessageHandlerName);
        configuration.UserContentController.AddUserScript(
            new WKUserScript(new NSString(ExternalBridgeScript), WKUserScriptInjectionTime.AtDocumentStart, isForMainFrameOnly: true));
        configuration.SetUrlSchemeHandler(new AppSchemeHandler(this, AppOriginUri), Scheme);

        var webView = new WKWebView(CGRect.Empty, configuration)
        {
            NavigationDelegate = new NavigationDelegate(AppOriginUri)
        };
#if DEBUG
        if (OperatingSystem.IsMacOSVersionAtLeast(13, 3))
        {
            webView.Inspectable = true;
        }
#endif
        return webView;
    }

    private void OnScriptMessageReceived(string message)
    {
        _webViewManager.OnMessageReceived(AppOriginUri, message);
    }
}
