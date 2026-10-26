using ObjCRuntime;

namespace BlazorHybridApp.MacOS.Views;

internal sealed class MainMenu : NSMenu
{
    internal MainMenu()
    {
        var applicationMenu = new NSMenu();
        applicationMenu.AddItem(new NSMenuItem("Quit BlazorHybridApp", new Selector("terminate:"), "q"));
        AddItem(new NSMenuItem { Submenu = applicationMenu });

        var editMenu = new NSMenu("Edit");
        editMenu.AddItem(new NSMenuItem("Cut", new Selector("cut:"), "x"));
        editMenu.AddItem(new NSMenuItem("Copy", new Selector("copy:"), "c"));
        editMenu.AddItem(new NSMenuItem("Paste", new Selector("paste:"), "v"));
        editMenu.AddItem(new NSMenuItem("Select All", new Selector("selectAll:"), "a"));
        AddItem(new NSMenuItem { Submenu = editMenu });
    }
}
