---
title: Font icon
parent: Custom controls
nav_order: 1
---

# Font icon

## Glyphs

A Lucide icon-font glyph. `Foreground` uses the design-system brushes and the
size follows the inherited `ControlProperties.Size`.

```xml
<fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<!-- Size flows from the inherited ControlProperties.Size -->
<fonts:FontIcon joufflu:ControlProperties.Size="lg"
                Text="{x:Static fonts:LucideFontIcons.Leaf}" />
```

Glyph names are exposed as constants on `LucideFontIcons` (for example `Leaf`,
`Home`, `Bell`, `Settings`, `Trash2`).
