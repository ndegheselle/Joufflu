---
title: Tooltip
parent: Custom controls
nav_order: 5
---

# Tooltip

## Tooltip.Content

Set `Tooltip.Content` on **any** element to attach a themed tooltip that shows
instantly on hover. It builds on the native `ToolTip`, so screen-edge flipping and
fade-in keep working, but unlike the native tooltip it appears with no delay.

`Tooltip.Content` is an `object`: it accepts a plain string or arbitrary XAML.

```xml
<!-- A string tooltip -->
<Button Content="Save" feedback:Tooltip.Content="Save your changes" />

<!-- Arbitrary content: icons, panels, anything -->
<Button Content="Rich tooltip">
    <feedback:Tooltip.Content>
        <StackPanel Orientation="Horizontal" joufflu:Spacing.Gap="8">
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Info}" />
            <TextBlock Text="Arbitrary content, icons included." />
        </StackPanel>
    </feedback:Tooltip.Content>
</Button>
```

## Tooltip.Placement

`Tooltip.Placement` chooses which side the tooltip sits on — `Top` (default),
`Bottom`, `Left` or `Right`. It is centered on the shared edge, kept a small gap
clear of the element, and still flips automatically when it would run off a screen
edge.

```xml
<Button Content="Top"    feedback:Tooltip.Content="Placed above" feedback:Tooltip.Placement="Top" />
<Button Content="Bottom" feedback:Tooltip.Content="Placed below" feedback:Tooltip.Placement="Bottom" />
<Button Content="Left"   feedback:Tooltip.Content="Placed left"  feedback:Tooltip.Placement="Left" />
<Button Content="Right"  feedback:Tooltip.Content="Placed right" feedback:Tooltip.Placement="Right" />
```

{: .note }
> Setting `Tooltip.Content` to `null` removes the tooltip, so it composes with
> triggers — the [navigation menu](../navigation/navigation-menu.md) attaches a
> right-placed tooltip to each item only while the menu is collapsed.
