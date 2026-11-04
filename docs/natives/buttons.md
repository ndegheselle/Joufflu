---
title: Buttons
parent: Native controls
nav_order: 4
---

# Buttons

A plain `<Button>` is themed by default. Named styles, applied through `Style`,
give it a semantic intent and a visual emphasis — solid, soft or outline.

## Variants

The solid named styles map to the semantic brushes: a filled background with its
matching content colour. `GhostButton` has no fill or border; hovering fills it.

```xml
<Button>Default</Button>
<Button Style="{StaticResource PrimaryButton}">Primary</Button>
<Button Style="{StaticResource SecondaryButton}">Secondary</Button>
<Button Style="{StaticResource GhostButton}">Ghost</Button>
<Button Style="{StaticResource SuccessButton}">Success</Button>
<Button Style="{StaticResource DangerButton}">Danger</Button>
<Button Style="{StaticResource InfoButton}">Info</Button>
<Button Style="{StaticResource WarningButton}">Warning</Button>
<Button IsEnabled="False">Disabled</Button>
```

## Soft

A tinted background with the semantic hue as text — lower emphasis than solid,
for secondary actions. The tint is the semantic colour at low opacity (≈14 %,
≈24 % on hover), not a dedicated palette entry, so it derives automatically in
both Light and Dark and follows any custom theme. Secondary is achromatic, so its
tint is lighter (≈8 %, ≈14 % on hover).

```xml
<Button Style="{StaticResource SoftPrimaryButton}">Primary</Button>
<Button Style="{StaticResource SoftSecondaryButton}">Secondary</Button>
<Button Style="{StaticResource SoftSuccessButton}">Success</Button>
<Button Style="{StaticResource SoftInfoButton}">Info</Button>
<Button Style="{StaticResource SoftWarningButton}">Warning</Button>
<Button Style="{StaticResource SoftDangerButton}">Danger</Button>
```

{: .note }
> Since the soft text is the semantic hue itself, the brighter semantics (Success,
> Warning) sit at lower text-contrast on the pale tint in the Light theme. Reserve
> soft buttons for short labels / secondary actions, or use solid when contrast
> matters.

## Outline

A coloured border and text over a transparent fill; hovering fills it with the
soft tint. Secondary uses the neutral border and foreground.

```xml
<Button Style="{StaticResource OutlinePrimaryButton}">Primary</Button>
<Button Style="{StaticResource OutlineSecondaryButton}">Secondary</Button>
<Button Style="{StaticResource OutlineSuccessButton}">Success</Button>
<Button Style="{StaticResource OutlineInfoButton}">Info</Button>
<Button Style="{StaticResource OutlineWarningButton}">Warning</Button>
<Button Style="{StaticResource OutlineDangerButton}">Danger</Button>
```

## Icon buttons

`IsSquare` makes a button as wide as it is tall, for a single icon. Composes with
any variant.

```xml
<Button toolkit:Sizing.IsSquare="True" Style="{StaticResource PrimaryButton}">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Plus}" />
</Button>
<Button toolkit:Sizing.IsSquare="True">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Settings}" />
</Button>
<Button toolkit:Sizing.IsSquare="True" Style="{StaticResource DangerButton}">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Trash}" />
</Button>
```

## Sizes

The inherited `Sizing.Size` attached property (`xs` / `sm` / `md` /
`lg`) scales height, font size and padding.

```xml
<Button toolkit:Sizing.Size="xs">XS</Button>
<Button toolkit:Sizing.Size="sm">SM</Button>
<Button toolkit:Sizing.Size="md">MD</Button>
<Button toolkit:Sizing.Size="lg">LG</Button>
```

A button inside an input (clear, open, step…) uses `EmbeddedButton`: see
[Create a custom control](../toolkit/custom-control.md).

## Toggle buttons

A plain `<ToggleButton>` is themed like a button and filled with the primary
colour when checked. The ghost, solid, soft and outline styles each have a toggle
twin, the `Button` suffix replaced by `ToggleButton`. Checked, the ghost toggle
turns primary, a solid one takes its hover shade, and soft and outline ones turn
solid.

```xml
<ToggleButton IsChecked="True">Default</ToggleButton>
<ToggleButton Style="{StaticResource PrimaryToggleButton}">Primary</ToggleButton>
<ToggleButton Style="{StaticResource GhostToggleButton}">Ghost</ToggleButton>
<ToggleButton Style="{StaticResource SoftSuccessToggleButton}">Success</ToggleButton>
<ToggleButton Style="{StaticResource OutlineDangerToggleButton}">Danger</ToggleButton>
```

`IconToggleButton` is a ready-made square toggle for a single icon, transparent
until hovered or checked. `IsSquare` composes with any other variant.

```xml
<ToggleButton Style="{StaticResource IconToggleButton}">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Bell}" />
</ToggleButton>
<ToggleButton toolkit:Sizing.IsSquare="True" Style="{StaticResource PrimaryToggleButton}">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Settings}" />
</ToggleButton>
```

The chevron toggle of a tree row, `ChevronToggleButton`, is on the
[Font icon](icons.md#chevron) page.
