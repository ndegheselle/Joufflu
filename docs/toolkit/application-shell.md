---
title: Application shell
parent: Toolkit
nav_order: 8
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

### Application icon

The title bar shows no icon by default. Add an `AppIcon` to
`ThemedWindow.TitleBarContent` to bring it back: it shows the application's icon (or
the `Source` it is given), a click opens the window's system menu and a double click
closes the window, like a native icon.

```xml
<controls:ThemedWindow ... TitleVisibility="Collapsed">
    <controls:ThemedWindow.TitleBarContent>
        <controls:AppIcon Margin="5,0,0,0" />
    </controls:ThemedWindow.TitleBarContent>
    ...
</controls:ThemedWindow>
```

`TitleBarContent` shares its cell with the title text, so collapse the title or pad it
when both are shown.

### Title bar over content

Set `AllowContentOverTitleBar="True"` to draw content beneath a transparent title
bar instead of below it. A full-height side panel's background then reaches the
top of the window, while the caption buttons keep floating top-right:

```xml
<controls:ThemedWindow ...
    AllowContentOverTitleBar="True"
    TitleVisibility="Collapsed">
    ...
</controls:ThemedWindow>
```

`TitleVisibility="Collapsed"` hides the title text, clearing the top-left corner for
the side panel's own header.

#### Layering

The window is drawn in three layers, bottom to top:

1. the title bar surface (`TitleBarBackground`) and its drag area;
2. the window content;
3. the title, `TitleBarContent` and the caption buttons.

So content can be drawn over the bar, but never over the title bar content or the
caption buttons: they stay visible and clickable.

#### Grabbing the window through the content

The drag area sits *under* the content. A control with a `null` background lets the
mouse through to it, so the window can still be grabbed and moved from the top strip;
a control with a background (even `Transparent`) swallows the click. Leave the
containers around the page (`DockPanel`, `Grid`, `OverlayContainer`, `ToastContainer`...)
without a `Background`, and only paint what must be clickable:

```xml
<!-- Draggable: no Background, the click reaches the title bar -->
<Grid Height="30" />

<!-- Not draggable: the border takes the click -->
<Border Height="30" Background="Transparent" />
```

#### Keeping content clear of the bar

**Compromise.** The window does not lay out your page: content is free to run under
the title bar, so a page's own controls (a header, a scrollbar, a button) can end up
behind the title bar content or the caption buttons. Reserve a strip of empty space
at the top of the content equal to the bar's height, a fixed value shared through
`Dimensions.TitleBarHeight` and `Dimensions.TitleBarHeightOffset`:

```xml
<controls:ThemedWindow ...
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
    AllowContentOverTitleBar="True">

    <feedback:ToastContainer Toasts="{Binding Toasts}">
        <nav:OverlayContainer Overlays="{Binding Overlays}">
            <DockPanel>
                <nav:NavigationMenu DockPanel.Dock="Left" ... />

                <!-- The page drops below the bar; overlays and toasts stay full-bleed. -->
                <ContentControl
                    Margin="{StaticResource {x:Static joufflu:Dimensions.TitleBarHeightOffset}}"
                    Content="{Binding Navigator.CurrentPage}" />
            </DockPanel>
        </nav:OverlayContainer>
    </feedback:ToastContainer>
</controls:ThemedWindow>
```

The `Joufflu.Samples` gallery window uses this setup. Because the height is a shared
resource, the offset always matches the title bar even if that height changes.

{: .note }
> Offset only the panels whose top strip holds interactive content, a hosted page
> and its scrollbar. Offsetting the page alone leaves the containers around it
> full-bleed, so modal backdrops still cover the whole window.

`TitleBarContent` is in layer 3 too: give it no background (or handle the drag
yourself) when the window should stay grabbable through it.

#### NavigationMenu under the bar

A `NavigationMenu` reaching the top of the window leaves its header strip (the
`Header` and the free space beside the collapse button) draggable. The header is not
interactive; the collapse button and the items are.

## OverlayContainer

Wraps the whole application and layers the modal overlay stack above it
(`Joufflu.Navigation`). Because it encapsulates everything — side menu included —
a full screen overlay covers the whole window.

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}">
    <!-- the whole app: menu, page, status bar, ... -->
</nav:OverlayContainer>
```

## ToastContainer

Stacks the toasts in a corner of whatever it wraps — `Position` picks which one
(`Joufflu.Feedback`, usable on its own without the navigation package). Wrap it
*around* the `OverlayContainer` so toasts stay above the overlays too:

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}" Position="BottomRight">
    <nav:OverlayContainer Overlays="{Binding Overlays}">
        <!-- the whole app -->
    </nav:OverlayContainer>
</feedback:ToastContainer>
```

## The current page

The current page needs no dedicated container: a plain `ContentControl` bound to
the navigator renders it, the view resolved by an implicit `DataTemplate`.

```xml
<ContentControl Content="{Binding Navigator.CurrentPage}" />
```
