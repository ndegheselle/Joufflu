---
title: Toolkit
nav_order: 6
has_children: true
---

# Toolkit

Design-system helpers that shape controls and layouts rather than being controls
themselves. They all live in the `Joufflu.Toolkit` namespace of the core
`Joufflu` package — `Sizing`, `Spacing`, `Derive`, `ThemedTooltip`, `Animate`, `DropTarget`,
`DragSource` and `SharedWidth`. The theme manager, tokens and window have their own section, see
[Themes](../themes/index.md).

- **Sizing** — the `Sizing.Size` and `IsSquare` attached properties.
- **Spacing** — the `Spacing.Gap` attached property for gaps between children.
- **Derived dimensions** — the `Derive.BorderThickness`, `Derive.CornerRadius` and `Derive.Margin` attached properties, with their factors scaling the value side by side, for thicknesses, radii and margins that follow the theme live.
- **Tooltip** — every `ToolTip` themed and centred on a side of its element; `ThemedTooltip.Placement` picks the side and `ThemedTooltip.MaxWidth` where its text wraps.
- **Animate** — the `Animate.Bounce` attached property for a looping vertical hop on any element, with `BounceHeight` and `BounceDuration` shaping it.
- **Drag and drop** — the `DropTarget.Command`, `IsDragOver` and `Effect` attached properties for turning any element into a drop target, with the `DropData` telling what was dropped and where, and `DragSource.Data`, `AllowedEffects` and `IsDragging` for turning any element into a drag source.
- **Shared width**: the `SharedWidth.IsShared` and `IsScope` attached properties, which give every input of a group the width of the narrowest one so they line up across the depths of a tree.
- **Custom input** — the rules an input of your own follows to sit with the Joufflu ones: background and border from the input category tokens, height, then sizes and padding, states, the validation error style and embedded buttons.

Snippets use the `toolkit` XML namespace for those helpers and `joufflu` for the
design-system keys, plus `nav` and `feedback` where an example targets one of
their controls:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
```
