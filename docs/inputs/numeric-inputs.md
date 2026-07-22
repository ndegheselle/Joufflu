---
title: Numeric & format inputs
parent: Inputs
nav_order: 1
---

# Numeric & format inputs

## NumericUpDown

Selects a whole number. Built on `FormatTextBox` with the numeric format, plus
clear and increment/decrement buttons.

```xml
<inputs:NumericUpDown Value="{Binding NumericValue, Mode=TwoWay}" />
```

## DecimalUpDown

Selects a double / decimal value using the decimal format.

```xml
<inputs:DecimalUpDown Value="{Binding DecimalValue, Mode=TwoWay}" />
```

## TimeSpanPicker

Selects a `TimeSpan` through a days/hours/minutes/seconds format.

```xml
<inputs:TimeSpanPicker Value="{Binding Duration, Mode=TwoWay}" />
```

## FormatTextBox

The base input: a format string describes groups, parsed into individual values
and navigated with <kbd>Tab</kbd> / arrows.

```xml
<format:FormatTextBox Format="{}{max:23}h {max:59}m {max:59}s" GlobalFormat="numeric" />
```

The `format` namespace is:

```xml
xmlns:format="clr-namespace:Joufflu.Inputs.Controls.Format;assembly=Joufflu.Inputs"
```
