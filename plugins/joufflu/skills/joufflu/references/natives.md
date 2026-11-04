# Native controls, named styles and icons (`Joufflu`)

Merging the core `Resources.xaml` styles every built-in WPF control implicitly:
`Button`, `ToggleButton`, `CheckBox`, `RadioButton`, `Slider`, `TextBox`, `ListBox`,
`Calendar`, `ComboBox`, `DatePicker`, `PasswordBox`, `Separator`, `Label`, `TreeView`, `ListView`,
`DataGrid`, `Hyperlink`, `Menu`, `MenuItem`, `ContextMenu`, `TabControl`, `ToolBar`, `ProgressBar`,
`StatusBar`, `ToolTip`, `ScrollBar`, `ScrollViewer`, `Window`, `GridSplitter`, `GroupBox`,
`Expander`. Use them as-is. A local style must be based on the default one:

```xml
<Style TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}"> … </Style>
```

Named styles are applied with `Style="{StaticResource Key}"`.

## Buttons

| Emphasis | Keys |
|---|---|
| Solid | `PrimaryButton`, `SecondaryButton`, `GhostButton`, `SuccessButton`, `InfoButton`, `WarningButton`, `DangerButton` |
| Soft (tinted background, semantic text) | `SoftPrimaryButton`, `SoftSecondaryButton`, `SoftSuccessButton`, `SoftInfoButton`, `SoftWarningButton`, `SoftDangerButton` |
| Outline (border + text, soft fill on hover) | `OutlinePrimaryButton`, `OutlineSecondaryButton`, `OutlineSuccessButton`, `OutlineInfoButton`, `OutlineWarningButton`, `OutlineDangerButton` |
| Other | `EmbeddedButton` (small button inside an input) |

Every Solid/Soft/Outline key has a `ToggleButton` twin with a `ToggleButton` suffix instead of `Button`
(`PrimaryToggleButton`, `SoftDangerToggleButton`, `OutlineInfoToggleButton`, `GhostToggleButton`…),
plus `IconToggleButton` (transparent icon toggle, square at every size) and `ChevronToggleButton`
(see Icons below). A plain, ghost or icon toggle turns `Primary` when checked; solid variants take
their hover fill (`XInteractionBrush`); soft and outline variants turn solid.

A plain `<Button>` has the default look. Soft Success/Warning have low contrast in Light;
keep them for short labels.

```xml
<Button Style="{StaticResource PrimaryButton}" Command="{Binding SaveCommand}">Save</Button>

<!-- Icon-only button: IsSquare makes width = height; composes with any variant -->
<Button toolkit:Sizing.IsSquare="True" Style="{StaticResource GhostButton}"
        ToolTip="Add">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Plus}" />
</Button>

<!-- Sizes: xs / sm / md (default) / lg -->
<Button toolkit:Sizing.Size="sm">Small</Button>

<!-- Icon + label -->
<Button Style="{StaticResource PrimaryButton}">
    <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="6">
        <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Save}" />
        <TextBlock Text="Save" />
    </StackPanel>
</Button>
```

## Menu items

`DangerMenuItem` colours a destructive `MenuItem` (e.g. "Delete" in a `ContextMenu`).

## Text boxes and lists

`MultilineTextBox` makes a `TextBox` multi-line: it accepts Return, stretches to the height it is
given (no fixed control height), aligns its text to the top and scrolls horizontally instead of wrapping.

```xml
<TextBox Height="120" Style="{StaticResource MultilineTextBox}" Text="{Binding Notes}" />
```

A `ListBox` scrolls by pixel and builds every item. For a long list, set
`ScrollViewer.CanContentScroll="True"` on it to scroll by item and only build the items in view.

## Validation errors

Binding errors (`INotifyDataErrorInfo`, `IDataErrorInfo`, `ValidationRule`) show in two layers:
the input's own template turns its border red on `Validation.HasError`, and `ValidationErrorTemplate`
adds a badge on the top-right corner whose tooltip lists the errors. The template draws no border,
which would double the input's. Every input is wired already, don't hand-roll error tooltips:
`TextBox`, `PasswordBox`, `ComboBox`, `DatePicker`, and in `Joufflu.Inputs` `NumericUpDown`,
`DecimalUpDown`, `TimeSpanPicker`, `FormatTextBox`, `ComboBoxSearch`, `ComboBoxTags`, `FilePicker`,
`TextEditable`, `ColorPicker` (and `Search`). A custom input needs both: a `Validation.HasError`
trigger colouring its border with `DangerBrush`, and
`Validation.ErrorTemplate="{DynamicResource ValidationErrorTemplate}"`. Redefine the key to restyle
every input; `Validation.ErrorTemplate="{x:Null}"` on an input drops the badge and keeps only its
red border.

