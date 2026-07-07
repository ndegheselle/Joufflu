using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Assets.Fonts;
using Joufflu.Navigation;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.ViewModels;

public class CustomControlsViewModel : ObservableObject
{
    /// <summary>A standalone navigator so the demo menu can show selection + navigation.</summary>
    public Navigator DemoNavigator { get; } = new();

    public ObservableCollection<NavigationMenuEntry> MenuItems { get; } = new();

    public CustomControlsViewModel()
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

    public string FontIconCode =>
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<!-- Size flows from the inherited ControlProperties.Size -->\n" +
        "<fonts:FontIcon joufflu:ControlProperties.Size=\"lg\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />";

    public string BadgeCode =>
        "<controls:Badge Variant=\"Success\">Active</controls:Badge>\n" +
        "<controls:Badge Variant=\"Danger\" joufflu:ControlProperties.Size=\"sm\">3</controls:Badge>";

    public string SpinnerCode =>
        "<controls:Spinner />\n" +
        "<controls:Spinner joufflu:ControlProperties.Size=\"lg\" />";

    public string MenuCode =>
        "<nav:NavigationMenu Navigator=\"{Binding DemoNavigator}\"\n" +
        "                    ItemsSource=\"{Binding MenuItems}\" />";
}
