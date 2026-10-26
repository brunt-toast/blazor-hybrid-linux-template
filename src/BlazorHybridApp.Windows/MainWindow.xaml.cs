using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorHybridApp.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        BlazorWebView.Services = CreateServiceProvider();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        return services.BuildServiceProvider();
    }
}
