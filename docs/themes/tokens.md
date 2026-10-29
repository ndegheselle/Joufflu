---
title: Design tokens
parent: Themes
nav_order: 3
---

# Design tokens

A theme is a collection of the keys below. Every control style reads them
through `DynamicResource`, so a theme only has to redefine what it wants to change — anything it
leaves out keeps the built-in value.

A theme registered with `ThemeManager` can define both colours and dimensions: it is merged right after
`Resources.xaml`, so its `Dimensions` keys (and `Colors.DisabledOpacity` / `MutedOpacity`) override the built-in
ones, and change with the theme. A dictionary merged after `Resources.xaml` still overrides
every theme.

Keys are `ComponentResourceKey` statics rather than strings, so a renamed key breaks the build
instead of silently falling back at runtime:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"

<!-- Always DynamicResource: a theme swap, or the customizer editing a
     dimension, has to reach controls already on screen. -->
<Border
    Background="{DynamicResource {x:Static joufflu:Brushes.DisplaySurface1Brush}}"
    BorderBrush="{DynamicResource {x:Static joufflu:Brushes.DisplayBorderBrush}}"
    BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.DisplayBorderThickness}}"
    CornerRadius="{DynamicResource {x:Static joufflu:Dimensions.DisplayCornerRadius}}" />

