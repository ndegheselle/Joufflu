# Inputs (`Joufflu.Inputs`)

```xml
xmlns:inputs="clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs"
xmlns:format="clr-namespace:Joufflu.Inputs.Controls.Format;assembly=Joufflu.Inputs"
```

All inputs are two-way bindable like WPF's own and honour `toolkit:Sizing.Size`.

| Need | Control | Main property |
|---|---|---|
| Whole number | `NumericUpDown` | `Value` (`long?`) |
| Decimal number | `DecimalUpDown` | `Value` (`decimal?`) |
| Duration | `TimeSpanPicker` | `Value` (`TimeSpan?`) |
| Custom segmented format (time, codes…) | `format:FormatTextBox` | `Format`, `GlobalFormat`, `Values` |
| Debounced search box | `Search` | `SearchText`, `SearchCommand`, `SearchChanged` |
| Combo filtering as you type | `ComboBoxSearch` | `ItemsSource`, `SelectedItem`, `DisplayMemberPath` |
| Multi-selection as tags | `ComboBoxTags` | `SelectedItems` (`IList`), `AllowAdd` |
| Label that becomes editable | `TextEditable` | `Text`, `TextChanged` |
| File path via system dialog | `FilePicker` | `FilePath`, `Options` |
| Colour | `ColorPicker` | `Color` (`System.Windows.Media.Color`) |
| Popup menu/panel off a button | `Dropdown.*` attached properties on a `ToggleButton` | `Dropdown.Popup` |

For a plain date, text or bool, use the natively styled `DatePicker`, `TextBox`, `CheckBox`.

## NumericUpDown / DecimalUpDown / TimeSpanPicker

Built on `FormatTextBox`, with clear and increment/decrement buttons.

```xml
<inputs:NumericUpDown Value="{Binding Quantity, Mode=TwoWay}" />
<inputs:DecimalUpDown Value="{Binding Price, Mode=TwoWay}" />
<inputs:TimeSpanPicker Value="{Binding Duration, Mode=TwoWay}" />   <!-- days/hours/minutes/seconds -->
```

View-model properties should be the nullable types above (`long?`, `decimal?`, `TimeSpan?`)
or convertible to them. A `ValueChanged` event is also raised.

## FormatTextBox

A text box split into groups described by a format string; <kbd>Tab</kbd> / arrows move
between groups, arrows up/down increment.

```xml
<format:FormatTextBox Format="{}{max:23}h {max:59}m {max:59}s" GlobalFormat="numeric" />
```

- Each `{…}` is a group; options are `|`-separated `key` or `key:value`.
- `GlobalFormat` options apply to every group (prefixed to each).
- A group needs a type: `numeric` or `decimal`.
- Options: `min:`, `max:`, `length:`, `padded`, `nullable`, `nullableChar:`, `format:`
  (string format), `incrementDelta:`, `noGlobalSelection`. An unknown option throws.
- The `{}` prefix escapes the braces in XAML.
- Values: `Values` (list, one per group), `ValuesChanged` event.

## Search

Debounced text box; <kbd>Escape</kbd> clears it.

```xml
<!-- MVVM: SearchText is two-way by default and only updates once typing settles -->
<inputs:Search SearchText="{Binding Query}" />
<inputs:Search SearchCommand="{Binding SearchCommand}" />   <!-- executed with the text -->
```

Code-behind: `search.SearchChanged += text => Filter(text);`, `search.ClearSearch()`.

## ComboBoxSearch

Editable `ComboBox` filtering its items as you type, on `DisplayMemberPath` (or
`ToString()`). The chevron or <kbd>Up</kbd>/<kbd>Down</kbd> open the full list.

```xml
<inputs:ComboBoxSearch ItemsSource="{Binding Countries}"
                       DisplayMemberPath="Name"
                       SelectedItem="{Binding SelectedCountry}" />
```

## ComboBoxTags

Builds on `ComboBoxSearch`: selected items are shown as removable tags. `AllowAdd="True"`
lets users add values not in the list.

```xml
<inputs:ComboBoxTags AllowAdd="True"
                     ItemsSource="{Binding Countries}"
                     SelectedItems="{Binding SelectedCountries}" />
```

Bind `SelectedItems` to a mutable collection (`ObservableCollection<T>`).

## TextEditable

Shows a value that turns into a text box on demand (inline rename, outside a form).

```xml
<inputs:TextEditable Text="{Binding Name, Mode=TwoWay}" />
```

`TextChanged` event carries `Text` and `OldText`.

## FilePicker

```xml
<inputs:FilePicker FilePath="{Binding FilePath, Mode=TwoWay}" />
```

`Options` takes a `FilePickerOptions` (`Filter`, e.g. `"PDF|*.pdf"`, and
`DefaultExtension`), set from code or a resource.

## ColorPicker

Hex field plus a popup (saturation/brightness square, hue slider, alpha slider, opacity %).

```xml
<inputs:ColorPicker Color="{Binding Color, Mode=TwoWay}" />
```

## Dropdown (attached properties on a `ToggleButton`)

The popup is open while the button is checked and closes on click outside. The button
stays a plain `ToggleButton` you style freely.

```xml
<ToggleButton Content="Actions" inputs:Dropdown.CloseOnClick="True" inputs:Dropdown.Placement="BottomRight">
    <inputs:Dropdown.Popup>
        <StackPanel toolkit:Spacing.Gap="4">
            <Button Content="Rename" Command="{Binding RenameCommand}" Style="{StaticResource GhostButton}" />
            <Button Content="Delete" Command="{Binding DeleteCommand}" Style="{StaticResource GhostButton}" />
        </StackPanel>
    </inputs:Dropdown.Popup>
</ToggleButton>

<!-- Icon-only "more" menu -->
<ToggleButton toolkit:Sizing.IsSquare="True" inputs:Dropdown.Placement="BottomRight">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.EllipsisVertical}" />
    <inputs:Dropdown.Popup> … </inputs:Dropdown.Popup>
</ToggleButton>
```

| Attached property | Default | Purpose |
|---|---|---|
| `Dropdown.Popup` | null | Popup content; setting it makes the button a dropdown |
| `Dropdown.Placement` | `BottomLeft` | `BottomLeft`, `BottomRight`, `TopLeft`, `TopRight` |
| `Dropdown.HorizontalOffset` / `VerticalOffset` | 0 | Extra offset |
| `Dropdown.CloseOnClick` | false | Close when a `ButtonBase` inside is clicked (also CheckBox/RadioButton!) |
| `Dropdown.PopupStyle` | null | Style for `inputs:DropdownPopupHost` (padding defaults to 0) |

```xml
<inputs:Dropdown.PopupStyle>
    <Style TargetType="{x:Type inputs:DropdownPopupHost}" BasedOn="{StaticResource {x:Type inputs:DropdownPopupHost}}">
        <Setter Property="Padding" Value="12,8" />
    </Style>
</inputs:Dropdown.PopupStyle>
```

Bindings in the popup: `DataContext`, `Foreground` and `Sizing.Size` flow in, so plain
bindings work. `ElementName` and `RelativeSource AncestorType` lookups outside the popup
do **not**; go through the host instead:

```xml
<Button Command="{Binding PlacementTarget.DataContext.SaveCommand,
    RelativeSource={RelativeSource AncestorType={x:Type inputs:DropdownPopupHost}}}" />
```
