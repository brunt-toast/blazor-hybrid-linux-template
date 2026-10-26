using BlazorHybridApp.Linux.Controls.BlazorWebView;
using BlazorHybridApp.Rcl.Components;

namespace BlazorHybridApp.Linux.Views;

internal sealed class MainWindow
{
    private readonly BlazorWebView _blazorWebView;
    private readonly Gtk.ApplicationWindow _window;
    private bool _isWebViewDisposed;

    internal MainWindow(Gtk.Application application, IServiceProvider serviceProvider)
    {
        _blazorWebView = new BlazorWebView(serviceProvider, "wwwroot/index.html", typeof(Main), "#app");

        _window = Gtk.ApplicationWindow.New(application);
        _window.Title = "BlazorHybridApp";
        _window.SetDefaultSize(1024, 768);
        _window.SetChild(_blazorWebView.View);
        _window.OnCloseRequest += OnCloseRequest;
        _window.Present();

        _blazorWebView.Start();
    }

    private bool OnCloseRequest(Gtk.Window sender, EventArgs args)
    {
        if (_isWebViewDisposed)
        {
            return false;
        }

        DisposeWebViewAndCloseAsync();
        return true;
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
            _window.Close();
        }
    }
}
