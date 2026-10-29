---
title: Color picker
parent: Inputs
grand_parent: Joufflu controls
nav_order: 7
---

# Color picker

## ColorPicker

Edits a colour through an editable hex value and a popup with a saturation square,
hue and alpha sliders. Two-way bindable through `Color`
(`System.Windows.Media.Color`).

```xml
<inputs:ColorPicker Color="{Binding Color, Mode=TwoWay}" />
```

The popup provides:

- a **saturation / brightness** square you can drag,
- a **hue** slider,
- an **alpha** slider (checkerboard behind a transparent → opaque ramp),
- an editable **hex** field and an **opacity** percentage. A `#RRGGBB` hex keeps the
  opacity, a pasted `#AARRGGBB` sets it too.

## Validation

Binding errors (here from an ObservableValidator) turn the input's border red, and
`ValidationErrorTemplate` adds a corner badge whose tooltip lists them. Pick an
opaque colour to clear it. The second input sets `Validation.ErrorTemplate` to
`{x:Null}`: no badge, only the red border.

```xml
<inputs:ColorPicker Color="{Binding OpaqueColor}" />

<!-- Opt out of the template: only the input's own red border remains -->
<inputs:ColorPicker Validation.ErrorTemplate="{x:Null}" Color="{Binding OpaqueColor}" />
```
