---
title: Custom input
parent: Toolkit
nav_order: 11
---

# Custom input

An input of your own sits with the Joufflu ones when it reads the same design tokens.
The gallery's **Custom input** page builds a star `RatingInput` from a bare `Control`
step by step; this page lists the rules it follows.

| | Rule | Tokens |
|---|---|---|
| **Required** | Background | `Brushes.BackgroundBrush` |
| **Required** | Border colour and thickness | `Brushes.BorderBrush`, `Dimensions.BorderThickness`, `Dimensions.CornerRadius` |
| **Required** | Height, for a one-line input | `Dimensions.HeightMd` |
| Optional | Sizes and padding | `HeightXs`…`HeightLg`, `FontSizeXs`…`FontSizeLg`, `InputPaddingXs`…`InputPaddingLg` |
| Optional | Hover, focus and disabled states | `Background100Brush`, `Border100Brush`, `Colors.DisabledOpacity` |
| Optional | Validation error style | `DangerBrush`, `ValidationErrorTemplate` |
| Optional | Buttons inside the input | `EmbeddedButton` |

Always reference the tokens with `DynamicResource`: a `StaticResource` is read once and
misses every later theme switch.

## Derive from a native control first

When the input is a native control underneath (a `TextBox` with extras, a `ComboBox`
variant…), derive from it and base the style on the native one. It inherits every rule
on this page at once, and only the template needs writing:

```xml
<Style TargetType="{x:Type local:PrefixedTextBox}" BasedOn="{StaticResource {x:Type TextBox}}">
    <Setter Property="Template"> ... </Setter>
</Style>
```

The template must keep reading `Background`, `BorderBrush`, `BorderThickness` and
`Padding` through `TemplateBinding`, or the inherited triggers have nothing to act on.
The rest of this page is for an input built from a bare `Control`.

## The minimum

Set the background, the border and the height from the tokens in the style, and let the
template read them from the control:

```xml
<Style TargetType="{x:Type local:RatingInput}">
    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.BackgroundBrush}}" />
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.BorderBrush}}" />
    <Setter Property="BorderThickness" Value="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}" />
    <Setter Property="Height" Value="{DynamicResource {x:Static joufflu:Dimensions.HeightMd}}" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:RatingInput}">
                <Grid>
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="{DynamicResource {x:Static joufflu:Dimensions.CornerRadius}}" />
                    <!-- content -->
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

Setting them in the style rather than in the template keeps them overridable, and lets
the triggers below change them. A multi-line input (a list, a text area) leaves the
height alone.

## Sizes and padding

`Sizing.Size` is inherited: a panel set to `lg` expects every input in it to grow. Set the
`md` values by default, and one trigger per other size:

| Size | Height | Font size | Input padding |
|---|---|---|---|
| `xs` | `HeightXs` (24) | `FontSizeXs` (11) | `InputPaddingXs` (3,2) |
| `sm` | `HeightSm` (28) | `FontSizeSm` (12) | `InputPaddingSm` (4.5,3) |
| `md` | `HeightMd` (32) | `FontSizeMd` (13) | `InputPaddingMd` (6,4) |
| `lg` | `HeightLg` (40) | `FontSizeLg` (16) | `InputPaddingLg` (9,6) |

```xml
<Setter Property="FontSize" Value="{DynamicResource {x:Static joufflu:Dimensions.FontSizeMd}}" />
<Setter Property="Padding" Value="{DynamicResource {x:Static joufflu:Dimensions.InputPaddingMd}}" />

<Style.Triggers>
    <Trigger Property="toolkit:Sizing.Size" Value="xs">
        <Setter Property="Height" Value="{DynamicResource {x:Static joufflu:Dimensions.HeightXs}}" />
        <Setter Property="FontSize" Value="{DynamicResource {x:Static joufflu:Dimensions.FontSizeXs}}" />
        <Setter Property="Padding" Value="{DynamicResource {x:Static joufflu:Dimensions.InputPaddingXs}}" />
    </Trigger>
    <!-- same for sm and lg -->
</Style.Triggers>
```

Inputs use the `InputPadding` keys, tighter than the `Padding` keys of the buttons. In the
template, lay the content over the border inside `Padding`, so it lines up with the text
of the native inputs:

```xml
<Grid>
    <Border ... />
    <Grid Margin="{TemplateBinding Padding}"> <!-- content --> </Grid>
</Grid>
```

## States

The native inputs lighten their background on hover and their border on focus, and fade
when disabled:

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.Background100Brush}}" />
</Trigger>
<Trigger Property="IsKeyboardFocusWithin" Value="True">
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.Border100Brush}}" />
</Trigger>
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Opacity" Value="{DynamicResource {x:Static joufflu:Colors.DisabledOpacity}}" />
</Trigger>
```

## Validation error style

Binding errors show in two layers, as on every Joufflu input: the input turns its own
border red, and `ValidationErrorTemplate` adds a badge on the corner whose tooltip lists
the errors. The template draws no border of its own, so the trigger is what makes the
border red:

```xml
<Setter Property="Validation.ErrorTemplate" Value="{DynamicResource ValidationErrorTemplate}" />

<!-- In Style.Triggers, after the hover and focus triggers so it wins over them -->
<Trigger Property="Validation.HasError" Value="True">
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}" />
</Trigger>
```

A user of the input drops the badge with `Validation.ErrorTemplate="{x:Null}"`; the red
border stays.

## Embedded buttons

A button inside the input (clear, open, step…) uses the `EmbeddedButton` style: a
transparent square of `HeightEmbedded` (20). Placed inside `Padding`, it fits every
size: the smallest input leaves exactly 24 − 2 × 2 = 20 for it.

```xml
<Grid Margin="{TemplateBinding Padding}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>
    <!-- content -->
    <Button Grid.Column="1" Command="{Binding ClearCommand, RelativeSource={RelativeSource TemplatedParent}}"
            Style="{StaticResource EmbeddedButton}">
        <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.x}" />
    </Button>
</Grid>
```

`EmbeddedButton` is a named style of `Button.named.xaml`. A `StaticResource` in a
`ResourceDictionary` only sees that dictionary and the ones it merges, so a dictionary
holding the input's style merges it:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Styles/Natives/Actions/Button.named.xaml" />
</ResourceDictionary.MergedDictionaries>
```

## Checking the result

Put the input next to a `TextBox` and compare them:

- in each size, with `Sizing.Size` on a parent panel;
- after switching between Light and Dark, and to a custom theme;
- hovered, focused and disabled;
- with a binding error, with and without `Validation.ErrorTemplate="{x:Null}"`.
