---
title: Toolkit
nav_order: 8
has_children: true
---

# Toolkit

Design-system helpers that shape controls and layouts rather than being controls
themselves.

- **Sizing** — the `ControlProperties.Size` and `IsSquare` attached properties.
- **Spacing** — the `Spacing.Gap` attached property for gaps between children.
- **Tooltip** — the `Tooltip.Content` and `Tooltip.Placement` attached properties for themed tooltips on any element.
- **Drop target** — the `DropTarget.Command`, `IsDragOver` and `Effect` attached properties for turning any element into a drop target.
- **Theme** — `ThemeManager` for System/Light/Dark plus registering custom themes, and how to bind a theme switcher UI to it.
- **Customize theme** — the live theme editor and preset themes.
- **Application shell** — the window styles, the `FullContainer` page host and the overlay/toast containers wrapping the app.

Snippets use the `joufflu` XML namespace, plus `nav` and `feedback` for the
application shell containers:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
```
