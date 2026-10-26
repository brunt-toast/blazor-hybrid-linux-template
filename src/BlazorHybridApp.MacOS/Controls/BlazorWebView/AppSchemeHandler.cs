using System.Globalization;
using WebKit;

namespace BlazorHybridApp.MacOS.Controls.BlazorWebView;

internal sealed class AppSchemeHandler : NSObject, IWKUrlSchemeHandler
{
    private readonly BlazorWebView _blazorWebView;
    private readonly Uri _appOriginUri;

    internal AppSchemeHandler(BlazorWebView blazorWebView, Uri appOriginUri)
    {
        _blazorWebView = blazorWebView;
        _appOriginUri = appOriginUri;
    }

    [Export("webView:startURLSchemeTask:")]
    public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
    {
        string url = RemoveQueryStringAndFragment(urlSchemeTask.Request.Url.AbsoluteString ?? string.Empty);

        if (!_blazorWebView.TryGetResponse(
                url,
                IsPageUrl(url),
                out int statusCode,
                out _,
                out Stream content,
                out IDictionary<string, string> headers))
        {
            Respond(urlSchemeTask, 404, new Dictionary<string, string>(), []);
            return;
        }

        using var memoryStream = new MemoryStream();
        using (content)
        {
            content.CopyTo(memoryStream);
        }

        Respond(urlSchemeTask, statusCode, headers, memoryStream.ToArray());
    }

    [Export("webView:stopURLSchemeTask:")]
    public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
    {
    }

    private static void Respond(IWKUrlSchemeTask urlSchemeTask, int statusCode, IDictionary<string, string> headers, byte[] body)
    {
        using var headerFields = new NSMutableDictionary<NSString, NSString>();
        foreach (KeyValuePair<string, string> header in headers)
        {
            headerFields[(NSString)header.Key] = (NSString)header.Value;
        }

        headerFields[(NSString)"Content-Length"] = (NSString)body.Length.ToString(CultureInfo.InvariantCulture);

        using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", headerFields);
        urlSchemeTask.DidReceiveResponse(response);
        urlSchemeTask.DidReceiveData(NSData.FromArray(body));
        urlSchemeTask.DidFinish();
    }

    private static string RemoveQueryStringAndFragment(string url)
    {
        int index = url.IndexOfAny(['?', '#']);
        return index == -1 ? url : url[..index];
    }

    private bool IsPageUrl(string url)
    {
        return !Path.HasExtension(url) && _appOriginUri.IsBaseOf(new Uri(url));
    }
}
