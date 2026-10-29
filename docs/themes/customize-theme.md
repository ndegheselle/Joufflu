---
title: Customize theme
parent: Themes
nav_order: 4
---

# Customize theme

The gallery's theme customizer tweaks colours and dimensions with the whole
gallery updating live. It opens in a window of its own, from the palette button
next to the light / dark switch in the gallery's title bar, so the gallery behind
it is the preview. Start from a **preset theme**, then copy or save the
generated resource dictionary and merge it into your app **after** the Joufflu
resources to apply it.

The design system is exposed as resource keys you override in a dictionary:
`joufflu:Colors.*` for the colours and `joufflu:Dimensions.*` for the metrics. The
`joufflu:Brushes.*` are built from the colours by the core resources, so overriding a
colour is enough for every brush to follow. [Design tokens](tokens.html) lists them all, with the role each one
plays in the control styles.

The controls read the category tokens (`Display*`, `Input*`, `Action*`), not the global ones. The
customizer has two modes:

- **Simple**: you set the surfaces (Base, Surface 1, Surface 2, Stroke, Foreground) and the shape
  (corner radius, border thickness, spacing) once, and displays, inputs and actions follow them. The
  generated dictionary leaves the category colours out, so they fall back to the global ones, and
  writes the global radius, thickness and spacing into the category keys (`DisplayRadius`,
  `InputCornerRadius`, `ActionBorderThickness`, `DisplayPadding`…). It also edits three scales,
  each set by its md value with the other sizes following by fixed ratios: **Control height**
  (`HeightXs`..`Lg`), **Font size** (`FontSizeXs`..`Xl`) and **Control padding**
  (`ActionPaddingXs`..`Lg`, the input paddings at half the horizontal). The control padding scale
  is shown in the simple mode only.
- **Complete**: each category gets its own Surface 1, Surface 2, Border, corner radius, border
  thickness and padding (horizontal and vertical), starting from the simple values. The padding is
  the inside of a display (`DisplayPadding`), and the md padding of an input or an action, the
  other sizes (`InputPaddingXs`..`Lg`, `ActionPaddingXs`..`Lg`) following it with the ratios of the
  control padding, which is only edited in the simple mode. The generated dictionary writes the
  category colours (`ActionSurface1Color`…) and shapes. Going back to the simple mode drops them.

The states are derived either way: hover and the strong focus border follow the category colours.
A hand-written dictionary overriding only `Dimensions.Radius` or `Dimensions.BorderThickness` no
longer reaches the controls.

```xml
<!-- Merge your overrides AFTER the Joufflu resources -->
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
    <ResourceDictionary Source="MyTheme.xaml" />
</ResourceDictionary.MergedDictionaries>
```

Run the **`Joufflu.Samples`** app and click the palette button in the title bar to
generate a dictionary interactively.
