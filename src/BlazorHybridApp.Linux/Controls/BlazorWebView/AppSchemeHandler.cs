using WebKit;

namespace BlazorHybridApp.Linux.Controls.BlazorWebView;

internal sealed class AppSchemeHandler
{
    private readonly BlazorWebViewManager _webViewManager;
    private readonly Uri _appOriginUri;

    internal AppSchemeHandler(BlazorWebViewManager webViewManager, Uri appOriginUri)
    {
        _webViewManager = webViewManager;
        _appOriginUri = appOriginUri;
    }

    internal void HandleRequest(URISchemeRequest request)
    {
        string url = RemoveQueryStringAndFragment(request.GetUri());

        if (!_webViewManager.TryGetResponse(
                url,
                IsPageUrl(url),
                out int statusCode,
                out string statusMessage,
                out Stream content,
                out IDictionary<string, string> headers))
        {
            Respond(request, 404, "Not Found", new Dictionary<string, string>(), []);
            return;
        }

        using var memoryStream = new MemoryStream();
        using (content)
        {
            content.CopyTo(memoryStream);
        }

        Respond(request, statusCode, statusMessage, headers, memoryStream.ToArray());
    }

    private static void Respond(
        URISchemeRequest request,
        int statusCode,
        string statusMessage,
        IDictionary<string, string> headers,
        byte[] body)
    {
        using GLib.Bytes bytes = GLib.Bytes.New(body);
        using Gio.MemoryInputStream stream = Gio.MemoryInputStream.NewFromBytes(bytes);

        Soup.MessageHeaders responseHeaders = Soup.MessageHeaders.New(Soup.MessageHeadersType.Response);
        foreach (KeyValuePair<string, string> header in headers)
        {
            responseHeaders.Append(header.Key, header.Value);
        }

        URISchemeResponse response = URISchemeResponse.New(stream, body.Length);
        response.SetStatus((uint)statusCode, statusMessage);
        response.SetHttpHeaders(responseHeaders);
        if (headers.TryGetValue("Content-Type", out string? contentType))
        {
            response.SetContentType(contentType);
        }

        request.FinishWithResponse(response);
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
