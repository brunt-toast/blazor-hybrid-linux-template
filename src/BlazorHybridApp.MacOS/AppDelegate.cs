using BlazorHybridApp.MacOS.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorHybridApp.MacOS;

[Register("AppDelegate")]
public sealed class AppDelegate : NSApplicationDelegate
{
    private MainWindow? _mainWindow;

    public override void DidFinishLaunching(NSNotification notification)
    {
        NSApplication.SharedApplication.MainMenu = new MainMenu();

        _mainWindow = new MainWindow(CreateServiceProvider());
        _mainWindow.MakeKeyAndOrderFront(this);
    }

    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
    {
        return true;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddBlazorWebView();
        return services.BuildServiceProvider();
    }
}
