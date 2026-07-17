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
        ["Submenu 1"] = "Submenu 1 page",
        ["Submenu 2"] = "Submenu 2 page",
        ["Nested submenu 1"] = "Nested submenu 1 page",
        ["Nested submenu 2"] = "Nested submenu 2 page",
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
        "    <!-- Optional header slot, hidden when collapsed -->\n" +
        "    <nav:NavigationMenu.Header>\n" +
        "        <StackPanel Orientation=\"Horizontal\" joufflu:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Rocket}\" />\n" +
        "            <TextBlock Text=\"Joufflu\" />\n" +
        "        </StackPanel>\n" +
        "    </nav:NavigationMenu.Header>\n" +
        "    <nav:NavigationTitle Title=\"Demo\" />\n" +
        "    <nav:NavigationItem Target=\"Home\">\n" +
        "        <nav:NavigationItem.Icon>\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Home}\" />\n" +
        "        </nav:NavigationItem.Icon>\n" +
        "        Home\n" +
        "    </nav:NavigationItem>\n" +
        "    <!-- A group displays like an item but expands to reveal children -->\n" +
        "    <nav:NavigationGroup Header=\"Parent\">\n" +
        "        <nav:NavigationItem Target=\"Submenu 1\">Submenu 1</nav:NavigationItem>\n" +
        "        <nav:NavigationItem Target=\"Submenu 2\">Submenu 2</nav:NavigationItem>\n" +
        "        <!-- Groups can nest -->\n" +
        "        <nav:NavigationGroup Header=\"Parent\">\n" +
        "            <!-- … more items … -->\n" +
        "        </nav:NavigationGroup>\n" +
        "    </nav:NavigationGroup>\n" +
        "</nav:NavigationMenu>";
}
