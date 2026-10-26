using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using WebKit;

namespace BlazorHybridApp.Linux.Controls.BlazorWebView;

internal sealed class BlazorWebViewManager : WebViewManager
{
    private readonly WebView _webView;

    internal BlazorWebViewManager(
        WebView webView,
        IServiceProvider provider,
        Dispatcher dispatcher,
        Uri appBaseUri,
        IFileProvider fileProvider,
        string hostPageRelativePath)
        : base(provider, dispatcher, appBaseUri, fileProvider, new JSComponentConfigurationStore(), hostPageRelativePath)
    {
        _webView = webView;
    }

    internal bool TryGetResponse(
        string uri,
        bool allowFallbackOnHostPage,
        out int statusCode,
        out string statusMessage,
        out Stream content,
        out IDictionary<string, string> headers)
    {
        return TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
    }

    internal void OnMessageReceived(Uri sourceUri, string message)
    {
        MessageReceived(sourceUri, message);
    }

    protected override void NavigateCore(Uri absoluteUri)
    {
        _webView.LoadUri(absoluteUri.ToString());
    }

    protected override void SendMessage(string message)
    {
        string script = $"__dispatchMessageCallback(\"{JavaScriptEncoder.Default.Encode(message)}\")";
        _ = _webView.EvaluateJavascriptAsync(script);
    }
}
