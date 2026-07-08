using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Assets.Fonts;
using Joufflu.Navigation;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.Views.Controls.Navigation;

public class NavigationMenuSamplesViewModel : ObservableObject
{
    /// <summary>A standalone navigator so the demo menu can show selection + navigation.</summary>
    public Navigator DemoNavigator { get; } = new();

    public ObservableCollection<NavigationMenuEntry> MenuItems { get; } = new();

    public NavigationMenuSamplesViewModel()
    {
        var home = new NavigationMenuItem { Icon = LucideFontIcons.Home, Title = "Home", Target = "Home page" };
        var inbox = new NavigationMenuItem { Icon = LucideFontIcons.Bell, Title = "Inbox", Target = "Inbox page" };
        var settings = new NavigationMenuItem { Icon = LucideFontIcons.Settings, Title = "Settings", Target = "Settings page" };

        MenuItems.Add(new NavigationMenuTitle("Demo"));
        MenuItems.Add(home);
        MenuItems.Add(inbox);
        MenuItems.Add(settings);

        DemoNavigator.Navigate(home.Target!);
    }

    public string Code =>
        "<nav:NavigationMenu Navigator=\"{Binding DemoNavigator}\"\n" +
        "                    ItemsSource=\"{Binding MenuItems}\" />";
}
