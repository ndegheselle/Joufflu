---
title: Dropdown
parent: Inputs
nav_order: 7
---

# Dropdown

Attached properties that hang a themed popup off any `ToggleButton` — a menu of
commands, a filter panel, a colour list. The popup is open while the button is
checked, and closes when you click outside it.

The button is not wrapped in a control: it stays a plain `ToggleButton` you own,
so its `Style`, its content, its triggers and every attached property
(`Sizing.IsSquare`, `Sizing.Size`, `Derive.*`, …) apply directly to it.

```xml
<ToggleButton Content="Actions">
    <inputs:Dropdown.Popup>
        <StackPanel joufflu:Spacing.Gap="4">
            <Button Content="Rename" Style="{StaticResource GhostButton}" />
            <Button Content="Duplicate" Style="{StaticResource GhostButton}" />
        </StackPanel>
    </inputs:Dropdown.Popup>
</ToggleButton>
```

| Attached property | Type | Default | Purpose |
|---|---|---|---|
| `Dropdown.Popup` | `object` | `null` | Content shown in the popup. Setting it is what turns the button into a dropdown. |
| `Dropdown.Placement` | `DropdownPlacement` | `BottomLeft` | Corner of the popup anchored to the matching corner of the button: `BottomLeft`, `BottomRight`, `TopLeft`, `TopRight`. |
| `Dropdown.HorizontalOffset` / `Dropdown.VerticalOffset` | `double` | `0` | Extra offset applied on top of the placement. |
| `Dropdown.PopupStyle` | `Style` | `null` | Style of the `DropdownPopupHost` drawing the chrome — its `Padding`, background, border, corner radius. |

`BottomRight` right-aligns the popup on the button, `TopLeft` / `TopRight` open it
upward — useful for a dropdown sitting near the bottom of the window.

```xml
<ToggleButton Content="Filter" inputs:Dropdown.Placement="BottomRight">
    ...
</ToggleButton>
```

An icon-only dropdown is just an icon-only toggle button:

```xml
<ToggleButton
    inputs:Dropdown.Placement="BottomRight"
    joufflu:Sizing.IsSquare="True"
    joufflu:Sizing.Size="lg">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.EllipsisVertical}" />
    <inputs:Dropdown.Popup>
        <TextBlock Text="An icon only dropdown." />
    </inputs:Dropdown.Popup>
</ToggleButton>
```

## Popup chrome

The popup content is wrapped in a `DropdownPopupHost`, a `ContentControl` whose
default style draws the themed border. A `Popup` is only a positioning primitive —
it derives from `FrameworkElement`, so it has no template, background or padding of
its own — which is why `Dropdown.PopupStyle` targets the host rather than the popup.
Padding, background, border and corner radius all go through it, based on the
default style to keep the theme:

```xml
<ToggleButton Content="Padded">
    <inputs:Dropdown.PopupStyle>
        <Style
            TargetType="{x:Type inputs:DropdownPopupHost}"
            BasedOn="{StaticResource {x:Type inputs:DropdownPopupHost}}">
            <Setter Property="Padding" Value="12,8" />
        </Style>
    </inputs:Dropdown.PopupStyle>
    <inputs:Dropdown.Popup>
        <TextBlock Text="Content padded away from the border." />
    </inputs:Dropdown.Popup>
</ToggleButton>
```

Padding defaults to `0`, so content sits flush against the border unless you ask
otherwise.

A `Popup` also lives outside the button's logical tree, so nothing would normally
flow into it: `DataContext`, `Foreground` and `Sizing.Size` are restored on the host
for you.
