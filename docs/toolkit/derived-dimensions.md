---
title: Derived dimensions
parent: Toolkit
nav_order: 8
---

# Derived dimensions

A `Thickness` or a `CornerRadius` declared in a `ResourceDictionary` is baked at
parse time. Its `Left`/`Top`/`Right`/`Bottom` (or `TopLeft`/`TopRight`/…) are
plain CLR properties, not dependency properties, so they can only be fed with
`StaticResource` and never follow a later change of the scalar they were built
from:

```xml
<!-- Baked at load: editing Dimensions.Thickness at runtime does nothing here -->
<Thickness x:Key="MenuBorderThickness"
           Right="{StaticResource {x:Static joufflu:Dimensions.Thickness}}" />
```

`Derive.BorderThickness` and `Derive.CornerRadius` build the value on the element
instead, from a real `DynamicResource`. A scalar edited at runtime — by the
[theme customizer](customize-theme.html), for instance — flows straight through,
and no derived resource key has to be declared or re-pushed by hand.

## Derive.BorderThickness

Point `Derive.BorderThickness` at a resource key and keep the sides you want
with `Derive.BorderSides`. The keys and everything else are unchanged; only the
sides you leave out are set to `0`.

```xml
<!-- Right edge only, following Dimensions.Thickness live -->
<Border extensions:Derive.BorderThickness="{x:Static joufflu:Dimensions.Thickness}"
        extensions:Derive.BorderSides="Right" />

<!-- Open at the bottom -->
<Border extensions:Derive.BorderThickness="{x:Static joufflu:Dimensions.Thickness}"
        extensions:Derive.BorderSides="Left,Top,Right" />
```

`BorderSides` is a flags enum: `Left`, `Top`, `Right`, `Bottom`, the `Horizontal`
and `Vertical` pairs, `All` (the default) and `None`.

Both properties work in a `Style` setter, so a control can derive its own border
without a keyed `Thickness`:

```xml
<Style TargetType="{x:Type nav:NavigationMenu}">
    <Setter Property="extensions:Derive.BorderThickness" Value="{x:Static joufflu:Dimensions.Thickness}" />
    <Setter Property="extensions:Derive.BorderSides" Value="Right" />
</Style>
```

## Derive.CornerRadius

Same shape, with `Derive.Corners` selecting the corners to round.

```xml
<!-- Top corners only, matching the border it sits in -->
<Border extensions:Derive.CornerRadius="{x:Static joufflu:Dimensions.Radius}"
        extensions:Derive.Corners="Top" />
```

`Corners` is a flags enum: `TopLeft`, `TopRight`, `BottomRight`, `BottomLeft`,
the `Top`, `Bottom`, `Left` and `Right` pairs, `All` (the default) and `None`.

## Notes

- The source resource may be a `double` (the usual case, spread over every side
  or corner before masking) or an already built `Thickness` / `CornerRadius`,
  whose own sides are then masked.
- `Derive.BorderThickness` applies to `Border` and to any `Control`;
  `Derive.CornerRadius` applies to `Border`. Anything else throws.
- The derived value is written with `SetCurrentValue`, so a style trigger or an
  animation targeting `BorderThickness` or `CornerRadius` still takes over.

Snippets use these XML namespaces:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:extensions="clr-namespace:Joufflu.Extensions;assembly=Joufflu"
```
