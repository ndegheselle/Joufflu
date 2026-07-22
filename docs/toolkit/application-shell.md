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
bar's height. The window exposes its measured height as the read-only
`TitleBarActualHeight` property, which is **0 unless `PlaceTitleBarOverContent` is
on** — so the same binding adds no gap when you switch back to a stacked title
bar. The offset is automatic and optional.

Convert that height to a top-only `Thickness` with `TopThicknessConverter`
(`0, height, 0, 0`) and bind it to the `Padding` of the panels that reach the top:

```xml
<controls:ThemedWindow ...
    xmlns:converters="clr-namespace:Joufflu.Converters;assembly=Joufflu"
    PlaceTitleBarOverContent="True">

    <controls:ThemedWindow.Resources>
        <converters:TopThicknessConverter x:Key="TopThickness" />
    </controls:ThemedWindow.Resources>

    <DockPanel>
        <!-- The menu insets its whole column, so the collapse button drops below the bar
             while the panel background still fills to the very top. -->
        <nav:NavigationMenu DockPanel.Dock="Left"
            Padding="{Binding TitleBarActualHeight,
                              RelativeSource={RelativeSource AncestorType={x:Type controls:ThemedWindow}},
                              Converter={StaticResource TopThickness}}"
            ... />

        <!-- The container insets only the page (and its scrollbar); overlays and toasts stay full-bleed. -->
        <nav:NavigationContainer
            Padding="{Binding TitleBarActualHeight,
                              RelativeSource={RelativeSource AncestorType={x:Type controls:ThemedWindow}},
                              Converter={StaticResource TopThickness}}"
            ... />
    </DockPanel>
</controls:ThemedWindow>
```

Using `Padding` (not `Margin`) keeps each panel's background edge-to-edge under
the transparent bar; only the inner content is pushed down. The `Joufflu.Samples`
gallery window uses this setup.

{: .note }
> `NavigationMenu` and `NavigationContainer` honour `Padding` for this purpose:
> the menu insets its entire column (header and collapse button included), while
> the container insets the hosted page only — overlays and toasts stay full-bleed
> so modal backdrops still cover the whole window.

## NavigationContainer

Hosts the current page and layers overlays and toasts above it. Pair it with a
`NavigationMenu`, driving both from a shared `Navigator`.

```xml
<nav:NavigationContainer Navigator="{Binding Navigator}"
                         Overlays="{Binding Overlays}"
                         Toasts="{Binding Toasts}" />
```
