using WebKit;

namespace BlazorHybridApp.MacOS.Controls.BlazorWebView;

internal sealed class NavigationDelegate : WKNavigationDelegate
{
    private readonly Uri _appOriginUri;

    internal NavigationDelegate(Uri appOriginUri)
    {
        _appOriginUri = appOriginUri;
    }

    public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
    {
        NSUrl url = navigationAction.Request.Url;
        bool isNewWindow = navigationAction.TargetFrame is null;

        if (url.AbsoluteString is null || (!isNewWindow && _appOriginUri.IsBaseOf(new Uri(url.AbsoluteString))))
        {
            decisionHandler(WKNavigationActionPolicy.Allow);
            return;
        }

        NSWorkspace.SharedWorkspace.OpenUrl(url);
        decisionHandler(WKNavigationActionPolicy.Cancel);
    }
}
