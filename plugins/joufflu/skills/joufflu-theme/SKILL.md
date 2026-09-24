---
name: joufflu-theme
description: >-
  Create, register and persist a custom Joufflu theme (a palette ResourceDictionary
  of joufflu:Colors / joufflu:Brushes keys), tweak the design tokens (corner
  radius, spacing, control heights, font sizes, paddings), add a theme switcher
  bound to ThemeManager, or brand an app with its own colours. Use when the user
  asks for a custom theme, brand colours, a dark variant, a theme picker or
  persisting the selected theme in a WPF app using Joufflu, or says
  "/joufflu-theme".
---

# Custom Joufflu theme

A Joufflu theme is a `ResourceDictionary` redefining the palette keys of `Light.xaml` /
`Dark.xaml`. `ThemeManager` swaps it in at runtime; every control reads it through
`DynamicResource`, so it re-themes live. The full token list and roles are in the
`joufflu` skill: `references/theming.md`.

## 1. Decide what is asked

| Request | Deliverable |
|---|---|
| "Use our brand colour", a palette | A **theme dictionary** registered with `ThemeManager` (step 2–3) |
| Rounder corners, denser UI, bigger fonts (all themes) | A **token override** dictionary merged after the core resources (step 5) |
| Let users pick | A **switcher** + persistence (step 4) |

Ask for the missing inputs only: base colour(s), light or dark, theme name.

## 2. Write the palette — `Themes/<Name>.xaml`

Every theme redefines the same 18 accent colours plus surfaces and text, **and** the
matching `SolidColorBrush`es built from them (the built-in brushes are defined inside the
theme dictionaries, so a theme must provide both). Keys it leaves out keep the built-in
value, but a complete palette avoids mismatches.

Template (fill every colour):

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu">

    <!-- Text -->
    <Color x:Key="{x:Static joufflu:Colors.ForegroundColor}">#2E3440</Color>
    <Color x:Key="{x:Static joufflu:Colors.Foreground100Color}">#70757F</Color>
    <Color x:Key="{x:Static joufflu:Colors.Foreground200Color}">#969BA3</Color>
    <!-- Borders -->
    <Color x:Key="{x:Static joufflu:Colors.BorderColor}">#D8DEE9</Color>
    <Color x:Key="{x:Static joufflu:Colors.Border100Color}">#E2E6EE</Color>
    <!-- Surfaces -->
    <Color x:Key="{x:Static joufflu:Colors.BackgroundColor}">#ECEFF4</Color>
    <Color x:Key="{x:Static joufflu:Colors.Background100Color}">#E5E9F0</Color>
    <Color x:Key="{x:Static joufflu:Colors.Background200Color}">#D8DEE9</Color>
    <!-- Accents: X / X100 / XContent for Primary, Secondary, Success, Info, Warning, Danger -->
    <Color x:Key="{x:Static joufflu:Colors.PrimaryColor}">#5E81AC</Color>
    <Color x:Key="{x:Static joufflu:Colors.Primary100Color}">#506E92</Color>
    <Color x:Key="{x:Static joufflu:Colors.PrimaryContentColor}">#FFFFFF</Color>
    <!-- … Secondary, Success, Info, Warning, Danger the same way … -->

    <!-- Brushes: one per colour above, always built with DynamicResource -->
    <SolidColorBrush x:Key="{x:Static joufflu:Brushes.ForegroundBrush}" Color="{DynamicResource {x:Static joufflu:Colors.ForegroundColor}}" />
    <SolidColorBrush x:Key="{x:Static joufflu:Brushes.Foreground100Brush}" Color="{DynamicResource {x:Static joufflu:Colors.Foreground100Color}}" />
    <!-- … one SolidColorBrush for every Color key: Foreground*, Border*, Background*, and X / X100 / XContent of each accent … -->
