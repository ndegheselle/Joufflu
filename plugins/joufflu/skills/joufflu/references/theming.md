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
resources (a registered theme may also carry dimensions of its own; this dictionary still
wins over it):

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
    <ResourceDictionary Source="Themes/Overrides.xaml" />
</ResourceDictionary.MergedDictionaries>
```

```xml
<!-- Themes/Overrides.xaml — xmlns:sys="clr-namespace:System;assembly=System.Runtime" -->
<!-- Rounder buttons only: the controls read the category keys, not the global Radius -->
<CornerRadius x:Key="{x:Static joufflu:Dimensions.ActionRadius}">8</CornerRadius>
```

The controls read the per-category shape (`DisplayRadius`/`InputRadius`/`ActionRadius` as
`CornerRadius`, `…Thickness` as `Thickness`, `DisplayPadding`), not the global `Radius` or
`Thickness`, which are for an app's own layout. A rounder UI overrides the three categories. When
overriding `TitleBarHeight`, also override `TitleBarHeightOffset`, since it is baked at load. A large radius gives a pill: the
controls clamp it to half their height. The `Joufflu.Samples` gallery's *Customize theme* window (palette button in its title bar)
generates a ready-to-merge dictionary interactively.

## Using tokens

Keys are `ComponentResourceKey` statics (typos fail the build). Always `DynamicResource`:

```xml
<!-- A panel of your own is a display: same tokens as Card -->
<Border Background="{DynamicResource {x:Static joufflu:Brushes.DisplayBaseBrush}}"
        BorderBrush="{DynamicResource {x:Static joufflu:Brushes.DisplayBorderBrush}}"
        BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.DisplayThickness}}"
        CornerRadius="{DynamicResource {x:Static joufflu:Dimensions.DisplayRadius}}"
        Padding="{DynamicResource {x:Static joufflu:Dimensions.DisplayPadding}}" />
<GradientStop Color="{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}" Offset="0" />
```

From C#: `element.SetResourceReference(Border.BackgroundProperty, Joufflu.Brushes.DisplayBaseBrush);`
(`SetResourceReference` keeps it dynamic; `FindResource` would freeze the current value).

## Colours — `joufflu:Colors.XColor` (Color) / `joufflu:Brushes.XBrush` (SolidColorBrush)

Use the brush in UI; the colour is for gradients and animations.

| Group | X | Used for |
|---|---|---|
| Bases (global) | `Base0` | The page; default base of an action |
| | `Base1` | A card; default base of a display and an input |
| | `Base2` | A contrast inside a card, for an app's own layout |
| Display | `DisplayBase` (colour) | Card, popup, tooltip, expander, group box, frame of a list/tree/grid/menu/tab control. Falls back to `Base1` |
| Input | `InputBase` (colour) | Text box, combo box, check box, radio button, slider. Falls back to `Base1` |
| Action | `ActionBase` (colour) | Button, toggle button. Falls back to `Base0` |
| Every category | derived brushes | `XBorderBrush` (base 10 % toward `Foreground`: the border, and a part on the base: header, track, tag), `XElevatedBrush` (border 10 % further: a border on a `XBorder` background, the border of an interacted control), `XInteractionBrush` (base 10 %: the background of an interacted control) |
| Text | `Foreground` | Default text and icons |
| Accents | `Primary`, `Secondary`, `Success`, `Info`, `Warning`, `Danger` | Each has `X` (fill) and `XForeground` (text/icons on the fill) as colours, and the brush `XInteractionBrush` (hover/pressed fill), `X` mixed 10 % toward `XForeground` |

The controls never read the global bases: they read a category, so a custom control picks the
category it belongs to. The category base is its only colour, and optional; a theme that leaves it
out gets the global one through a `ColorFallback` (public, in `Joufflu`). A theme overrides one category alone:
`<Color x:Key="{x:Static joufflu:Colors.InputBaseColor}">#FFFFFF</Color>`.

State rules, the same for every control: an interaction turns the border to `XElevatedBrush` and the
background to `XInteractionBrush` (a part with no border at rest keeps none). A display has none of its
own (`DisplayInteractionBrush` is for an app's informative hover); an input is interacted on focus, no
hover; an action on hover, no focus border, and translates down when pressed (no colour); a grip
(scroll bar thumb, grid splitter, slider thumb) turns `ActionElevatedBrush` hovered or dragged; the items of a
display (rows, menu items, tabs, nav entries) are ghost actions (no fill at rest); disabled is
`DisabledOpacity`; selected and checked are `Primary`. No theme defines a hover colour.

Removed (breaking): `Stroke`, `Surface1`/`Surface2` (now `Base1`/`Base2`), `Base` (now `Base0`), the
category `Surface1`/`Surface2`/`Border` colours (only `XBaseColor` remains), `XContent` (now
`XForeground`), `XElevatedBrush` of an accent (now `XInteractionBrush`), `InputBorderElevatedBrush`
(now `InputElevatedBrush`), `ActionBorderElevatedBrush` (now `ActionElevatedBrush`; the old
`ActionElevatedBrush` hover is now `ActionInteractionBrush`).

Also available, derived automatically from the accent colour (not per theme):
`Brushes.XSoftBrush` (≈14 % tint: soft button, selected row) and `Brushes.XSoftInteractionBrush`
(≈24 %, its hover) for each accent.

`Colors.DisabledOpacity` (double, `0.5`): opacity of a disabled control.
`Colors.MutedOpacity` (double, `0.65`): opacity of muted text (placeholders, hints, descriptions).

Pairing rule: text on an `X` fill uses `XForeground`; text on a base uses `Foreground`.
`Danger` is also the colour of validation errors.

## Dimensions — `joufflu:Dimensions.*`

| Key | Type | Default | Role |
|---|---|---|---|
| `Thickness` | Thickness | 1 | Global border (app layout) |
| `Radius` | CornerRadius | 4 | Global corner radius (app layout) |
| `DisplayRadius` / `InputRadius` / `ActionRadius` | CornerRadius | 4 | Category radius, each corner clamped to half the control height (large = pill) |
| `DisplayThickness` / `InputThickness` / `ActionThickness` | Thickness | 1 | Category border |
| `DisplayPadding` | Thickness | 12 | Padding inside a display (card) |
| `DisplayPaddingSm` | Thickness | 4 | Padding inside a compact display (popup, menu, tooltip, badge) |
| `Spacing` | Thickness | 12 | Standard gap between siblings and page padding |
| `TitleBarHeight` | double | 30 | Custom title bar height |
| `TitleBarHeightOffset` | Thickness | 0,30,0,0 | Margin pushing content below an overlapping title bar |
| `HeightXs` / `Sm` / `Md` / `Lg` | double | 24 / 28 / 32 / 40 | Single-line control heights |
| `HeightEmbedded` | double | 20 | Small clear button inside an input |
| `FontSizeXs` / `Sm` / `Md` / `Lg` / `Xl` | double | 11 / 12 / 13 / 16 / 24 | Type scale (`Md` = body) |
| `ActionPaddingXs` / `Sm` / `Md` / `Lg` | Thickness | 6,2 / 9,3 / 12,4 / 18,6 | Buttons and content-shaped controls |
| `InputPaddingXs` / `Sm` / `Md` / `Lg` | Thickness | 3,2 / 4.5,3 / 6,4 / 9,6 | Text inputs |

To use only some sides of a scalar with live updates, see `Derive` in
[toolkit.md](toolkit.md).
