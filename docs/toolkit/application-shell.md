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

## NavigationContainer

`NavigationContainer` hosts the current page and layers overlays and toasts above
it. Pair it with a `NavigationMenu`, driving both from a shared `Navigator`.

```xml
<nav:NavigationContainer Navigator="{Binding Navigator}"
                         Overlays="{Binding Overlays}"
                         Toasts="{Binding Toasts}" />
```
