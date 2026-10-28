---
name: joufflu-theme
description: >-
  Create, register and persist a custom Joufflu theme (a palette ResourceDictionary
  of joufflu:Colors keys), tweak the design tokens (corner
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

Every theme redefines the same 18 accent colours plus surfaces and text. It may also
override `Dimensions.*` keys (see §5) when its look includes them: `ThemeManager` merges
it right after `Resources.xaml`, so they win over the defaults.
The brushes are defined once and `ThemeManager` builds them from whichever theme it
applies. Keys it leaves out keep the built-in value, but a complete palette avoids
mismatches.

Template (fill every colour):

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu">

    <!-- Text -->
    <Color x:Key="{x:Static joufflu:Colors.ForegroundColor}">#2E3440</Color>
    <!-- Strokes: borders, separators, icon strokes -->
    <Color x:Key="{x:Static joufflu:Colors.StrokeColor}">#D8DEE9</Color>
    <!-- Surfaces -->
    <Color x:Key="{x:Static joufflu:Colors.BaseColor}">#ECEFF4</Color>
    <Color x:Key="{x:Static joufflu:Colors.Surface1Color}">#E5E9F0</Color>
    <Color x:Key="{x:Static joufflu:Colors.Surface2Color}">#DADEE5</Color>
    <!-- Accents: X / XContent for Primary, Secondary, Success, Info, Warning, Danger -->
    <Color x:Key="{x:Static joufflu:Colors.PrimaryColor}">#5E81AC</Color>
    <Color x:Key="{x:Static joufflu:Colors.PrimaryContentColor}">#FFFFFF</Color>
    <!-- … Secondary, Success, Info, Warning, Danger the same way … -->
</ResourceDictionary>
```

Write out every line in the real file; don't leave the `…` placeholders. If the
Joufflu repository or the package source is available, copy `Joufflu/Themes/Light.xaml`
(or `Dark.xaml`) as the starting point so no key is missed. The sample themes in
`Joufflu.Samples/Themes/` (Nord, Ocean, Dracula, …) follow this layout.

Do **not** define brushes (`XBrush`, `XSoftBrush`, `XSoftElevatedBrush`): `ThemeManager`
builds them from the colours of every theme it applies, and those take precedence over any
brush the theme defines.

### Choosing the colours

- **Surfaces**: `Base` is the page and what a button is filled with; `Surface1` (cards, popups, inputs,
  rows) sits on it; `Surface2` is only the contrast inside a control (a table or group header, a
  card secondary, a track). Hover, pressed and the strong and focus strokes are not colours you
  define: they are a colour mixed 10-40 % toward `Foreground`, so the direction is right in a light
  and in a dark theme.
- **Text**: `Foreground` must reach at least 4.5:1 contrast on `Base`, `Surface1` and
  `Surface2`. Muted and disabled text is not a colour: it dims `Foreground` with
  `MutedOpacity` and `DisabledOpacity`.
- **Accents**: `XContent` is the text on `X`: pick near-white or a very dark shade of the same
  hue, whichever gives ≥ 4.5:1 on `X`. The hover, `XElevatedBrush`, is not a colour you define: it is
  `X` mixed 10 % toward `XContent`, so a dark fill with light content lightens and a pale one darkens.
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

Put them in the theme file when they belong to that theme. For changes that apply
whatever the palette, merge a dictionary **after** the core
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
