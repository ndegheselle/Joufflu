---
title: Dropdown
parent: Inputs
grand_parent: Joufflu controls
nav_order: 8
---

# Dropdown

Attached properties that hang a themed popup off any `ToggleButton` — a menu of
commands, a filter panel, a colour list. The popup is open while the button is
checked, and closes when you click outside it.

The button is not wrapped in a control: it stays a plain `ToggleButton` you own,
so its `Style`, its content, its triggers and every attached property
(`Sizing.IsSquare`, `Sizing.Size`, `Derive.*`, …) apply directly to it.

## Dropdown

`Dropdown.Popup` hangs the popup off the button, open while it is checked.
`Dropdown.CloseOnClick` dismisses it when a button inside it is clicked, the way
picking a command from a menu does (see [Closing on click](#closing-on-click)).

```xml
<ToggleButton Content="Actions" inputs:Dropdown.CloseOnClick="True">
    <inputs:Dropdown.Popup>
        <StackPanel toolkit:Spacing.Gap="4">
            <Button Content="Rename" Style="{StaticResource GhostButton}" />
            <Button Content="Duplicate" Style="{StaticResource GhostButton}" />
            <Button Content="Delete" Style="{StaticResource GhostButton}" />
        </StackPanel>
    </inputs:Dropdown.Popup>
</ToggleButton>
```

| Attached property | Type | Default | Purpose |
|---|---|---|---|
| `Dropdown.Popup` | `object` | `null` | Content shown in the popup. Setting it is what turns the button into a dropdown. |
| `toolkit:PopupPlacement.Side` / `Alignment` / `Gap` | | `Bottom` / `Start` / `2` | Where the popup opens, set on the button (see [Popup placement](#popup-placement)). |
| `Dropdown.HorizontalOffset` | `double` | `0` | Extra horizontal offset applied on top of the placement. |
| `Dropdown.VerticalOffset` | `double` | `0` | Extra vertical offset applied on top of the placement; the popup sits 2 DIPs from the button by default (the `PopupPlacement` gap). |
| `Dropdown.CloseOnClick` | `bool` | `false` | Close the popup when a button inside it is clicked. |
| `Dropdown.PopupStyle` | `Style` | `null` | Style of the `DropdownPopupHost` drawing the chrome — its background, border, `CornerRadius`, `Padding`, `Foreground` or template (see [Popup chrome](#popup-chrome)). |

## Popup placement

The [`PopupPlacement`](../toolkit/tooltip.md#popupplacement) properties set on the
button place its popup, the way `ToolTipService.Placement` is set on the element
owning a tooltip:

- `Side` — `Bottom` (default), `Top`, `Left` or `Right`. `Top` opens it upward,
  useful for a dropdown sitting near the bottom of the window.
- `Alignment` — `Start` (default), `Center` or `End`. Below or above the button, `End`
  right-aligns the popup on it.
- `Gap` — space between the button and the popup, `2` by default.

When the popup does not fit on the screen, it flips to the opposite side.
`Dropdown.HorizontalOffset` and `Dropdown.VerticalOffset` add to that placement.

```xml
<!-- Side: Bottom (default), Top, Left, Right; Alignment: Start (default), Center, End; Gap: 2 by default -->
<ToggleButton Content="Filter"
              toolkit:PopupPlacement.Side="Bottom"
              toolkit:PopupPlacement.Alignment="End">
    <inputs:Dropdown.Popup>
        <TextBlock Text="Opens down, right aligned." />
    </inputs:Dropdown.Popup>
</ToggleButton>
```

{: .note }
> `Dropdown.Placement` and its `DropdownPlacement` corners are gone: `BottomRight` is
> now `Side="Bottom"` with `Alignment="End"`, `TopLeft` is `Side="Top"`. A tooltip on the
> same button keeps its own `Tooltip.Placement`, so the two popups are placed apart.

## Button style

Because the button is a real `ToggleButton`, every `ToggleButton` style and
attached property works directly on it, `Sizing.IsSquare` and `Sizing.Size`
included. An icon-only dropdown is just an icon-only toggle button:

```xml
<ToggleButton
    toolkit:PopupPlacement.Alignment="End"
    toolkit:Sizing.IsSquare="True"
    toolkit:Sizing.Size="lg">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.EllipsisVertical}" />
    <inputs:Dropdown.Popup>
        <TextBlock Text="An icon only dropdown." />
    </inputs:Dropdown.Popup>
</ToggleButton>
```

## Closing on click

By default the popup stays open until you click outside it. For a menu of commands
that is rarely what you want, so `Dropdown.CloseOnClick` dismisses it as soon as a
button inside is clicked, as in the first example above.

It listens for `ButtonBase.Click`, not for any mouse click, so a `TextBox`, a
`Slider` or a `ComboBox` in the popup stays usable. The flip side is that a
`CheckBox`, a `RadioButton` or a nested `ToggleButton` is a `ButtonBase` too and
will close it — leave `CloseOnClick` off for a popup built out of those.

The command still runs: the click is not handled, it only unchecks the button.

## Popup chrome

The popup content is wrapped in a `DropdownPopupHost`, a `ContentControl` whose
default style draws the themed border. A `Popup` is only a positioning primitive —
it derives from `FrameworkElement`, so it has no template, background or padding of
its own — which is why `Dropdown.PopupStyle` targets the host rather than the popup.
Base it on the default style to keep the theme:

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

The style sets the host's `Background`, `BorderBrush`, `BorderThickness`,
`CornerRadius`, `Padding` and `Foreground`. They default to the display tokens:
`DisplayBase0Brush`, `DisplayBorder0Brush`, `DisplayThickness`, `DisplayRadius` and
`DisplayPaddingSm`.

## Bindings inside the popup

A `Popup` lives outside the button's logical tree, so nothing would normally flow
into it. `DataContext` and `Sizing.Size` are relayed from the button and the host
restates the theme `Foreground`, so ordinary command and value bindings need nothing
special. A `Foreground` set on the button itself does not flow in; set it through
`Dropdown.PopupStyle` instead:

```xml
<ToggleButton Content="Actions">
    <inputs:Dropdown.Popup>
        <Button Command="{Binding RenameCommand}" Content="Rename" />
    </inputs:Dropdown.Popup>
</ToggleButton>
```

What cannot cross the boundary is anything that walks *out* of the popup — a
`RelativeSource` ancestor lookup or an `ElementName`, both of which stop at the
popup's own `PopupRoot`. Bind through `DropdownPopupHost.PlacementTarget` instead;
the host is a plain ancestor inside the popup, so that lookup stays in one tree:

```xml
<Button Command="{Binding PlacementTarget.DataContext.SaveCommand,
    RelativeSource={RelativeSource AncestorType={x:Type inputs:DropdownPopupHost}}}" />
```

`PlacementTarget` is the `ToggleButton` itself, so the same route reaches its
properties — `IsChecked`, a `Tag`, an inherited value — not just its `DataContext`.
