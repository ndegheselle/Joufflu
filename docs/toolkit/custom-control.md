---
title: Create a custom control
parent: Toolkit
nav_order: 8
---

# Create a custom control

A control of your own sits with the Joufflu ones when it reads the tokens of its
category, so a theme restyling that category restyles it too. Pick the category the
control belongs to, then follow its rules. A control derived from a native one
(`TextBox`, `Button`, `ListBox`…) gets them all by basing its style on the native one.

| Category | What it is | Tokens | States |
|---|---|---|---|
| **Display** | Shows content without being interacted with: a card, a popup, a tooltip, a panel, the frame of a list | `DisplaySurface1/2Brush`, `DisplayBorderBrush`, `DisplayCornerRadius`, `DisplayBorderThickness`, `DisplayPadding` (`DisplayPaddingSm` when compact) | None. Its items (rows, entries) are actions without a fill at rest |
| **Input** | Takes a value: a text field, a picker, a rating, a check | `InputSurface1/2Brush`, `InputBorderBrush`, `InputRadius`, `InputBorderThickness`, `InputPaddingXs`…`Lg`, `HeightXs`…`Lg` | No hover; a strong border on focus (`InputBorderElevatedBrush`) |
| **Action** | Runs something: a button, a toggle, an item of a display | `ActionSurface1/2Brush`, `ActionBorderBrush`, `ActionRadius`, `ActionBorderThickness`, `ActionPaddingXs`…`Lg`, `HeightXs`…`Lg` | A stronger background on hover (`ActionElevatedBrush`), moved down when pressed, no focus border |

In every category, disabled is `DisabledOpacity`, selected and checked are the primary,
and sizes follow `Sizing.Size`. [Design tokens](../themes/tokens.html) lists each key with
its role and fallback.

## Example: an input

The gallery's **Create a custom control** page builds a star `RatingInput` from a bare
`Control` step by step, following every rule of the **Input** category by hand; a display
or an action follows the same steps with the keys of its own category. The rules:

| | Rule | Tokens |
|---|---|---|
| **Required** | Background | `Brushes.InputSurface1Brush` (`InputSurface2Brush` for an elevated part inside: a tag, a track) |
| **Required** | Border colour, thickness and radius | `Brushes.InputBorderBrush`, `Dimensions.InputBorderThickness`, `Dimensions.InputRadius` (clamped to half the height with `MaxCornerRadiusConverter`) |
| **Required** | Height, for a one-line input | `Dimensions.HeightMd` |
| Optional | Sizes and padding | `HeightXs`…`HeightLg`, `FontSizeXs`…`FontSizeLg`, `InputPaddingXs`…`InputPaddingLg` |
| Optional | Focus and disabled states (an input has no hover) | `InputBorderElevatedBrush` (focus), `Colors.DisabledOpacity` |
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
    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.InputSurface1Brush}}" />
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.InputBorderBrush}}" />
    <Setter Property="BorderThickness" Value="{DynamicResource {x:Static joufflu:Dimensions.InputBorderThickness}}" />
    <Setter Property="Height" Value="{DynamicResource {x:Static joufflu:Dimensions.HeightMd}}" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type local:RatingInput}">
                <Grid>
                    <!-- The radius is clamped to half the height, like the native inputs: a large one gives a pill -->
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Tag="{DynamicResource {x:Static joufflu:Dimensions.InputRadius}}">
                        <Border.CornerRadius>
                            <MultiBinding Converter="{StaticResource MaxCornerRadiusConverter}">
                                <Binding Path="Tag" RelativeSource="{RelativeSource Self}" />
                                <Binding Path="ActualWidth" RelativeSource="{RelativeSource Self}" />
                                <Binding Path="ActualHeight" RelativeSource="{RelativeSource Self}" />
                            </MultiBinding>
                        </Border.CornerRadius>
                    </Border>
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

Inputs use the `InputPadding` keys, tighter than the `ActionPadding` keys of the buttons.
The height of a one-line input comes from its size, so, like the native inputs, the
template lays the content over the border inside the **horizontal** part of `Padding`
only, centred vertically: it lines up with the text of the native inputs, and a large
vertical padding never crushes it. `HorizontalThicknessConverter` drops the vertical part:

```xml
<Grid>
    <Border ... />
    <Grid VerticalAlignment="Center"
          Margin="{TemplateBinding Padding, Converter={StaticResource HorizontalThicknessConverter}}">
        <!-- content -->
    </Grid>
</Grid>
```

## States

An input has no hover state: the native inputs strengthen their border on focus, and fade
when disabled:

```xml
<Trigger Property="IsKeyboardFocusWithin" Value="True">
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.InputBorderElevatedBrush}}" />
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

<!-- In Style.Triggers, after the focus trigger so it wins over it -->
<Trigger Property="Validation.HasError" Value="True">
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}" />
</Trigger>
```

A user of the input drops the badge with `Validation.ErrorTemplate="{x:Null}"`; the red
border stays.

## Embedded buttons

A button inside the input (clear, open, step…) uses the `EmbeddedButton` style: a
transparent square of `HeightEmbedded` (20). Centred in the content row, which takes
no vertical padding, it fits every size, down to the 24 of `xs`.

```xml
<Grid VerticalAlignment="Center"
      Margin="{TemplateBinding Padding, Converter={StaticResource HorizontalThicknessConverter}}">
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
- focused and disabled;
- with a binding error, with and without `Validation.ErrorTemplate="{x:Null}"`.
