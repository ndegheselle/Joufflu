---
title: Numeric & format inputs
parent: Inputs
nav_order: 1
---

# Numeric & format inputs

## NumericUpDown

Selects a whole number (`long?`). Built on `FormatTextBox` with a single
`IntegerGroup`, plus clear and increment/decrement buttons.

```xml
<inputs:NumericUpDown Value="{Binding NumericValue, Mode=TwoWay}" />
```

## DecimalUpDown

Selects a `decimal?` value, through a single `DecimalGroup`.

```xml
<inputs:DecimalUpDown Value="{Binding DecimalValue, Mode=TwoWay}" />
```

## TimeSpanPicker

Selects a `TimeSpan` through a days/hours/minutes/seconds format.

```xml
<inputs:TimeSpanPicker Value="{Binding Duration, Mode=TwoWay}" />
```

To bound or format the number of a `NumericUpDown` or a `DecimalUpDown`, write its
group yourself: a group given as content replaces the default one (which is
nullable and keeps its own caret, so set those back when you want them).

```xml
<inputs:NumericUpDown Value="{Binding Quantity, Mode=TwoWay}">
    <format:IntegerGroup Min="0" Max="99" IsNullable="True" SelectsWhole="False" />
</inputs:NumericUpDown>
```

## FormatTextBox

The base input: groups the user types into, with literal text between them,
each group read into its own value and navigated with <kbd>Tab</kbd> / arrows.

```xml
<format:FormatTextBox>
    <format:IntegerGroup Max="23" StringFormat="00" />
    <format:FormatLiteral Text="h " />
    <format:IntegerGroup Max="59" StringFormat="00" />
    <format:FormatLiteral Text="m" />
</format:FormatTextBox>
```

`IntegerGroup` counts in `long` and `DecimalGroup` in `decimal`. Both take:

| Property | Default | Meaning |
|---|---|---|
| `Min`, `Max` | the type's own | Bounds of what can be typed or spun to. `Max` also sets the group's width: once the number is as long as it, typing moves on to the next group. |
| `Step` | `1` / `0.1` | What <kbd>↑</kbd> / <kbd>↓</kbd> and the mouse wheel add or take away. |
| `StringFormat` | none | A .NET numeric format string: `00` pads to two digits, `N0` separates thousands. |
| `IsNullable` | `false` | Whether the group can hold no number rather than zero. |
| `PromptChar` | `-` | Shown once per character of a group holding no number. |
| `SelectsWhole` | `true` | Selected, typed over and emptied as one thing; when `false` the group keeps its own caret, like a plain text box. |

`Values` holds one value per group, raising `ValuesChanged`.

The `format` namespace is:

```xml
xmlns:format="clr-namespace:Joufflu.Inputs.Controls.Format;assembly=Joufflu.Inputs"
```
