using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Navigation;

namespace Joufflu.Samples.Views.Controls.Navigation;

public class NavigationMenuSamplesViewModel : ObservableObject
{
    /// <summary>A standalone navigator so the demo menu can show selection + navigation.</summary>
    public Navigator DemoNavigator { get; } = new();

    /// <summary>Demo pages keyed by the menu items' text targets.</summary>
    private readonly Dictionary<string, object> _pages = new()
    {
        ["Home"] = "Home page",
        ["Inbox"] = "Inbox page",
        ["Settings"] = "Settings page",
    };

    /// <summary>Bound to <c>NavigationMenu.TargetResolver</c>; turns a text target into a page.</summary>
    public Func<string, object?> ResolveTarget { get; }

    public NavigationMenuSamplesViewModel()
    {
        ResolveTarget = ResolvePage;

        if (ResolvePage("Home") is { } home)
            DemoNavigator.Navigate(home);
    }

    private object? ResolvePage(string target) =>
        _pages.TryGetValue(target, out object? page) ? page : null;

    public string Code =>
        "<nav:NavigationMenu Navigator=\"{Binding DemoNavigator}\"\n" +
        "                    TargetResolver=\"{Binding ResolveTarget}\">\n" +
        "    <nav:NavigationTitle Title=\"Demo\" />\n" +
        "    <nav:NavigationItem Target=\"Home\">\n" +
        "        <nav:NavigationItem.Icon>\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Home}\" />\n" +
        "        </nav:NavigationItem.Icon>\n" +
        "        Home\n" +
        "    </nav:NavigationItem>\n" +
        "    <!-- … more items … -->\n" +
        "</nav:NavigationMenu>";
}
