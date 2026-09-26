# Native controls, named styles and icons (`Joufflu`)

Merging the core `Resources.xaml` styles every built-in WPF control implicitly:
`Button`, `ToggleButton`, `CheckBox`, `RadioButton`, `Slider`, `TextBox`, `ListBox`,
`Calendar`, `ComboBox`, `DatePicker`, `TextBlock`, `Label`, `TreeView`, `ListView`,
`DataGrid`, `Hyperlink`, `Menu`, `MenuItem`, `TabControl`, `ToolBar`, `ProgressBar`,
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
plus `IconToggleButton` (24×24 transparent icon toggle). Solid variants darken when checked;
soft and outline variants turn solid.

A plain `<Button>` has the default look. Soft Success/Warning have low contrast in Light;
keep them for short labels.

```xml
<Button Style="{StaticResource PrimaryButton}" Command="{Binding SaveCommand}">Save</Button>

<!-- Icon-only button: IsSquare makes width = height; composes with any variant -->
<Button toolkit:Sizing.IsSquare="True" Style="{StaticResource GhostButton}"
        toolkit:Tooltip.Content="Add">
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

## Validation errors

`ValidationErrorTemplate` shows binding errors (`INotifyDataErrorInfo`, `IDataErrorInfo`,
`ValidationRule`): a danger border plus a badge on the top-right corner whose tooltip lists the
errors. `TextBox`, `PasswordBox`, `ComboBox`, `DatePicker` and `FilePicker` use it already; don't
hand-roll error tooltips. Apply it to any other control with
`Validation.ErrorTemplate="{DynamicResource ValidationErrorTemplate}"`, or redefine the key to
restyle every input.

## Card

Named `Border` styles for padded, rounded surfaces:

- `Card`: `Background100` fill, themed border, corner radius, standard padding.
- `CardSecondary`: recessed panel (deeper `Background`) meant to sit **inside** a `Card`.

```xml
<Border Style="{StaticResource Card}">
    <StackPanel toolkit:Spacing.Gap="8">
        <TextBlock Style="{StaticResource H4}" Text="Title" />
        <Border Style="{StaticResource CardSecondary}">
            <TextBlock Text="Nested" TextWrapping="Wrap" />
        </Border>
    </StackPanel>
</Border>
```

## Typography

Named `TextBlock` styles; an unstyled `TextBlock` is body text (`FontSizeMd`).

| Key | Role |
|---|---|
| `H1` … `H6` | Page title down to minor subheading |
| `Lead` | Larger, lighter intro paragraph |
| `Muted` | Secondary text (`Foreground100`) |
| `Small` | Fine print, captions |

```xml
<TextBlock Style="{StaticResource H1}" Text="Settings" />
<TextBlock Style="{StaticResource Muted}" Text="Changes apply immediately." />
```

## Icons — `FontIcon`

`FontIcon` (in `Joufflu.Assets.Fonts`) derives from `TextBlock` and renders a glyph
of the embedded [Lucide](https://lucide.dev) font. Glyphs are constants on
`LucideFontIcons` named in PascalCase after the Lucide icon name (`Home`, `Plus`,
`Trash`, `Save`, `Search`, `Settings`, `Bell`, `Info`, `Leaf`, `EllipsisVertical`,
`StickyNote`, …). If unsure a glyph exists, grep `LucideFontIcons.cs` in the package
source or check IntelliSense; don't guess.

```xml
<fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon toolkit:Sizing.Size="lg" Text="{x:Static fonts:LucideFontIcons.Leaf}" />   <!-- xs/sm/md/lg/xl -->
<fonts:FontIcon Foreground="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}"
                Text="{x:Static fonts:LucideFontIcons.Trash}" />
```

Its size follows the inherited `Sizing.Size`, so an icon inside a `lg` button is `lg`.
