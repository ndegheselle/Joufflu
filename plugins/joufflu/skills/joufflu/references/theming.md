# Theming and design tokens (`Joufflu`)

## ThemeManager (`Joufflu.Themes`)

`ThemeManager.Instance` is an `ObservableObject` owning the active palette. It inserts the
matching dictionary into `Application.Resources` at runtime; never merge
`Themes/Light.xaml` / `Dark.xaml` yourself.

| Member | Purpose |
|---|---|
| `Initialize()` | Call once in `OnStartup`, before the first window. Applies `Theme`. |
| `Theme` (string) | Active theme name. Set it anywhere, the UI re-themes live. |
| `ThemeManager.System` / `.Light` / `.Dark` | Built-in names. `System` follows the Windows app theme live. |
| `IsDark` | Whether the theme actually on screen is dark. |
| `Themes` | `ReadOnlyObservableCollection<string>` of every selectable name (built-ins + registered). |
| `Register(name, Uri source, bool isDark = false)` / `Register(name, ResourceDictionary, bool isDark = false)` | Add or replace a custom theme. Re-registering the active one re-applies it live. |
| `Unregister(name)` | Remove a custom theme (built-ins can't be removed). |
| `GetDictionary(name)` | The dictionary of a theme. |

The selection is **not persisted**: the app does it.

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // 1. Register custom themes BEFORE Initialize so a saved custom selection applies on launch
    ThemeManager.Instance.Register("Ocean",
        new Uri("pack://application:,,,/MyApp;component/Themes/Ocean.xaml"), isDark: true);

    // 2. Restore the saved name
    string? saved = LoadThemeSetting();                 // app's own settings store
    if (!string.IsNullOrWhiteSpace(saved))
        ThemeManager.Instance.Theme = saved;

    // 3. Save on change
    ThemeManager.Instance.PropertyChanged += (_, args) =>
    {
        if (args.PropertyName == nameof(ThemeManager.Theme))
            SaveThemeSetting(ThemeManager.Instance.Theme);
    };

    ThemeManager.Instance.Initialize();
    // … show the main window
}
```

### Theme switcher

No dedicated control: bind to `ThemeManager.Instance.Theme`. The `EnumMatch` converter
must be declared by the app (it is not a library resource):

```xml
<!-- xmlns:themes="clr-namespace:Joufflu.Themes;assembly=Joufflu"
     xmlns:conv="clr-namespace:Joufflu.Converters;assembly=Joufflu" -->
<conv:EnumMatchToBooleanConverter x:Key="EnumMatch" />

<RadioButton Content="Dark"
    IsChecked="{Binding Theme, Source={x:Static themes:ThemeManager.Instance},
        Converter={StaticResource EnumMatch}, ConverterParameter=Dark, Mode=TwoWay}" />
```

Or a `ComboBox`:

```xml
<ComboBox ItemsSource="{Binding Themes, Source={x:Static themes:ThemeManager.Instance}}"
          SelectedItem="{Binding Theme, Source={x:Static themes:ThemeManager.Instance}, Mode=TwoWay}" />
```

## Overriding tokens without a new theme

Tweak dimensions (or colours for every theme) in a dictionary merged **after** the core
resources:

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
    <ResourceDictionary Source="Themes/Overrides.xaml" />
</ResourceDictionary.MergedDictionaries>
```

```xml
<!-- Themes/Overrides.xaml — xmlns:sys="clr-namespace:System;assembly=System.Runtime" -->
<sys:Double x:Key="{x:Static joufflu:Dimensions.Radius}">8</sys:Double>
<CornerRadius x:Key="{x:Static joufflu:Dimensions.CornerRadius}">8</CornerRadius>
```

When overriding a scalar, also override the composite built from it (`Radius` →
`CornerRadius`, `Thickness` → `BorderThickness`, `Spacing` → `SpacingThickness`), since
composites are baked at load. The `Joufflu.Samples` gallery page *Customize theme*
generates a ready-to-merge dictionary interactively.

## Using tokens

Keys are `ComponentResourceKey` statics (typos fail the build). Always `DynamicResource`:

```xml
<Border Background="{DynamicResource {x:Static joufflu:Brushes.Background100Brush}}"
        BorderBrush="{DynamicResource {x:Static joufflu:Brushes.BorderBrush}}"
        BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}"
        CornerRadius="{DynamicResource {x:Static joufflu:Dimensions.CornerRadius}}"
        Padding="{DynamicResource {x:Static joufflu:Dimensions.SpacingThickness}}" />
<GradientStop Color="{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}" Offset="0" />
```

From C#: `element.SetResourceReference(Border.BackgroundProperty, Joufflu.Brushes.Background100Brush);`
(`SetResourceReference` keeps it dynamic; `FindResource` would freeze the current value).

## Colours — `joufflu:Colors.XColor` (Color) / `joufflu:Brushes.XBrush` (SolidColorBrush)

Use the brush in UI; the colour is for gradients and animations.

| Group | X | Used for |
|---|---|---|
| Surfaces | `Background` | Window/page background (furthest back) |
| | `Background100` | Elevated: cards, popups, drop-downs, rows |
| | `Background200` | Transient: hovered ghost button, hovered/selected row, slider track |
| | `Border` | Default border of framed controls |
| | `Border100` | Stronger border: hover/focus, separators |
| Text | `Foreground` | Default text and icons |
| | `Foreground100` | Muted text, placeholders, hints (`Muted` style) |
| | `Foreground200` | Text on a selected row |
| Accents | `Primary`, `Secondary`, `Success`, `Info`, `Warning`, `Danger` | Each has `X` (fill), `X100` (hover/pressed fill), `XContent` (text/icons on the fill) |

Also available, derived automatically from the accent colour (not per theme):
`Brushes.XSoftBrush` (≈14 % tint: soft button, selected row) and `Brushes.XSoftStrongBrush`
(≈24 %, its hover) for each accent.

`Colors.DisabledOpacity` (double, `0.5`): opacity of a disabled control.

Pairing rule: text on an `X` fill uses `XContent`; text on surfaces uses `Foreground*`.
`Danger` is also the colour of validation errors.

## Dimensions — `joufflu:Dimensions.*`

| Key | Type | Default | Role |
|---|---|---|---|
| `Thickness` | double | 1 | Base border width |
| `BorderThickness` | Thickness | 1 | Uniform border |
| `Radius` | double | 4 | Base corner radius |
| `CornerRadius` | CornerRadius | 4 | Uniform radius |
| `Spacing` | double | 12 | Standard gap between siblings |
| `SpacingThickness` | Thickness | 12 | Standard page padding |
| `TitleBarHeight` | double | 30 | Custom title bar height |
| `TitleBarHeightOffset` | Thickness | 0,30,0,0 | Margin pushing content below an overlapping title bar |
| `HeightXs` / `Sm` / `Md` / `Lg` | double | 24 / 28 / 32 / 40 | Single-line control heights |
| `HeightEmbedded` | double | 20 | Small clear button inside an input |
| `FontSizeXs` / `Sm` / `Md` / `Lg` / `Xl` | double | 11 / 12 / 13 / 16 / 24 | Type scale (`Md` = body) |
| `PaddingXs` / `Sm` / `Md` / `Lg` | Thickness | 6,2 / 9,3 / 12,4 / 18,6 | Buttons and content-shaped controls |
| `InputPaddingXs` / `Sm` / `Md` / `Lg` | Thickness | 3,2 / 4.5,3 / 6,4 / 9,6 | Text inputs |

To use only some sides of a scalar with live updates, see `Derive` in
[toolkit.md](toolkit.md).
