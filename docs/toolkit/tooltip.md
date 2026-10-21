---
title: Tooltip
parent: Toolkit
nav_order: 4
---

# Tooltip

## ThemedTooltip.Content

Set `ThemedTooltip.Content` on **any** element to attach a themed tooltip that shows
instantly on hover. It builds on the native `ToolTip`, so screen-edge flipping and
fade-in keep working, but unlike the native tooltip it appears with no delay, unless the
element sets `ToolTipService.InitialShowDelay` or `BetweenShowDelay` itself.

`ThemedTooltip.Content` is an `object`: it accepts a plain string or arbitrary XAML.

```xml
<!-- A string tooltip -->
<Button Content="Save" toolkit:ThemedTooltip.Content="Save your changes" />

<!-- Arbitrary content: icons, panels, anything -->
<Button Content="Rich tooltip">
    <toolkit:ThemedTooltip.Content>
        <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Info}" />
            <TextBlock Text="Arbitrary content, icons included." />
        </StackPanel>
    </toolkit:ThemedTooltip.Content>
</Button>
```

## ThemedTooltip.Placement

`ThemedTooltip.Placement` chooses which side the tooltip sits on — `Top`, `Bottom`
(default), `Left` or `Right`. It is centered on the shared edge, kept a small gap
clear of the element, and still flips automatically when it would run off a screen
edge.

```xml
<Button Content="Top"    toolkit:ThemedTooltip.Content="Placed above" toolkit:ThemedTooltip.Placement="Top" />
<Button Content="Bottom" toolkit:ThemedTooltip.Content="Placed below" toolkit:ThemedTooltip.Placement="Bottom" />
<Button Content="Left"   toolkit:ThemedTooltip.Content="Placed left"  toolkit:ThemedTooltip.Placement="Left" />
<Button Content="Right"  toolkit:ThemedTooltip.Content="Placed right" toolkit:ThemedTooltip.Placement="Right" />
```

{: .note }
> Setting `ThemedTooltip.Content` to `null` removes the tooltip, so it composes with
> triggers — the [navigation menu](../navigation/navigation-menu.md) attaches a
> right-placed tooltip to each item only while the menu is collapsed.