A host showing a view model that validates itself (`ContentControl Content="{Binding CurrentPage}"`)
needs `ValidatesOnNotifyDataErrors=False` on that binding, or the whole page is outlined in red.

## Card

Named `Border` styles for padded, rounded surfaces:

- `Card`: a display, `DisplayBase0` fill, `DisplayBorder0`, `DisplayRadius`, `DisplayPadding`.
- `CardSecondary`: `DisplayBorder0` panel with a `DisplayBorder1` border, meant to sit **inside** a `Card`.

```xml
<Border Style="{StaticResource Card}">
    <StackPanel toolkit:Spacing.Gap="8">
        <TextBlock toolkit:Sizing.Size="lg" FontWeight="SemiBold" Text="Title" />
        <Border Style="{StaticResource CardSecondary}">
            <TextBlock Text="Nested" TextWrapping="Wrap" />
        </Border>
    </StackPanel>
</Border>
```

## Typography

A `TextBlock` takes its font size from `toolkit:Sizing.Size`
(`xs`…`xl` → `Dimensions.FontSizeXs`…`FontSizeXl`; `md` keeps the inherited body size). The
size is inherited, so a panel set to `sm`/`lg` sizes its text too. A heading is a size plus a
`FontWeight`; muted text is dimmed with `Colors.MutedOpacity`, not given another colour.
Two named styles exist: `TextMuted` (muted opacity, wrapping) and `Title` (semi-bold,
`FontSizeLg`), e.g. `<TextBlock Style="{StaticResource Title}" />`.

| Role | Markup |
|---|---|
| Page title | `toolkit:Sizing.Size="xl" FontWeight="Bold"` |
| Section heading | `toolkit:Sizing.Size="lg" FontWeight="SemiBold"` |
| Minor heading | `FontWeight="SemiBold"` |
| Caption, fine print | `toolkit:Sizing.Size="xs"` |
| Muted text | `Opacity="{DynamicResource {x:Static joufflu:Colors.MutedOpacity}}"` |

```xml
<TextBlock toolkit:Sizing.Size="xl" FontWeight="Bold" Text="Settings" />
<TextBlock Opacity="{DynamicResource {x:Static joufflu:Colors.MutedOpacity}}"
           Text="Changes apply immediately." />
```

Inside a control template of your own, set `FontSize` from the token directly
(`{DynamicResource {x:Static joufflu:Dimensions.FontSizeLg}}`) rather than relying on the
implicit `TextBlock` style.

## Icons — `FontIcon`

`FontIcon` (in `Joufflu.Assets.Fonts`) derives from `TextBlock` and renders a glyph
of the embedded [Lucide](https://lucide.dev) font. Glyphs are constants on
`LucideFontIcons` named, mostly in PascalCase, after the Lucide icon name (the close
glyph is the lowercase `x`) (`Home`, `Plus`,
`Trash`, `Save`, `Search`, `Settings`, `Bell`, `Info`, `Leaf`, `EllipsisVertical`,
`StickyNote`, …). If unsure a glyph exists, grep `LucideFontIcons.cs` in the package
source or check IntelliSense; don't guess.

```xml
<fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon toolkit:Sizing.Size="lg" Text="{x:Static fonts:LucideFontIcons.Leaf}" />   <!-- xs/sm/md/lg/xl -->
<fonts:FontIcon Foreground="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}"
                Text="{x:Static fonts:LucideFontIcons.Trash}" />
```

`Chevron` (a `FontIcon`) is the chevron of a header that opens content: `IsExpanded` picks the
glyph, no animation. `Placement="Leading"` (default, tree) right → down, `"Trailing"` (expander,
menu group) down → up. `ChevronToggleButton` is the square, chrome-less tree-row expander built on it.

```xml
<ToggleButton IsChecked="{Binding IsExpanded}" Style="{StaticResource ChevronToggleButton}" />
<fonts:Chevron IsExpanded="{Binding IsExpanded}" Placement="Trailing" />
```

Its size follows the inherited `Sizing.Size`, so an icon inside a `lg` button is `lg`.
