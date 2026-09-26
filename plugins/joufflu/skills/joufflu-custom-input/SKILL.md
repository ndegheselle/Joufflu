---
name: joufflu-custom-input
description: >-
  Build a custom WPF input control (a new Control, UserControl or templated control
  that takes a value) so it matches the Joufflu inputs: background, border and height
  from the design tokens, then Sizing.Size variants with input padding, hover/focus/
  disabled states, the validation error style and EmbeddedButton inner buttons. Use
  when the user asks to create, write or style an input, picker, field or editor of
  their own in a project that references Joufflu, when a custom input doesn't look like
  the Joufflu ones or ignores the theme or Sizing.Size, or says "/joufflu-custom-input".
---

# Build a custom input on Joufflu

Goal: the new input is indistinguishable from the Joufflu inputs next to it, in every
size, theme and state. The `joufflu` skill has the full API; `docs/toolkit/custom-input.md`
and the gallery's **Toolkit > Custom input** page (`Joufflu.Samples/Views/Toolkit/RatingInput.*`)
show the finished result.

## 1. Check it doesn't exist, then pick the base

- Look in the `joufflu` skill references first: numeric, decimal, timespan, search, combo,
  tags, file, colour, editable text and dropdown inputs already exist.
- **Wraps a native control** (a `TextBox` with a prefix, a `ComboBox` variant…): derive from
  it and base the style on the native style, `BasedOn="{StaticResource {x:Type TextBox}}"`.
  That inherits every rule below; write only the template, and keep it reading
  `Background`, `BorderBrush`, `BorderThickness` and `Padding` through `TemplateBinding`.
  Stop at step 6.
- **Anything else**: derive from `Control` and apply steps 2 to 5 yourself.

## 2. Required: background, border, height

In the style (not hard-coded in the template), always with `DynamicResource`:

```xml
<Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.BackgroundBrush}}" />
<Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.BorderBrush}}" />
<Setter Property="BorderThickness" Value="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}" />
<Setter Property="Height" Value="{DynamicResource {x:Static joufflu:Dimensions.HeightMd}}" />  <!-- one-line inputs only -->
```

The template's surface reads them from the control, with the token corner radius:

```xml
<ControlTemplate TargetType="{x:Type local:MyInput}">
    <Grid>
        <Border Background="{TemplateBinding Background}"
                BorderBrush="{TemplateBinding BorderBrush}"
                BorderThickness="{TemplateBinding BorderThickness}"
                CornerRadius="{DynamicResource {x:Static joufflu:Dimensions.CornerRadius}}" />
        <Grid Margin="{TemplateBinding Padding}"> <!-- content --> </Grid>
    </Grid>
</ControlTemplate>
```

No `#RRGGBB`, no literal thickness, radius or height anywhere.

## 3. Optional: sizes and padding

Default to `md`, one `Sizing.Size` trigger per other size. Inputs use the `InputPadding*`
keys (not the button `Padding*` keys):

```xml
<Setter Property="FontSize" Value="{DynamicResource {x:Static joufflu:Dimensions.FontSizeMd}}" />
<Setter Property="Padding" Value="{DynamicResource {x:Static joufflu:Dimensions.InputPaddingMd}}" />
<Style.Triggers>
    <Trigger Property="toolkit:Sizing.Size" Value="xs">
        <Setter Property="Height" Value="{DynamicResource {x:Static joufflu:Dimensions.HeightXs}}" />
        <Setter Property="FontSize" Value="{DynamicResource {x:Static joufflu:Dimensions.FontSizeXs}}" />
        <Setter Property="Padding" Value="{DynamicResource {x:Static joufflu:Dimensions.InputPaddingXs}}" />
    </Trigger>
    <!-- sm: HeightSm / FontSizeSm / InputPaddingSm, lg: HeightLg / FontSizeLg / InputPaddingLg -->
</Style.Triggers>
```

Put the content inside `Margin="{TemplateBinding Padding}"`, laid over the border (not
inside it), so it lines up with the text of a `TextBox`.

## 4. Optional: states and validation

Add, in this order, to `Style.Triggers` (the error must come after hover and focus):

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
<Trigger Property="Validation.HasError" Value="True">
    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}" />
</Trigger>
```

and the badge listing the errors:

```xml
<Setter Property="Validation.ErrorTemplate" Value="{DynamicResource ValidationErrorTemplate}" />
```

`ValidationErrorTemplate` draws no border: the `HasError` trigger is what turns it red.
Never hand-roll an error tooltip or a second red border. If the error border belongs to an
inner element rather than the control's `BorderBrush`, use a `ControlTemplate` trigger with
`TargetName` instead.

## 5. Optional: embedded buttons

Buttons inside the input (clear, open, step…) use `Style="{StaticResource EmbeddedButton}"`
with a `fonts:FontIcon` as content, inside the padded content grid, usually in an `Auto`
column on the right. Don't size them: `HeightEmbedded` is chosen to fit the smallest input
inside its padding. Hide a clear button when there is nothing to clear.

A `StaticResource` in a `ResourceDictionary` only resolves against that dictionary and the
ones it merges, so the dictionary holding the style merges what it references:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Styles/Natives/Actions/Button.named.xaml" />
</ResourceDictionary.MergedDictionaries>
```

## 6. Wire it up and check

- Give the style `x:Key="{x:Type local:MyInput}"` so it applies implicitly, and merge its
  dictionary where the input is used (or into the library's `Themes/Generic.xaml` with a
  `DefaultStyleKey` override for a control library).
- Value properties bind two-way by default (`FrameworkPropertyMetadataOptions.BindsTwoWayByDefault`),
  like the native inputs.
- Check next to a `TextBox`, and say what you checked: each `Sizing.Size` set on a parent
  panel, Light and Dark, hover, focus, disabled, and a binding error with and without
  `Validation.ErrorTemplate="{x:Null}"`.
