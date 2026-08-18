---
title: Navigation menu
parent: Navigation
nav_order: 1
---

# Navigation menu

## Collapsible side menu

The collapsible side menu plugged into its own `Navigator`. The chevron collapses
it to an icons-only rail. A `NavigationGroup` displays like an item but expands to
reveal children, and groups can nest.

When collapsed, each item and group hides its label and surfaces it as a
right-placed [tooltip](../toolkit/tooltip.md) on hover, keeping the icons-only
rail discoverable.

```xml
<nav:NavigationMenu Navigator="{Binding DemoNavigator}">
    <nav:NavigationTitle>Demo</nav:NavigationTitle>
    <!-- An item targets the type of the page it navigates to -->
    <nav:NavigationItem TargetType="{x:Type vm:HomeViewModel}">
        <nav:NavigationItem.Icon>
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Home}" />
        </nav:NavigationItem.Icon>
        Home
    </nav:NavigationItem>
    <!-- A group displays like an item but expands to reveal children -->
    <nav:NavigationGroup Header="Parent">
        <nav:NavigationItem TargetType="{x:Type vm:Submenu1ViewModel}">Submenu 1</nav:NavigationItem>
        <nav:NavigationItem TargetType="{x:Type vm:Submenu2ViewModel}">Submenu 2</nav:NavigationItem>
        <!-- Groups can nest -->
        <nav:NavigationGroup Header="Parent">
            <!-- … more items … -->
        </nav:NavigationGroup>
    </nav:NavigationGroup>
</nav:NavigationMenu>
```

## Resolving a target type to a page

An item's `TargetType` is the **type** of the page (view model) it navigates to.
The `Navigator` turns that type into the page instance through the resolver it is
built with — usually a lookup in the shell view model's page registry:

```csharp
// Pages keyed by their own type, which is what the menu items target.
private readonly Dictionary<Type, object> _pages = new object[]
{
    new HomeViewModel(),
    new Submenu1ViewModel(),
    new Submenu2ViewModel(),
}.ToDictionary(page => page.GetType());

public Navigator Navigator { get; }

public ShellViewModel()
{
    Navigator = new Navigator(target => _pages.GetValueOrDefault(target));
}
```

An item shows as selected while the current page is of its `TargetType`, so give
each menu entry its own page type. Returning `null` from the resolver (an unknown
type) leaves the current page in place.

Navigating from code takes either form — a type, resolved the same way, or a page
instance directly:

```csharp
Navigator.Navigate(typeof(HomeViewModel));
Navigator.Navigate(new HomeViewModel());
```
