---
title: Dropdown
parent: Inputs
nav_order: 7
---

# Dropdown

A toggle button that opens a themed popup holding any content — a menu of
commands, a filter panel, a colour list. `Header` is the button, the control's
content is the popup.

```xml
<inputs:Dropdown Header="Actions">
    <StackPanel joufflu:Spacing.Gap="4">
        <Button Content="Rename" Style="{StaticResource GhostButton}" />
        <Button Content="Duplicate" Style="{StaticResource GhostButton}" />
    </StackPanel>
</inputs:Dropdown>
```

| Property | Type | Default | Purpose |
|---|---|---|---|
| `Header` | `object` | `null` | Content of the toggle button that opens the popup. |
| `PopupPlacement` | `DropdownPlacement` | `BottomLeft` | Corner of the popup anchored to the matching corner of the button: `BottomLeft`, `BottomRight`, `TopLeft`, `TopRight`. |
| `HorizontalOffset` / `VerticalOffset` | `double` | `0` | Extra offset applied on top of the placement. |
| `ButtonStyle` | `Style` | `null` | Style of the toggle button — any `ToggleButton` style. |
| `PopupStyle` | `Style` | `null` | Style of the hosting `Popup`. |

`BottomRight` right-aligns the popup on the button, `TopLeft` / `TopRight` open it
upward — useful for a dropdown sitting near the bottom of the window.

```xml
<inputs:Dropdown Header="Filter" PopupPlacement="BottomRight">
    ...
</inputs:Dropdown>
```
