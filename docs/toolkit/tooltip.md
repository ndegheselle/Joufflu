---
title: Tooltip
parent: Toolkit
nav_order: 4
---

# Tooltip

Set `ToolTip` the usual way, as a string or arbitrary XAML: every tooltip is themed. Its
style places it with a custom placement rather than at the mouse, centred on a side of
the element and kept a small gap clear of it, and WPF flips it to the opposite side
when it would run off a screen edge. The show delay is the native one.

```xml
<!-- A string tooltip -->
<Button Content="Save" ToolTip="Save your changes" />

<!-- Arbitrary content: icons, panels, anything -->
<Button Content="Rich tooltip">
    <Button.ToolTip>
        <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Info}" />
            <TextBlock Text="Arbitrary content, icons included." />
        </StackPanel>
    </Button.ToolTip>
</Button>
```

## ThemedTooltip.Placement

`ThemedTooltip.Placement`, set on the element, chooses which side its tooltip sits on —
`Top`, `Bottom` (default), `Left` or `Right`.

```xml
<Button Content="Top"    ToolTip="Placed above" toolkit:ThemedTooltip.Placement="Top" />
<Button Content="Bottom" ToolTip="Placed below" toolkit:ThemedTooltip.Placement="Bottom" />
<Button Content="Left"   ToolTip="Placed left"  toolkit:ThemedTooltip.Placement="Left" />
<Button Content="Right"  ToolTip="Placed right" toolkit:ThemedTooltip.Placement="Right" />
```

## ThemedTooltip.MaxWidth

`ThemedTooltip.MaxWidth`, set on the element, is the width past which the text of its
tooltip wraps: `320` by default.

```xml
<Button Content="Narrow"
        toolkit:ThemedTooltip.MaxWidth="160"
        ToolTip="A long description, wrapped at a narrower max width." />
```

{: .note }
> The style sets the tooltip's own `Placement`, so `ToolTipService.Placement` on an
> element no longer moves it: use `ThemedTooltip.Placement`. A `ToolTip` set from a
> trigger composes as usual — the [navigation menu](../navigation/navigation-menu.md)
> gives each item a right-placed tooltip only while the menu is collapsed.