</ResourceDictionary>
```

Write out every line in the real file; don't leave the `…` placeholders. If the
Joufflu repository or the package source is available, copy `Joufflu/Themes/Light.xaml`
(or `Dark.xaml`) as the starting point so no key is missed. The sample themes in
`Joufflu.Samples/Themes/` (Nord, Ocean, Dracula, …) follow this layout.

Do **not** define `XSoftBrush` / `XSoftStrongBrush`: they are derived from the accent
colour automatically.

### Choosing the colours

- **Surfaces**: `Background` is furthest back; `Background100` (cards, popups) is one step
  closer to the viewer; `Background200` is hover/selection. In a light theme they get
  slightly darker in that order or `Background100` is white; in a dark theme
  `Background100` is lighter than `Background`.
- **Text**: `Foreground` must reach at least 4.5:1 contrast on `Background` and
  `Background100`; `Foreground100` is muted (≈ 3:1+); `Foreground200` is text on a selected
  row (`Background200`).
- **Accents**: `X100` is the hover/pressed fill, about 8–12 % darker (light theme) or
  lighter (dark theme) than `X`. `XContent` is the text on `X`: pick near-white or a very
  dark shade of the same hue, whichever gives ≥ 4.5:1 on both `X` and `X100`.
- Keep semantics recognisable: Success green, Info blue, Warning amber, Danger red.
  `Secondary` is a neutral, low-emphasis fill.
- Compute contrast ratios instead of eyeballing when the user cares about accessibility,
  and say which pairs fall short.

## 3. Register it

The file must be a WPF `Page` resource (default build action for `.xaml` in a WPF
project). In `App.OnStartup`, **before** `Initialize()`:

```csharp
ThemeManager.Instance.Register(
    "Ocean",
    new Uri("pack://application:,,,/MyApp;component/Themes/Ocean.xaml"),   // MyApp = assembly name
    isDark: true);

ThemeManager.Instance.Initialize();
ThemeManager.Instance.Theme = "Ocean";   // or let the user pick (step 4)
```

To make it the app's only look, just set `Theme` to it after registering. Don't merge the
theme file in `App.xaml`: `ThemeManager` owns palette insertion.

## 4. Switcher and persistence

Switcher (the app declares the converter):

```xml
<!-- xmlns:themes="clr-namespace:Joufflu.Themes;assembly=Joufflu" -->
<ComboBox ItemsSource="{Binding Themes, Source={x:Static themes:ThemeManager.Instance}}"
          SelectedItem="{Binding Theme, Source={x:Static themes:ThemeManager.Instance}, Mode=TwoWay}" />
```

or radio buttons with `conv:EnumMatchToBooleanConverter` (`ConverterParameter=<name>`,
`Mode=TwoWay`).

`ThemeManager` does not save the choice. Restore before `Initialize()` and save on change,
using the app's existing settings store (look for one before creating a new file):

```csharp
string? saved = settings.Theme;
if (!string.IsNullOrWhiteSpace(saved))
    ThemeManager.Instance.Theme = saved;

ThemeManager.Instance.PropertyChanged += (_, args) =>
{
    if (args.PropertyName == nameof(ThemeManager.Theme))
        settings.Save(theme: ThemeManager.Instance.Theme);
};

ThemeManager.Instance.Initialize();
```

## 5. Token overrides (shape, spacing, sizes)

For changes that apply whatever the palette, merge a dictionary **after** the core
resources in `App.xaml`:

```xml
<ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
<ResourceDictionary Source="Themes/Tokens.xaml" />
```

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
    xmlns:sys="clr-namespace:System;assembly=System.Runtime">

    <!-- Rounder: override the scalar AND the composite built from it -->
    <sys:Double x:Key="{x:Static joufflu:Dimensions.Radius}">8</sys:Double>
    <CornerRadius x:Key="{x:Static joufflu:Dimensions.CornerRadius}">8</CornerRadius>

    <!-- Denser controls -->
    <sys:Double x:Key="{x:Static joufflu:Dimensions.HeightMd}">28</sys:Double>
    <Thickness x:Key="{x:Static joufflu:Dimensions.PaddingMd}">10,3</Thickness>
    <Thickness x:Key="{x:Static joufflu:Dimensions.InputPaddingMd}">5,3</Thickness>
</ResourceDictionary>
```

Pairs to keep in sync: `Radius`/`CornerRadius`, `Thickness`/`BorderThickness`,
`Spacing`/`SpacingThickness`, `TitleBarHeight`/`TitleBarHeightOffset` (`0,<height>,0,0`),
`HeightEmbedded` ≈ 0.625 × `HeightMd`.

The `Joufflu.Samples` gallery has a *Customize theme* page that edits all of this live and
exports a ready-to-merge dictionary; suggest it when the user wants to experiment visually.

## 6. Verify

`dotnet build`, then run the app and flip between the new theme and Light/Dark: every
surface should change. A control that keeps its old colour usually has a hardcoded brush
or a `StaticResource` token somewhere (fix it with the `joufflu-restyle` skill).
