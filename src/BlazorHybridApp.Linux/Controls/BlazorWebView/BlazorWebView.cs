using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.FileProviders;
using WebKit;

namespace BlazorHybridApp.Linux.Controls.BlazorWebView;

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

    internal static readonly Uri AppOriginUri = new($"{Scheme}://localhost/");

    private readonly BlazorWebViewManager _webViewManager;
    private readonly AppSchemeHandler _appSchemeHandler;

    internal BlazorWebView(IServiceProvider serviceProvider, string hostPage, Type rootComponentType, string rootComponentSelector)
    {
        View = CreateWebView();

        string contentRootDir = Path.GetDirectoryName(hostPage) ?? string.Empty;
        string hostPageRelativePath = Path.GetRelativePath(contentRootDir, hostPage);
        var fileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, contentRootDir));

        _webViewManager = new BlazorWebViewManager(View, serviceProvider, new GLibDispatcher(), AppOriginUri, fileProvider, hostPageRelativePath);
        _appSchemeHandler = new AppSchemeHandler(_webViewManager, AppOriginUri);
        _ = _webViewManager.AddRootComponentAsync(rootComponentType, rootComponentSelector, ParameterView.Empty);

        View.WebContext!.RegisterUriScheme(Scheme, _appSchemeHandler.HandleRequest);
    }

    internal WebView View { get; }

    internal void Start()
    {
        _webViewManager.Navigate("/");
    }

    public async ValueTask DisposeAsync()
    {
        await _webViewManager.DisposeAsync();
    }

    private WebView CreateWebView()
    {
        var webView = WebView.New();

        Settings settings = webView.GetSettings();
#if DEBUG
        settings.EnableDeveloperExtras = true;
#endif
        // WebKitGTK renders a blank or corrupted page with GPU compositing on some drivers (DevToys issue #1234).
        settings.HardwareAccelerationPolicy = HardwareAccelerationPolicy.Never;
        webView.SetSettings(settings);

        UserContentManager userContentManager = webView.GetUserContentManager();
        UserContentManager.ScriptMessageReceivedSignal.Connect(
            userContentManager,
            OnScriptMessageReceived,
            after: false,
            detail: MessageHandlerName);
        userContentManager.RegisterScriptMessageHandler(MessageHandlerName, null);
        userContentManager.AddScript(
            UserScript.New(
                ExternalBridgeScript,
                UserContentInjectedFrames.TopFrame,
                UserScriptInjectionTime.Start,
                null,
                null));

        webView.OnDecidePolicy += OnDecidePolicy;

        return webView;
    }

    private void OnScriptMessageReceived(UserContentManager sender, UserContentManager.ScriptMessageReceivedSignalArgs args)
    {
        _webViewManager.OnMessageReceived(AppOriginUri, args.Value.ToString());
    }

    private bool OnDecidePolicy(WebView sender, WebView.DecidePolicySignalArgs args)
    {
        if (args.Decision is not NavigationPolicyDecision navigationDecision)
        {
            return false;
        }

        string? uri = navigationDecision.GetNavigationAction().GetRequest().GetUri();
        bool isNewWindow = args.DecisionType == PolicyDecisionType.NewWindowAction;
        if (uri is null || (!isNewWindow && AppOriginUri.IsBaseOf(new Uri(uri))))
        {
            return false;
        }

        Gio.AppInfoHelper.LaunchDefaultForUri(uri, null);
        navigationDecision.Ignore();
        return true;
    }
}
