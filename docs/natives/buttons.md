---
title: Buttons
parent: Native controls
nav_order: 1
---

# Buttons

A plain `<Button>` is themed by default. Named styles, applied through the
`Style` property, give it a semantic intent and a visual emphasis — solid, soft
or outline.

## Variants

The solid named styles map to the design system's semantic brushes: a filled
background with its matching content colour.

```xml
<Button>Default</Button>
<Button Style="{StaticResource Primary}">Primary</Button>
<Button Style="{StaticResource Secondary}">Secondary</Button>
<Button Style="{StaticResource Ghost}">Ghost</Button>
<Button Style="{StaticResource Success}">Success</Button>
<Button Style="{StaticResource Danger}">Danger</Button>
<Button Style="{StaticResource Info}">Info</Button>
<Button Style="{StaticResource Warning}">Warning</Button>
```

## Soft

The soft variant is a tinted background with the semantic hue as text — lower
emphasis than solid, useful for secondary actions. The tint is the semantic
colour at low opacity (≈14 %, ≈24 % on hover) rather than a dedicated palette
entry, so it is derived automatically in both Light and Dark and follows any
custom theme.

```xml
<Button Style="{StaticResource SoftPrimary}">Primary</Button>
<Button Style="{StaticResource SoftSecondary}">Secondary</Button>
<Button Style="{StaticResource SoftSuccess}">Success</Button>
<Button Style="{StaticResource SoftInfo}">Info</Button>
<Button Style="{StaticResource SoftWarning}">Warning</Button>
<Button Style="{StaticResource SoftDanger}">Danger</Button>
```

{: .note }
> Because the soft text is the semantic hue itself, the brighter semantics
> (Success, Warning) sit at a lower text-contrast on the pale tint in the Light
> theme. Reserve soft buttons for short labels / secondary actions, or use the
> solid variant when contrast matters.

## Outline

The outline variant is a coloured border and text over a transparent fill;
hovering fills it with the soft tint.

```xml
<Button Style="{StaticResource OutlinePrimary}">Primary</Button>
<Button Style="{StaticResource OutlineSecondary}">Secondary</Button>
<Button Style="{StaticResource OutlineSuccess}">Success</Button>
<Button Style="{StaticResource OutlineInfo}">Info</Button>
<Button Style="{StaticResource OutlineWarning}">Warning</Button>
<Button Style="{StaticResource OutlineDanger}">Danger</Button>
```

## Icon buttons

`IsSquare` makes a button as wide as it is tall — ideal for a single icon. It
composes with any variant.

```xml
<Button joufflu:ControlProperties.IsSquare="True" Style="{StaticResource Primary}">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Plus}" />
</Button>
```

## Sizes

The inherited `ControlProperties.Size` attached property (`xs` / `sm` / `md` /
`lg`) scales height, font size and padding.

```xml
<Button joufflu:ControlProperties.Size="xs">XS</Button>
<Button joufflu:ControlProperties.Size="sm">SM</Button>
<Button joufflu:ControlProperties.Size="md">MD</Button>
<Button joufflu:ControlProperties.Size="lg">LG</Button>
```
