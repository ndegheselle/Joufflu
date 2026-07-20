---
title: Application shell
parent: Toolkit
nav_order: 5
---

# Application shell

These styles shape the whole window rather than a control on a page. The
`Joufflu.Samples` gallery window itself is the live example.

## Window

The default `Window` style themes a standard WPF window (background, foreground
and native chrome) to match the design system. It is applied implicitly to every
`Window`.

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

Set `PlaceTitleBarOverContent="True"` to draw the window content beneath a
transparent title bar instead of below it. A full-height side panel's background
then reaches the very top of the window, while the caption buttons keep floating
at the top-right:

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

{: .note }
> Because the content now sits under the title bar, offset the top of your content
> so nothing hides behind the floating caption buttons — bind a top margin/padding
> to the read-only `TitleBarActualHeight` property. The `Joufflu.Samples` gallery
> window uses this exact setup as its live example.

## NavigationContainer

`NavigationContainer` hosts the current page and layers overlays and toasts above
it. Pair it with a `NavigationMenu`, driving both from a shared `Navigator`.

```xml
<nav:NavigationContainer Navigator="{Binding Navigator}"
                         Overlays="{Binding Overlays}"
                         Toasts="{Binding Toasts}" />
```
