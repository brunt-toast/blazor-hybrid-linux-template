using BlazorHybridApp.Linux.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorHybridApp.Linux;

internal static class Program
{
    private static MainWindow? _mainWindow;

    private static int Main(string[] args)
    {
        WebKit.Module.Initialize();

        ServiceProvider serviceProvider = CreateServiceProvider();

        Gtk.Application application = Gtk.Application.New("com.example.BlazorHybridApp", Gio.ApplicationFlags.NonUnique);
        application.OnActivate += (_, _) => _mainWindow = new MainWindow(application, serviceProvider);

        return application.RunWithSynchronizationContext(args);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddBlazorWebView();
        return services.BuildServiceProvider();
    }
}
