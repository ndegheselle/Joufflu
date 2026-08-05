---
title: Custom controls
nav_order: 6
has_children: true
---

# Custom controls

Custom controls built on the design system.

- **Font icon** (`Joufflu`) — a Lucide icon-font glyph.
- **Badge** (`Joufflu.Feedback`) — a themed pill in the semantic colours.
- **Spinner** (`Joufflu.Feedback`) — an indeterminate loading indicator.
- **Toasts** (`Joufflu.Feedback`) — transient notifications shown from a service.
- **Tooltip** (`Joufflu.Feedback`) — the `Tooltip.Content` and `Tooltip.Placement` attached properties for themed tooltips on any element.

`FontIcon` ships in the core `Joufflu` package; the other four live in the
optional [`Joufflu.Feedback`](https://www.nuget.org/packages/Joufflu.Feedback)
package (`dotnet add package Joufflu.Feedback`), whose `Resources.xaml` must be
merged after the core one. They use these namespaces:

```xml
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
```
