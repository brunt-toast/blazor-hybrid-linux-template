using WebKit;

namespace BlazorHybridApp.MacOS.Controls.BlazorWebView;

internal sealed class ScriptMessageHandler : NSObject, IWKScriptMessageHandler
{
    private readonly Action<string> _messageReceived;

    internal ScriptMessageHandler(Action<string> messageReceived)
    {
        _messageReceived = messageReceived;
    }

    public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
    {
        _messageReceived(((NSString)message.Body).ToString());
    }
}
