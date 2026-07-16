---
title: Sizing
parent: Toolkit
nav_order: 1
---

# Sizing

## ControlProperties.Size

The inherited `Size` attached property (`xs` / `sm` / `md` / `lg`) scales height,
font size and padding. Because it inherits, setting it on a panel sizes every
child.

```xml
<!-- Attached property drives height, font size and padding -->
<Button joufflu:ControlProperties.Size="xs" />
<Button joufflu:ControlProperties.Size="sm" />
<Button joufflu:ControlProperties.Size="md" />  <!-- default -->
<Button joufflu:ControlProperties.Size="lg" />

<!-- Size is inherited, so a panel sets it for every child -->
<StackPanel joufflu:ControlProperties.Size="lg">
    <TextBox /> <ComboBox /> <Button>OK</Button>
</StackPanel>
```

## ControlProperties.IsSquare

`IsSquare` forces equal width and height — ideal for single-icon buttons.

```xml
<Button joufflu:ControlProperties.IsSquare="True"
        joufflu:ControlProperties.Size="lg">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Leaf}" />
</Button>
```
