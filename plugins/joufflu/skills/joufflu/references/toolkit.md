# Toolkit (`Joufflu`, namespace `Joufflu.Toolkit`)

Attached properties that shape controls and layouts. Prefix `toolkit:`
(`clr-namespace:Joufflu.Toolkit;assembly=Joufflu`), tokens with `joufflu:`.

## Sizing

- `Sizing.Size`: `xs` / `sm` / `md` (default) / `lg`. **Inherited**: set on a panel to size
  every child. Scales height (`Dimensions.Height*`), font size and padding. Honoured by
  buttons, inputs, `TreeView`, `FontIcon` (also `xl`), `Badge`, `Spinner`, the Joufflu inputs.
- `Sizing.IsSquare="True"`: width = height, for icon-only buttons.

```xml
<StackPanel toolkit:Sizing.Size="sm" toolkit:Spacing.Gap="8">
    <TextBox /> <ComboBox /> <Button>OK</Button>
</StackPanel>
```

## Spacing

`Spacing.Gap` (a `Thickness`) spaces a panel's children: `StackPanel` along its
orientation, `WrapPanel` and `Grid` on both axes. `"8"` is uniform, `"8,12"` is
`horizontal,vertical`.

```xml
<StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8"> … </StackPanel>
<WrapPanel toolkit:Spacing.Gap="8,12"> … </WrapPanel>
<Grid toolkit:Spacing.Gap="12"> … </Grid>
```

⚠️ Implemented with child margins: a `Margin` set directly on a child is overwritten while
the gap is active. Put extra space on a wrapper instead.

## Tooltip

Themed tooltip shown instantly on hover (native `ToolTip` has a delay).

```xml
<Button Content="Save" toolkit:Tooltip.Content="Save your changes" toolkit:Tooltip.Placement="Bottom" />
<Button Content="Rich">
    <toolkit:Tooltip.Content>
        <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Info}" />
            <TextBlock Text="Any content." />
        </StackPanel>
    </toolkit:Tooltip.Content>
</Button>
```

`Placement`: `Top` (default), `Bottom`, `Left`, `Right`; flips at screen edges. Setting
`Content` to `null` removes it (usable from triggers).

## Drag and drop

### Drop target

`DropTarget.Command` makes any element a drop target (`AllowDrop` and the drag events
are handled). The command receives the data; its `CanExecute` **is the filter**: refused
data can't be dropped and never highlights. `CanExecute` runs on every mouse move, so keep
it cheap (look at paths, not file contents).

```xml
<Border toolkit:DropTarget.Command="{Binding DropFilesCommand}"
        BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}">
    <Border.Style>
        <Style TargetType="Border">
            <!-- Set Background/BorderBrush in the style, not on the element, so the trigger can override -->
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.BorderBrush}}" />
            <Style.Triggers>
                <Trigger Property="toolkit:DropTarget.IsDragOver" Value="True">
                    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.Primary100Brush}}" />
                    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock Text="Drop .pdf files here" />
</Border>
```

```csharp
public IRelayCommand DropFilesCommand { get; } = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);

private static bool CanDropFiles(IDataObject? data)
{
    string[]? files = data?.GetData(DataFormats.FileDrop) as string[];
    return files?.Length > 0 && files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
}
```

- The parameter is actually a `DropData` (which is an `IDataObject`): `drop.Target` (the
  element holding the command) and `drop.Position` (relative to `Target`) tell where it landed.
- `DropTarget.IsDragOver` (inherited) is true only while **accepted** data hovers.
- `DropTarget.Effect`: effect reported to the source (`Copy` default, `Move`, …).

### Drag source

`DragSource.Data="{Binding}"` makes any element draggable; the drag only starts past
the system threshold, so clicks still work. Non-`IDataObject` data is wrapped in a
`DataObject` registered under its type **and every base class/interface**; a string is
text.

```xml
<Border toolkit:DragSource.Data="{Binding}" toolkit:DragSource.AllowedEffects="Move" />
<Border toolkit:DropTarget.Command="{Binding TakeCommand}" toolkit:DropTarget.Effect="Move" />
```

