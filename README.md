# BlazorHybridApp

A Blazor Hybrid desktop app for Windows, macOS and Linux. 

## Use as a template

```
dotnet new install .
dotnet new blazorhybrid-desktop -n MyApp
```

## Prerequisites

- .NET 10 SDK
- Windows: WebView2 runtime (included in Windows 11)
- macOS: Xcode and the macOS workload (`sudo dotnet workload install macos`)
- Linux: GTK 4 and WebKitGTK 6.0
  - Debian/Ubuntu: `sudo apt install libgtk-4-1 libwebkitgtk-6.0-4`
  - Fedora: `sudo dnf install gtk4 webkitgtk6.0`
  - Arch: `sudo pacman -S gtk4 webkitgtk-6.0`

## Known issues

- **WSL:** WebKitGTK can stop when it starts its sandboxed web process. For local testing only, set `WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1`. Do not use this setting in production.
- **Linux:** the Linux host turns off GPU acceleration in WebKitGTK, because some drivers show a blank page.
- **Developer tools:** Debug builds enable the web inspector on all three platforms.
  - Windows and Linux: right-click the page and select **Inspect** or **Inspect Element**.
  - macOS: in Safari, open the **Develop** menu and select the app.
