---
title: Tooltip
parent: Toolkit
nav_order: 3
---

# Tooltip

## Tooltip.Content

Set `Tooltip.Content` on **any** element to attach a themed tooltip that shows
instantly on hover. It builds on the native `ToolTip` under the hood, so
screen-edge flipping and fade-in keep working — but unlike the native tooltip it
appears with no delay.

`Tooltip.Content` is an `object`, so it accepts a plain string or arbitrary XAML.

```xml
<!-- A string tooltip -->
<Button Content="Save" joufflu:Tooltip.Content="Save your changes" />

<!-- Arbitrary content: icons, panels, anything -->
<Button Content="Rich tooltip">
    <joufflu:Tooltip.Content>
        <StackPanel Orientation="Horizontal" joufflu:Spacing.Gap="8">
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Info}" />
            <TextBlock Text="Arbitrary content, icons included." />
        </StackPanel>
    </joufflu:Tooltip.Content>
</Button>
```

## Tooltip.Placement

`Tooltip.Placement` chooses which side of the element the tooltip sits on —
`Top` (default), `Bottom`, `Left` or `Right`. The tooltip is centered on the
shared edge and kept a small gap clear of the element, and it still flips
automatically when it would run off a screen edge.

```xml
<Button Content="Top"    joufflu:Tooltip.Content="Placed above" joufflu:Tooltip.Placement="Top" />
<Button Content="Bottom" joufflu:Tooltip.Content="Placed below" joufflu:Tooltip.Placement="Bottom" />
<Button Content="Left"   joufflu:Tooltip.Content="Placed left"  joufflu:Tooltip.Placement="Left" />
<Button Content="Right"  joufflu:Tooltip.Content="Placed right" joufflu:Tooltip.Placement="Right" />
```

{: .note }
> Setting `Tooltip.Content` to `null` removes the tooltip, so it composes with
> triggers — for example the [navigation menu](../navigation/navigation-menu.html)
> attaches a right-placed tooltip to each item only while the menu is collapsed.