```csharp
BaseNode? node = data?.GetData(typeof(BaseNode)) as BaseNode;
string? text = data?.GetData(DataFormats.UnicodeText) as string;
```

`AllowedEffects` (default `Copy`) and the target's `Effect` must agree for the drop to
happen. `DragSource.IsDragging` (inherited) is true during the drag (e.g. fade the original
to `Opacity 0.4`). The drag is a blocking call.

## Derive (theme-following thicknesses, radii and margins)

A `Thickness`/`CornerRadius` resource built from a scalar is baked at load. `Derive`
builds it on the element from a live `DynamicResource`, with a per-side factor
(`0` drops the side, `1` keeps it, other values scale it).

```xml
<!-- Right border only; order Left,Top,Right,Bottom -->
<Border toolkit:Derive.BorderThickness="{x:Static joufflu:Dimensions.Thickness}"
        toolkit:Derive.BorderThicknessFactor="0,0,1,0" />
<!-- Top corners only; order TopLeft,TopRight,BottomRight,BottomLeft -->
<Border toolkit:Derive.CornerRadius="{x:Static joufflu:Dimensions.Radius}"
        toolkit:Derive.CornerRadiusFactor="1,1,0,0" />
<!-- Margin everywhere but the top -->
<Border toolkit:Derive.Margin="{x:Static joufflu:Dimensions.Spacing}"
        toolkit:Derive.MarginFactor="1,0,1,1" />
```

The value is the resource **key** (`{x:Static …}`), not a `DynamicResource`. It is written
with `SetCurrentValue`, which beats style setters and local values: don't also set the fed
property in the same style, move triggers onto the `Derive` property, and don't use it for
properties consumers must override. `BorderThickness` works on `Border` and any `Control`,
`CornerRadius` on `Border`, `Margin` on any `FrameworkElement`.

## Animate

```xml
<Button toolkit:Animate.Bounce="True">Look at me</Button>
<feedback:Badge toolkit:Animate.Bounce="{Binding HasUnread}">3</feedback:Badge>
<fonts:FontIcon toolkit:Animate.Bounce="True" toolkit:Animate.BounceHeight="10"
                toolkit:Animate.BounceDuration="0:0:0.6" Text="{x:Static fonts:LucideFontIcons.Bell}" />
```

Looping hop through a `RenderTransform` (no layout impact). `BounceHeight` in DIPs
(default 6), `BounceDuration` = full cycle incl. pause (default 1 s). Paused while hidden.

## Converters (`Joufflu.Converters`, prefix `conv:`)

Declare them in resources yourself; none are registered under a key by the library.

| Converter | Behaviour |
|---|---|
| `BooleanConverter` (`BooleanConverter.Default`) | Any value → bool: bool as-is, non-empty string, int > 0, non-empty collection, non-null. `ConverterParameter=False` inverts. |
| `BooleansConverter` (multi) | Combines several values with `&&` (default) or `\|\|` given as parameter |
| `BooleanFlipConverter` | Inverts a bool |
| `VisibilityConverter` (`VisibilityConverter.Default`) | Same truthiness as `BooleanConverter` → `Visible`/`Collapsed`; `ConverterParameter=False` inverts |
| `EnumMatchToBooleanConverter` | `value.Equals(parameter)`; two-way for `RadioButton` groups bound to one enum/string |
| `TypeConverter` | Value → its `Type` (for `DataTrigger Value="{x:Type …}"`) |

```xml
<conv:EnumMatchToBooleanConverter x:Key="EnumMatch" />
<TextBlock Visibility="{Binding Items, Converter={x:Static conv:VisibilityConverter.Default}}" />
```

`Joufflu.Extensions.EnumValuesExtension` lists an enum's values:
`ItemsSource="{ext:EnumValues {x:Type local:MyEnum}}"` (`IgnoreElements` skips the first N).

## Helpers (`Joufflu.Helpers`)

- `MoreVisualTreeHelper.FindParent<T>(child)`, `GetChild<T>(element, recursive)`,
  `GetChildren<T>(element, recursive)`, `FindSelfOrParent(origin, type)`, `GetParent(element)`.
- `ClipboardManager(window)` with a `ClipboardChanged` event.