<!-- The raw Color is there for what a brush cannot express -->
<GradientStop Offset="0" Color="{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}" />
```

See [Theme](theme.html) for swapping palettes at runtime, [Customize theme](customize-theme.html)
for editing these values live, and [Derived dimensions](../toolkit/derived-dimensions.html) for scaling one
per side or per corner.

## Colors

Each colour exists twice: `Joufflu.Colors.XColor` holds the `Color`, `Joufflu.Brushes.XBrush` the
`SolidColorBrush` built from it. Use the brush in the UI; the colour is there for gradients,
animations and anything that needs the raw value.

### Surfaces

Four colours a theme defines for the neutrals. The controls do not read them directly but through the three
categories below, which fall back to them unless the theme gives a category colour of its own. Use
them for an app's own layout.

| Key | Used for |
|---|---|
| `Base` | The page, and by default what an action is filled with, so on a display it reads as set into it. An action is not put directly on the page. |
| `Surface1` | A card, and by default the background of a display and of an input. |
| `Surface2` | An elevated part inside a card, and by default the `Surface2` of every category. |
| `Stroke` | A border or a separator, and by default the border of every category. |

### Categories

Every control belongs to one of three categories, each with a background (`Surface1`), an elevated
background (`Surface2`) and a `Border`. The category colours are optional: a theme that leaves one
out gets the global colour in brackets, through a `ColorFallback`. The states are always the same,
derived by mixing a colour toward the `Foreground`, so no theme defines a hover colour. Disabled is
`DisabledOpacity`, selected and checked are the primary.

**Display**: what shows content without being interacted with: card, popup, tooltip, expander,
group box, and the frame of a list, tree, grid, menu or tab control. No states. Its items (rows,
menu items, tabs, navigation entries) are actions without a fill at rest.

| Key | Used for |
|---|---|
| `DisplaySurface1` | The background (`Surface1`). |
| `DisplaySurface2` | An elevated part: a header, a nested area (`Surface2`). |
| `DisplayBorder` | The border and the separators (`Stroke`). |

**Input**: what takes a value: text box, combo box, check box, radio button, slider. No hover;
focus is a strong border.

| Key | Used for |
|---|---|
| `InputSurface1` | The background (`Surface1`). |
| `InputSurface2` | An elevated part inside: a tag, a track (`Surface2`). |
| `InputBorder` | The border (`Stroke`). |
| `InputBorderElevatedBrush` | Derived, the border mixed 40% toward `Foreground`: the border of a focused input. |

**Action**: what runs something: button, toggle button. Hover is a stronger background, pressed a
translation down rather than a colour; an action and an item of a display have no focus border.

| Key | Used for |
|---|---|
| `ActionSurface1` | The background (`Base`). |
| `ActionSurface2` | An elevated part inside (`Surface2`). |
| `ActionBorder` | The border (`Stroke`). |
| `ActionElevatedBrush` | Derived, the fill mixed 10% toward `Foreground`: a hovered action or display item. |
| `ActionBorderElevatedBrush` | Derived: a grip hovered or dragged (scroll bar thumb, grid splitter, slider thumb). |

A theme overrides a category by defining its colour, for example inputs set apart from the cards:

```xml
<Color x:Key="{x:Static joufflu:Colors.InputSurface1Color}">#FFFFFF</Color>
<Color x:Key="{x:Static joufflu:Colors.ActionBorderColor}">#A1A1AA</Color>
```

`ColorFallback` is public: a library of your own can declare an optional colour the same way (see
`Joufflu/Styles/Brushes.xaml`).

### Text

| Key | Used for |
|---|---|
| `Foreground` | Default text and icon colour, on `Base`, `Surface1` or `Surface2`. |

### Accents

Six semantic families sharing one shape — the three keys of each are shown side by side below:
`X` is the fill, `XContent` the text and icons drawn on top of it: the two colours a theme defines.
The others are brushes only, derived from the fill so no theme defines them. `XElevatedBrush` is the
fill while hovered or pressed: the fill mixed 10% toward its own `XContent`, so a dark fill with light
content lightens and a pale one darkens, in a light and in a dark theme alike. `XSoftBrush` is the fill at low opacity (soft buttons,
selected rows, outline hover) and `XSoftElevatedBrush` its hover, drawn with the fill as text, as a soft
button is.

<table>
<tbody>
<tr><td style="white-space:nowrap"><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#18181B;color:#FAFAFA">Aa</div><code style="font-size:11px">Primary</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#27272A;color:#FAFAFA">Aa</div><code style="font-size:11px">PrimaryElevated</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#FAFAFA;color:#FAFAFA">&nbsp;</div><code style="font-size:11px">PrimaryContent</code></div><div style="display:inline-block;text-align:center;margin-left:8px;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(24,24,27,.14);color:#18181B">Aa</div><code style="font-size:11px">PrimarySoft</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(24,24,27,.24);color:#18181B">Aa</div><code style="font-size:11px">PrimarySoftElevated</code></div></td><td>Main call to action: the default filled button, the selected tab or navigation entry.</td></tr>
<tr><td style="white-space:nowrap"><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#E4E4E7;color:#27272A">Aa</div><code style="font-size:11px">Secondary</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#D4D4D8;color:#27272A">Aa</div><code style="font-size:11px">SecondaryElevated</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#27272A;color:#27272A">&nbsp;</div><code style="font-size:11px">SecondaryContent</code></div><div style="display:inline-block;text-align:center;margin-left:8px;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(228,228,231,.08);color:#E4E4E7">Aa</div><code style="font-size:11px">SecondarySoft</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(228,228,231,.14);color:#E4E4E7">Aa</div><code style="font-size:11px">SecondarySoftElevated</code></div></td><td>Neutral filled action, for a button that must read as a button without competing with <code>Primary</code>.</td></tr>
<tr><td style="white-space:nowrap"><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#00D390;color:#004C39">Aa</div><code style="font-size:11px">Success</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#00BD81;color:#004C39">Aa</div><code style="font-size:11px">SuccessElevated</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#004C39;color:#004C39">&nbsp;</div><code style="font-size:11px">SuccessContent</code></div><div style="display:inline-block;text-align:center;margin-left:8px;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(0,211,144,.14);color:#00D390">Aa</div><code style="font-size:11px">SuccessSoft</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(0,211,144,.24);color:#00D390">Aa</div><code style="font-size:11px">SuccessSoftElevated</code></div></td><td>Confirmation: success toast, valid state, positive badge.</td></tr>
<tr><td style="white-space:nowrap"><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#3B82F6;color:#172554">Aa</div><code style="font-size:11px">Info</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#1D6FF4;color:#172554">Aa</div><code style="font-size:11px">InfoElevated</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#172554;color:#172554">&nbsp;</div><code style="font-size:11px">InfoContent</code></div><div style="display:inline-block;text-align:center;margin-left:8px;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(59,130,246,.14);color:#3B82F6">Aa</div><code style="font-size:11px">InfoSoft</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(59,130,246,.24);color:#3B82F6">Aa</div><code style="font-size:11px">InfoSoftElevated</code></div></td><td>Neutral information: info toast, informative badge.</td></tr>
<tr><td style="white-space:nowrap"><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#EEAF00;color:#411E03">Aa</div><code style="font-size:11px">Warning</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#D69D00;color:#411E03">Aa</div><code style="font-size:11px">WarningElevated</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#411E03;color:#411E03">&nbsp;</div><code style="font-size:11px">WarningContent</code></div><div style="display:inline-block;text-align:center;margin-left:8px;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(238,175,0,.14);color:#EEAF00">Aa</div><code style="font-size:11px">WarningSoft</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(238,175,0,.24);color:#EEAF00">Aa</div><code style="font-size:11px">WarningSoftElevated</code></div></td><td>Something needs attention but nothing is broken yet.</td></tr>
<tr><td style="white-space:nowrap"><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#FF627D;color:#4D0218">Aa</div><code style="font-size:11px">Danger</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#FF3E5F;color:#4D0218">Aa</div><code style="font-size:11px">DangerElevated</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:#4D0218;color:#4D0218">&nbsp;</div><code style="font-size:11px">DangerContent</code></div><div style="display:inline-block;text-align:center;margin-left:8px;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(255,98,125,.14);color:#FF627D">Aa</div><code style="font-size:11px">DangerSoft</code></div><div style="display:inline-block;text-align:center;margin-right:6px"><div style="width:78px;height:34px;line-height:34px;border-radius:4px;border:1px solid rgba(128,128,128,.4);background:rgba(255,98,125,.24);color:#FF627D">Aa</div><code style="font-size:11px">DangerSoftElevated</code></div></td><td>Destructive action and validation errors — a delete button, the border of a field in error.</td></tr>
</tbody>
</table>

The swatches show the built-in **Light** palette; every theme redefines the same seventeen keys (`Foreground`, the four surfaces and the twelve accent colours).
The sample gallery renders them live against whichever theme is selected.

### State

| Key | Used for |
|---|---|
| `DisabledOpacity` | Opacity applied to a whole control while `IsEnabled` is `False` (`0.5`). Disabled controls fade rather than switch to a dedicated palette, so a single key covers every control. |
| `MutedOpacity` | Opacity of muted text: placeholders, hints, descriptions, captions (`0.65`). Muted text is dimmed rather than given its own colour, so it follows the foreground it sits in. |

## Dimensions

`Joufflu.Dimensions` holds the metrics. The values in parentheses are the built-in defaults.

### Shape

A scalar plus the ready made `Thickness`/`CornerRadius` built from it. Bind the scalar through
[`Derive`](../toolkit/derived-dimensions.html) when you need only some sides or corners, the composite one
otherwise.

| Key | Type | Used for |
|---|---|---|
| `Thickness` | `double` | Base border width (`1`). |
| `BorderThickness` | `Thickness` | `Thickness` on all four sides, for a uniform border. |
| `Radius` | `double` | Base corner radius (`4`). |
| `CornerRadius` | `CornerRadius` | `Radius` on all four corners, for a uniformly rounded border. |
| `DisplayRadius`, `InputRadius`, `ActionRadius` | `double` | The radius of each category (`4`). The controls clamp it to half their height, so a large one gives a pill. |
| `DisplayCornerRadius`, `InputCornerRadius`, `ActionCornerRadius` | `CornerRadius` | The radius of each category on all four corners. |
| `DisplayBorderThickness`, `InputBorderThickness`, `ActionBorderThickness` | `Thickness` | The border of each category (`1`). |
| `DisplayPadding` | `Thickness` | The padding inside a display (`12`). An action is padded by `ActionPaddingXs`…`ActionPaddingLg`, an input by `InputPaddingXs`…`InputPaddingLg`; a button or an input takes only their horizontal part, its height coming from its size, while a row, a menu item or a tab takes both. |
| `DisplayPaddingSm` | `Thickness` | The padding inside a compact display: a popup, a menu, a tooltip, a badge (`4`, a third of `DisplayPadding` in the customizer). |

The global `Radius`, `CornerRadius`, `Thickness` and `BorderThickness` are for an app's own layout:
the controls read the category keys. Override a category key in a dictionary merged after
`Resources.xaml` (or in a registered theme) to reshape that category alone:

```xml
<CornerRadius x:Key="{x:Static joufflu:Dimensions.ActionCornerRadius}">16</CornerRadius>
<system:Double x:Key="{x:Static joufflu:Dimensions.ActionRadius}">16</system:Double>
```

### Layout

Distances between things rather than inside them.

| Key | Type | Used for |
|---|---|---|
| `Spacing` | `double` | Standard gap between sibling elements (`12`). Feed it to [`Spacing.Gap`](../toolkit/spacing.html). |
| `SpacingThickness` | `Thickness` | `Spacing` on all four sides, the standard page padding. |
| `TitleBarHeight` | `double` | Height of the custom window title bar (`30`). |
| `TitleBarHeightOffset` | `Thickness` | `Top` only, used as a margin to push content below an overlapping title bar. |

### Control heights

Vertical size of single line controls. [`Sizing.Size`](../toolkit/sizing.html) picks one of the four; `md` is
the default written in the base styles, so only `xs`/`sm`/`lg` appear in the size triggers.

| Key | Value | Used for |
|---|---|---|
| `HeightXs` | `24` | Dense tables and toolbars. |
| `HeightSm` | `28` | |
| `HeightMd` | `32` | The default. |
| `HeightLg` | `40` | Prominent, touch friendly controls. |
| `HeightEmbedded` | `20` | `0.625` of `HeightMd`, kept in sync by the theme customizer. The small clear button embedded inside a search box, combo box or numeric input. |

### Font sizes

A `TextBlock` picks one through [`Sizing.Size`](../natives/typography.html) (`xs` to `xl`; `md` keeps
the inherited body size), like the controls do, so restyling the scale restyles the text.

| Key | Value |
|---|---|
| `FontSizeXs` | `11` |
| `FontSizeSm` | `12` |
| `FontSizeMd` | `13` (the default body size) |
| `FontSizeLg` | `16` |
| `FontSizeXl` | `24` |

### Paddings

Space inside a control, one `Thickness` per size. Two families: buttons and anything content shaped
use `ActionPaddingXs`…`ActionPaddingLg`, text inputs use `InputPadding*`, whose halved horizontal keeps the caret closer to
the border.

| Key | Value | Input key | Value |
|---|---|---|---|
| `ActionPaddingXs` | `6,2` | `InputPaddingXs` | `3,2` |
| `ActionPaddingSm` | `9,3` | `InputPaddingSm` | `4.5,3` |
| `ActionPaddingMd` | `12,4` | `InputPaddingMd` | `6,4` |
| `ActionPaddingLg` | `18,6` | `InputPaddingLg` | `9,6` |

`ActionPaddingMd` and `InputPaddingMd` are the defaults.

See the **Design tokens** page in the sample gallery for the same reference with live swatches.
