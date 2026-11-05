---
title: Toolkit
nav_order: 6
has_children: true
---

# Toolkit

Design-system helpers that shape controls and layouts rather than being controls
themselves. They all live in the `Joufflu.Toolkit` namespace of the core
`Joufflu` package — `Sizing`, `Spacing`, `Derive`, `Tooltip`, `PopupPlacement`, `Animate`, `DropTarget`,
`DropData`, `DragSource`, `SharedWidth` and `DataGridEditing` (which the `DataGrid` style uses to
theme the editors of its built-in columns). The theme manager, tokens and window have their own section, see
[Themes](../themes/index.md).

- **Sizing** — the `Sizing.Size` and `IsSquare` attached properties.
- **Spacing** — the `Spacing.Gap` attached property for gaps between children.
- **Derived dimensions** — the `Derive.BorderThickness`, `Derive.CornerRadius` and `Derive.Margin` attached properties, with their factors scaling the value side by side, for thicknesses, radii and margins that follow the theme live.
- **Tooltip & popup placement** — every `ToolTip` themed and placed on a side of its element; `Tooltip.Placement` picks the side, `Tooltip.Alignment` how it lines up (centred by default) and `Tooltip.MaxWidth` where its text wraps. The `PopupPlacement.Side`, `Alignment` and `Gap` attached properties place any `Popup` the same way, flipped to the opposite side at a screen edge.
- **Animate** — the `Animate.Bounce` attached property for a looping vertical hop on any element, with `BounceHeight` and `BounceDuration` shaping it, and `Animate.Spin` for an endless turn around its centre, with `SpinDuration` setting its speed.
- **Drag and drop** — the `DropTarget.Command`, `IsDragOver` and `Effect` attached properties for turning any element into a drop target, with the `DropData` telling what was dropped and where, and `DragSource.Data`, `AllowedEffects` and `IsDragging` for turning any element into a drag source.
- **Shared width** — the `SharedWidth` decorator and its `IsScope` attached property, which give every input of a group the width of the narrowest one so they line up across the depths of a tree.
- **Create a custom control** — the category a control of your own belongs to (display, input, action), the tokens and states of each, then an input built step by step: background and border, height, sizes and padding, states, the validation error style and embedded buttons.

Snippets use the `toolkit` XML namespace for those helpers, `joufflu` for the
design-system keys and `fonts` for the icons, plus `feedback` where an example
targets one of its controls:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
```
