---
title: Application shell
parent: Toolkit
nav_order: 6
---

# Application shell

Styles that shape the whole window rather than a control on a page. The
`Joufflu.Samples` gallery window is the live example.

## Window

The default `Window` style themes a standard WPF window (background, foreground,
native chrome) to match the design system. Applied implicitly to every `Window`.

```xml
<Window ...>
    <!-- inherits the themed Window style automatically -->
</Window>
```

## ThemedWindow

`ThemedWindow` is a custom window with a fully styled title bar and caption
buttons.

```xml
<controls:ThemedWindow xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
                       Title="My app">
    ...
</controls:ThemedWindow>
```

### Title bar over content

Set `PlaceTitleBarOverContent="True"` to draw content beneath a transparent title
bar instead of below it. A full-height side panel's background then reaches the
top of the window, while the caption buttons keep floating top-right:

```xml
<controls:ThemedWindow ...
    PlaceTitleBarOverContent="True"
    IconVisibility="Collapsed">
    ...
</controls:ThemedWindow>
```

`IconVisibility="Collapsed"` hides the title-bar icon and `TitleVisibility="Collapsed"`
hides the title text — set both to clear the top-left corner for the side panel's
own header.

#### Keeping content clear of the bar

**Why.** In this mode the title bar spans the full window width and stays
draggable across its whole surface — that is what lets you drag the window from
anywhere along the top. But the bar is drawn *over* the content, and though
transparent its drag surface is hit-test visible: any control in the top strip is
covered by it. A side-panel collapse button becomes unclickable; a page's
vertical scrollbar runs behind the caption buttons.

**How.** Reserve a strip of empty space at the top of your content equal to the
bar's height. That height is a fixed value shared through the `Dimensions.TitleBarHeight`
resource, so build a top-only `Thickness` from it and set it as the `Margin` of the
panel that reaches the top:

```xml
<controls:ThemedWindow ...
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
    PlaceTitleBarOverContent="True">

    <controls:ThemedWindow.Resources>
        <!-- Offsets the content below the title bar drawn over it. -->
        <Thickness x:Key="ContentTitleBarMargin"
                   Top="{StaticResource {x:Static joufflu:Dimensions.TitleBarHeight}}" />
    </controls:ThemedWindow.Resources>

    <DockPanel>
        <nav:NavigationMenu DockPanel.Dock="Left" ... />

        <!-- The container drops below the bar; overlays and toasts stay full-bleed. -->
        <nav:NavigationContainer
            Margin="{StaticResource ContentTitleBarMargin}"
            ... />
    </DockPanel>
</controls:ThemedWindow>
```

The `Joufflu.Samples` gallery window uses this setup. Because the height is a shared
resource, the offset always matches the title bar even if that height changes.

{: .note }
> Offset only the panels whose top strip holds interactive content — a hosted page
> and its scrollbar. The `NavigationContainer` insets the page while leaving overlays
> and toasts full-bleed, so modal backdrops still cover the whole window.

## NavigationContainer

Hosts the current page and layers overlays and toasts above it. Pair it with a
`NavigationMenu`, driving both from a shared `Navigator`.

```xml
<nav:NavigationContainer Navigator="{Binding Navigator}"
                         Overlays="{Binding Overlays}"
                         Toasts="{Binding Toasts}" />
```
