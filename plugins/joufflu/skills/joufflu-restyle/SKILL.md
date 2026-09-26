---
name: joufflu-restyle
description: >-
  Move existing WPF XAML views onto the Joufflu design system: replace hardcoded
  colours, sizes and margins with Joufflu tokens and attached properties, remove
  local styles that fight the implicit native styles, and swap hand-made controls
  (custom numeric boxes, search boxes, tooltips, MessageBox dialogs, drag-and-drop
  handlers, icon images) for their Joufflu equivalents. Use when the user asks to
  restyle, clean up, modernise, "make theme-aware", fix dark mode, or audit a view
  in a project that references Joufflu, or when a control doesn't follow the theme
  switch; or says "/joufflu-restyle".
---

# Restyle a view onto Joufflu

Goal: the view looks like the rest of Joufflu, follows every theme switch, and keeps its
behaviour. Change styling only; don't touch bindings, names or logic unless swapping a
control requires it. The `joufflu` skill has the full API (`references/*.md`).

## 1. Scope and prerequisites

- Confirm the project references `Joufflu` and that `App.xaml` merges
  `pack://application:,,,/Joufflu;component/Resources.xaml` and calls
  `ThemeManager.Instance.Initialize()`. If not, fix the wiring first (see the `joufflu`
  skill, section 3); otherwise nothing below has any effect.
- Work on the file(s) the user named. For "the whole app", list the candidate views with
  their hit counts from step 2 and agree on an order before editing many files.

## 2. Audit

Search the XAML (and code-behind that sets visuals) for:

| Pattern | Problem |
|---|---|
| `#RRGGBB`, named colours (`White`, `Gray`, `Red`…) in `Background`/`Foreground`/`BorderBrush`/`Fill`/`Stroke`/`Color` | Doesn't follow the theme |
| `new SolidColorBrush(`, `Brushes.White`, `Colors.` in C# | Same |
| `{StaticResource …}` pointing at a Joufflu token | Frozen at load; theme switch ignored |
| Local `<Style TargetType="Button">` / `TextBox`… without `BasedOn` | Replaces the Joufflu style entirely |
| `Margin` on every child of a panel | `Spacing.Gap` |
| Fixed `Height="32"`, `FontSize="…"`, `Padding` on controls | `Sizing.Size` / typography styles |
| `CornerRadius="4"`, `BorderThickness="1"` literals | Tokens |
| `ToolTip="…"` | `toolkit:Tooltip.Content` |
| `MessageBox.Show`, extra modal `Window`s | Overlays (`Confirm`, `OverlayViewModel`) |
| Status text / popups for success/errors | Toasts |
| `AllowDrop` + `Drop`/`DragOver` handlers, `DoDragDrop` in `MouseMove` | `DropTarget` / `DragSource` |
| `Image` icons or Segoe MDL2/Fluent glyphs | `fonts:FontIcon` + `LucideFontIcons` |
| Hand-rolled numeric/search/tag/colour/file inputs | `Joufflu.Inputs` controls |

Report the findings briefly before a large edit.

## 3. Replacement rules

Add the namespaces you use to the root element:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
```

### Colours → semantic brushes

Map by **role**, not by closest hue:

| Original role | Token |
|---|---|
| Window/page background | Remove it (the `Window` style sets it) or `Brushes.BackgroundBrush` |
| Panel/card/popup background | `<Border Style="{StaticResource Card}">` or `Brushes.Background100Brush` |
| Inset panel inside a card | `CardSecondary` / `Brushes.BackgroundBrush` |
| Hover / selected row | `Brushes.Background200Brush` (usually already done by the native style) |
| Normal text | Remove (inherits `Foreground`) |
| Grey/secondary text | `Style="{StaticResource Muted}"` or `Brushes.Foreground100Brush` |
| Borders, separators | `Brushes.BorderBrush` / `Brushes.Border100Brush` |
| Brand / main action | `Brushes.PrimaryBrush`, text on it `PrimaryContentBrush` |
| Green OK, blue info, orange warning, red error/delete | `Success*`, `Info*`, `Warning*`, `Danger*` |
| Light tinted highlight | `Brushes.XSoftBrush` |

```xml
<!-- before -->
<Border Background="#FFFFFF" BorderBrush="#DDDDDD" BorderThickness="1" CornerRadius="4" Padding="12">
<!-- after -->
<Border Style="{StaticResource Card}">
```

```xml
Foreground="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}"
```

Always `DynamicResource`. In C#: `element.SetResourceReference(Control.ForegroundProperty,
Joufflu.Brushes.DangerBrush);`.

### Buttons

Remove custom button styles/templates that only recolour. Pick by intent:
main action `PrimaryButton`; neutral `SecondaryButton`; toolbar/low emphasis `GhostButton`;
destructive `DangerButton`; secondary semantic actions `Soft*` or `Outline*`. Icon-only:
`toolkit:Sizing.IsSquare="True"` + a `FontIcon` + `toolkit:Tooltip.Content`.

### Layout and sizes

- Uniform child margins → `toolkit:Spacing.Gap` on the panel (and remove the child
  margins: they are overwritten anyway).
- Page padding → `Margin="{DynamicResource {x:Static joufflu:Dimensions.SpacingThickness}}"`.
- Control heights/font sizes/paddings → remove them; use `toolkit:Sizing.Size` on the
  control or its panel for a denser/larger area.
- Headings → `H1`…`H6`, intro `Lead`, captions `Small`, secondary `Muted`; drop literal
  `FontSize`/`FontWeight`.
- Literal `CornerRadius`/`BorderThickness` → `Dimensions.CornerRadius` /
  `Dimensions.BorderThickness` (DynamicResource), or `toolkit:Derive.*` for partial sides.

### Local styles

Keep a local style only if it adds something; base it on the Joufflu one:

```xml
<Style TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}"> … </Style>
<Style TargetType="Button" BasedOn="{StaticResource PrimaryButton}"> … </Style>
```

Delete `ControlTemplate`s that only reimplemented the default look.

### Behaviour swaps (confirm with the user when it changes the view model)

- `MessageBox.Show("Sure?", …, YesNo)` → `await _overlays.Confirm(message, title,
  EnumConfirmationType.Danger) == true` (needs an `IOverlayService` and an
  `OverlayContainer` in the shell — see the `joufflu-new-app` skill).
- Transient success/error messages → `_toasts.Success(...)` / `_toasts.Error(...)`.
- Drag/drop code-behind → `toolkit:DropTarget.Command` with `CanExecute` as the filter,
  `toolkit:DragSource.Data`.
- Hand-made inputs → `inputs:NumericUpDown` (`long?`), `DecimalUpDown` (`decimal?`),
  `TimeSpanPicker`, `Search` (`SearchText` debounced), `ComboBoxSearch`, `ComboBoxTags`,
  `FilePicker`, `ColorPicker`, `Dropdown.Popup` on a `ToggleButton` for popup menus.

## 4. Verify

- `dotnet build` passes (token keys are `x:Static`, so typos fail the build: good).
- Grep the edited files again: no hex colours, no `StaticResource` on Joufflu tokens
  (except `Margin`/`Thickness` pieces documented as `StaticResource`, like
  `Dimensions.TitleBarHeightOffset`, and named styles, which are always `StaticResource`).
- Offer to run the app and switch between Light and Dark to confirm every surface follows.

Summarise what changed per file, and list anything left as-is on purpose (e.g. a brand
logo colour that must stay fixed).
