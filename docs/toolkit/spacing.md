---
title: Spacing
parent: Toolkit
nav_order: 2
---

# Spacing

## Spacing.Gap

`Gap` adds even spacing between the children of a panel — no per-child margins.
A `StackPanel` spaces along its orientation; `WrapPanel` and `Grid` can space on
both axes.

`Gap` is a `Thickness`, so a single value is a uniform gap on both axes, while a
`"horizontal,vertical"` pair sets each axis independently (the gap's `Left`
drives the horizontal axis and `Top` the vertical one).

```xml
<!-- Gap adds spacing between children of any panel -->
<!-- StackPanel: gap along the Orientation axis -->
<StackPanel Orientation="Horizontal" joufflu:Spacing.Gap="8">
    <Button>One</Button> <Button>Two</Button> <Button>Three</Button>
</StackPanel>

<!-- Gap is a Thickness: one value is uniform, -->
<!-- "horizontal,vertical" sets each axis (Left/Top) -->
<WrapPanel joufflu:Spacing.Gap="8,12">
    ...
</WrapPanel>

<!-- Grid: gap sits between rows and columns -->
<Grid joufflu:Spacing.Gap="12">
    ...
</Grid>
```

{: .note }
> Because spacing is implemented through child margins, any margin set directly
> on a child is overwritten while spacing is active.
