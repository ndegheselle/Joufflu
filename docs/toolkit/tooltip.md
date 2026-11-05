---
title: Tooltip & popup placement
parent: Toolkit
nav_order: 4
---

# Tooltip & popup placement

Set `ToolTip` the usual way, as a string or arbitrary XAML: every tooltip is themed. Its
style places it with a custom placement rather than at the mouse, centred on a side of
the element and kept a small gap clear of it, and WPF flips it to the opposite side
when it would run off a screen edge. The show delay is the native one. Any other `Popup`
is placed the same way with [`PopupPlacement`](#popupplacement).

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

## Tooltip.Placement

`Tooltip.Placement`, set on the element, chooses which side its tooltip sits on —
`Top`, `Bottom` (default), `Left` or `Right`.

```xml
<Button Content="Top"    ToolTip="Placed above" toolkit:Tooltip.Placement="Top" />
<Button Content="Bottom" ToolTip="Placed below" toolkit:Tooltip.Placement="Bottom" />
<Button Content="Left"   ToolTip="Placed left"  toolkit:Tooltip.Placement="Left" />
<Button Content="Right"  ToolTip="Placed right" toolkit:Tooltip.Placement="Right" />
```

## Tooltip.Alignment

`Tooltip.Alignment`, set on the element, lines its tooltip up with it along that side —
`Start`, `Center` (default) or `End`. Above or below, `Start` aligns the left edges and
`End` the right ones; beside, the top and the bottom edges.

```xml
<Button Content="Start"  ToolTip="Aligned on the left edge"  toolkit:Tooltip.Alignment="Start" />
<Button Content="Center" ToolTip="Centred under the button" toolkit:Tooltip.Alignment="Center" />
<Button Content="End"    ToolTip="Aligned on the right edge" toolkit:Tooltip.Alignment="End" />
```

## Tooltip.MaxWidth

`Tooltip.MaxWidth`, set on the element, is the width past which the text of its
tooltip wraps: `320` by default.

```xml
<Button Content="Narrow"
        toolkit:Tooltip.MaxWidth="160"
        ToolTip="A long description, wrapped at a narrower max width." />
```

{: .note }
> The placement is the one of [`PopupPlacement`](#popupplacement), with its default 2 DIP gap.
> The style sets the tooltip's own `Placement`, so `ToolTipService.Placement` on an
> element no longer moves it: use `Tooltip.Placement`. The style does it through
> `Tooltip.IsPlaced="True"`, so a `ToolTip` style of your own keeps the placement
> when it is based on the Joufflu one (`BasedOn="{StaticResource {x:Type ToolTip}}"`) or
> sets that property itself. A `ToolTip` set from a
> trigger composes as usual — the [navigation menu](../navigation/navigation-menu.md)
> gives each item a right-placed tooltip only while the menu is collapsed.

{: .note }
> `Tooltip` was named `ThemedTooltip` before. Its `Placement` takes a `PopupSide`, which
> replaces `TooltipPlacement` with the same values.

## PopupPlacement

`PopupPlacement` places any `Popup` on a side of its `PlacementTarget`, lined up with it,
and flips it to the opposite side when it would run off a screen edge. Tooltips and
[dropdowns](../inputs/dropdown.md) are placed the same way: a dropdown reads these
properties from its button. Set anywhere else than on a `Popup` or a dropdown button,
they do nothing.

```xml
<!-- Under the search box, its right edge on the box's right edge, 2 DIPs below (the default gap) -->
<Popup PlacementTarget="{Binding ElementName=SearchBox}"
       toolkit:PopupPlacement.Side="Bottom"
       toolkit:PopupPlacement.Alignment="End">
    <inputs:DropdownPopupHost>
        <TextBlock Text="Placed popup" />
    </inputs:DropdownPopupHost>
</Popup>
```

| Attached property | Type | Default | Purpose |
|---|---|---|---|
| `PopupPlacement.Side` | `PopupSide?` | `null` (`Bottom`) | Side of the target the popup sits on: `Top`, `Bottom`, `Left`, `Right`. |
| `PopupPlacement.Alignment` | `PopupAlignment?` | `null` (`Start`) | How the popup lines up with the target along that side: `Start`, `Center`, `End`. |
| `PopupPlacement.Gap` | `double` | `2` | Space kept between the target and the popup, on the opposite side too; the same as tooltips. |

Above or below the target, `Start` aligns the left edges and `End` the right ones;
beside it, the top and the bottom edges. `Center` centres the popup on that side.

Setting `Side` or `Alignment` hands the placement over: the popup's `Placement` becomes
`Custom` and a `CustomPopupPlacementCallback` reads the three properties each time it
opens. The one left unset takes its default. A `Gap` alone changes nothing. Clearing
both gives the popup back its native `Placement`.

- The popup's own `HorizontalOffset` and `VerticalOffset` still apply on top.
- Changing a property while the popup is open moves it: WPF only places a popup when it
  opens, so it is closed and reopened.
- The popup is placed on the preferred side first, then on the opposite side with the
  same alignment when the first does not fit on the screen. It slides sideways to stay
  on screen above or below the target, up or down beside it.

{: .note }
> A `Popup` renders in its own tree, so the theme `Foreground` does not reach its
> content. Wrap the content in an `inputs:DropdownPopupHost` (from `Joufflu.Inputs`)
> for the themed frame and foreground, as above.
