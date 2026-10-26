using BlazorHybridApp.MacOS.Controls.BlazorWebView;
using BlazorHybridApp.Rcl.Components;

namespace BlazorHybridApp.MacOS.Views;

internal sealed class MainWindow : NSWindow
{
    private readonly BlazorWebView _blazorWebView;
    private bool _isWebViewDisposed;

    internal MainWindow(IServiceProvider serviceProvider)
        : base(
            new CGRect(0, 0, 1024, 768),
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable,
            NSBackingStore.Buffered,
            false)
    {
        Title = "BlazorHybridApp";
        ReleasedWhenClosed = false;

        _blazorWebView = new BlazorWebView(serviceProvider, "wwwroot/index.html", typeof(Main), "#app");
        ContentView = _blazorWebView.View;
        WindowShouldClose = OnWindowShouldClose;
        Center();

        _blazorWebView.Start();
    }

    private bool OnWindowShouldClose(NSObject sender)
    {
        if (_isWebViewDisposed)
        {
            return true;
        }

        DisposeWebViewAndCloseAsync();
        return false;
    }

    private async void DisposeWebViewAndCloseAsync()
    {
        try
        {
            await _blazorWebView.DisposeAsync();
        }
        finally
        {
            _isWebViewDisposed = true;
            Close();
        }
    }
}
