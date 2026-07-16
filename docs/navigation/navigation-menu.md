---
title: Navigation menu
parent: Navigation
nav_order: 1
---

# Navigation menu

## Collapsible side menu

The collapsible side menu plugged into its own `Navigator`. Use the chevron to
collapse it to an icons-only rail. A `NavigationGroup` displays like an item but
expands to reveal children, and groups can nest.

```xml
<nav:NavigationMenu Navigator="{Binding DemoNavigator}"
                    TargetResolver="{Binding ResolveTarget}">
    <nav:NavigationTitle Title="Demo" />
    <nav:NavigationItem Target="Home">
        <nav:NavigationItem.Icon>
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Home}" />
        </nav:NavigationItem.Icon>
        Home
    </nav:NavigationItem>
    <!-- A group displays like an item but expands to reveal children -->
    <nav:NavigationGroup Header="Parent">
        <nav:NavigationItem Target="Submenu 1">Submenu 1</nav:NavigationItem>
        <nav:NavigationItem Target="Submenu 2">Submenu 2</nav:NavigationItem>
        <!-- Groups can nest -->
        <nav:NavigationGroup Header="Parent">
            <!-- … more items … -->
        </nav:NavigationGroup>
    </nav:NavigationGroup>
</nav:NavigationMenu>
```

`TargetResolver` is a `Func<string, object?>` that maps each item's `Target`
string to the page (or view model) to navigate to.
