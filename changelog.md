# Joufflu.Inputs (unreleased)

- Build the groups of a `FormatTextBox` from options parsed up front rather than read by a virtual call from the group's constructor, and make `numeric` and `decimal` one generic number group, as their code was identical but for the step and the type they count in. The format strings are unchanged. **Breaking** : `NumericGroup`, `DecimalGroup`, `BaseNumericGroup<T>` and `GroupsFactory` are no longer public, and `StringFormat`, `IsNullable` and `NullableChar` leave `BaseGroup`, whose `Length` can no longer be set from outside
- Give a number group one way to be written by the user, clamped between its bounds, and one to be loaded from outside, taken as it stands, rather than a `new` typed `Value` hiding the base one, which clamped or not depending on the type the group was seen as. **Breaking** : `BaseGroup.Value` is read only, and `BaseGroup.SetValueFrom` becomes `Load`
- Let a `FormatTextBox` group edit its own text only, handed the caret and the selection within it, while the box alone moves the caret and the selection around : the groups no longer hold their box. This fixes a group with `length` and `noGlobalSelection` next to literal text, which cut its slice out of the box text by its max length and so typed over the literal (typing `2` after `x1` in `x{numeric|length:2|noGlobalSelection}` gave `x2`). Spinning a group keeping its own caret now puts the caret at the end of the number, where it went back to wherever the last typing had left it. **Breaking** : `BaseGroup` exposes `Input`, `DeleteCharacter`, `Clear`, `Increment`, `Decrement`, `Render`, `SelectsWhole`, `IsEmpty` and `IsFull` in place of `OnInput`, `OnAfterInput`, `OnSelection`, `OnDelete`, `OnDeleteCharacter` and `ToString`, and `IBaseNumericGroup` is gone
- Move what a `FormatTextBox` shows and how it answers the keyboard into a `FormatEditor` of its own, free of WPF : the box forwards its input there and writes the editor's text and selection back to itself in one place, so the logic is tested without a window. The editor is created once the control is initialized and again whenever `Format` or `GlobalFormat` changes, rather than on `Loaded`. This fixes two groups side by side (`{max:9}{max:9}`), where filling the first one selected it again instead of the second, typing `12` giving `20`. `Values` is only handed out when it changes, after the text shows the edit, and the increment buttons give the box the focus. **Breaking** : `SelectedGroup`, `SelectedGroupIndex`, `Groups`, `ChangeSelectedGroup`, `ParseGroups` and `OnLoaded` leave `FormatTextBox`, and `BaseGroup` is no longer public

# Joufflu.FileExplorer (unreleased)

- Group the code by feature rather than by kind : `Nodes` holds what every source and control shares (`IExplorerNode`, `IExplorerDirectory`, `IExplorerFile`, `IExplorerSource`, `ExplorerNodeRename`, `ExplorerNodeComparer`), `FileSystem` the disk implementation (`FileSystemSource`, `FileSystemWatcherSource`, `FileSystemFile`, `FileSystemDirectory` and the shell and clipboard plumbing only they use), `Icons` the system icons with `ExplorerIconConverter`, and `Controls/Base/Rename` the editable name of a node. `FileSizeConverter` sits next to `ExplorerList`, its only user. **Breaking** : `Joufflu.FileExplorer.Data` and `Joufflu.FileExplorer.Sources` become `Joufflu.FileExplorer.Nodes` and `Joufflu.FileExplorer.FileSystem`, and `Joufflu.FileExplorer.Converters` becomes `Joufflu.FileExplorer.Icons` for `ExplorerIconConverter`
- Name the two steps of a rename in the controls displaying nodes : `BeginRenameCommand` opens the editable name and `EndRenameCommand` closes it, handing a validated name over to the source's `RenameCommand`. They were `RenamingCommand` and `RenameCommand`, the latter easily mistaken for the source's. `IExplorerUi`, implemented by those controls alone, is gone : `ExplorerMenuContext` exposes `BeginRenameCommand` in place of `Ui`, so a context menu binds `{Binding BeginRenameCommand}`. **Breaking** for a context menu template binding `Ui.RenamingCommand`

# Joufflu (unreleased)

- Define the `Brushes` once, in `Styles/Brushes.xaml`, rather than in every theme : a theme is now its colours alone, and the theme customizer generates colours only. `ThemeManager` pairs a fresh copy of the brushes with every theme it applies, since a brush resolves its `DynamicResource` colour once and never again. This also fixes the `XSoftBrush` / `XSoft100Brush` tints, which were shared across themes and kept the colours of the first one shown. A custom theme still defining its own brushes keeps working, the paired ones taking precedence
- Rename the `XSoftStrongBrush` keys `XSoft100Brush` (`PrimarySoft100Brush`, `DangerSoft100Brush`, …), the hover of a soft tint following the same convention as the hover of a fill, `X100`. The *Design tokens* page shows each accent's soft tint and its hover next to its other keys. **Breaking** : a reference to a `XSoftStrongBrush` key needs renaming

# Joufflu.Data 0.8.2

- Add `DataDisplay`, showing a node read only : the key, the type and the value of each node, a value read the way its editor shows it (the name of a `Choice` option, a date without its time when it has none), null and undefined greyed apart from a text, and a forced value flagged with a feather
- Let objects and arrays be forced to a manual value in `DataFill` and `DataEdit` too, and shown forced in `DataDisplay`, their properties or items hidden while forced and written as the entry instead. `IsManual` and `ManualEntry` move from `DataValue` up to `DataNode`, and `Load()` forces an object or an array left out, matching a manual value or given a JSON of another shape, rather than filling its children

# Joufflu.Data 0.8.1

- Add `DataNode.Load(JToken, manualValues)`, the reverse of `ToToken()` : fills a tree with a JSON value, forcing what matches a manual value or what the editor can't hold rather than losing it, and a property left out to undefined when not required
- Add `JToken.ToDataNode()`, inferring a tree from a JSON value for `DataEdit`, and `DataArray.Add(DataNode)`
- Add `DataNode.Changed`, raised when a node or anything under it changes
- Fall back to the default value when leaving manual mode on a value the editor can't hold, a string in a number field for example, instead of failing on `ToToken()`

# Version 0.7.0

- Add a *Design tokens* page, to the sample gallery and to the documentation, listing every `Colors`, `Brushes` and `Dimensions` key with the role it actually plays in the control styles, grouped by what it is for — the sample one paints each colour as a swatch live against the selected theme. The naming conventions the palette rests on are spelled out rather than left to be inferred : `X100` is the hover and pressed variant of an accent, `XContent` what is drawn on top of it, `Background100` an elevated surface and `Background200` a transient one
- Give `ComboBoxSearch` a chevron of its own to open its choices with, on the right of the clear button and following the input padding of every `Sizing.Size` : its editable text box takes the whole control surface, so the drop down had no handle left and could only be opened by typing or with the arrow keys. Opening it shows the list whole even when an item is already selected — the text a selection leaves behind is not something the user searched for, so it no longer filters the choices down to the one already picked
- Give `ComboBoxTags` the same chevron, in a column of its own on the right of the tags : its drop down toggle covered the whole control but was aligned to the right as soon as the control was editable, which it always is, leaving it zero wide and the list reachable by typing only
- Keep the clear button of `Search`, `ComboBoxSearch` and `FilePicker` flush with the text at every `Sizing.Size`, its margin following the input padding of the size instead of staying on the `md` one
- Add `Animate.Bounce`, a looping vertical hop on any element, landing and rebounding twice before it rests so it settles rather than pulses : bindable, so the loop starts and stops with a view model flag, with `Animate.BounceHeight` (DIPs) and `Animate.BounceDuration` (a full cycle, hop plus the pause before the next one) shaping it. It animates a `TranslateTransform` of its own added to the element's `RenderTransform`, so it affects no layout and moves no neighbour, and a transform already there is composed with rather than replaced, then restored when the bounce stops. The hop pauses itself while the element is not visible — collapsed, hidden, under a collapsed ancestor or out of the visual tree — and resumes when it comes back, and the element stays collectable whether the bounce was stopped first or not
- Move the toolkit helpers out of the root `Joufflu` namespace into a `Joufflu.Toolkit` one : `Sizing`, `Spacing`, `Tooltip`, `DropTarget`, `DropData` and `DragSource`, joined by `Derive` which leaves `Joufflu.Extensions`. The design system keys — `Colors`, `Brushes`, `Dimensions` — stay in `Joufflu`, `ThemeManager` in `Joufflu.Themes` and `ThemedWindow` in `Joufflu.Controls`, so the root namespace is the design system and the toolkit is what shapes controls with it. **Breaking** : XAML declaring `xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"` (or `xmlns:extensions="clr-namespace:Joufflu.Extensions;assembly=Joufflu"` for `Derive`) needs a `xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"` and the matching prefix on those attached properties, and C# touching them needs a `using Joufflu.Toolkit;`
- Add the `Joufflu.Data` package : JSON Schema as two trees rather than a raw text box. `DataEditor` fills a value in against a schema, one row per value edited by the widget its kind calls for, with a property the schema does not require left out until it is ticked — to a schema an absent property and a property holding "" are not the same thing. `SchemaEditor` writes the schema itself, and `SchemaView` shows one read only. Any field can hold a reference to be resolved later instead of a literal, whatever its declared type, picked from what the caller offers. Schemas are NJsonSchema `JsonSchema` objects, so one derived from a .NET type round-trips without a conversion layer, and a shape the editors cannot model — a schema saying nothing of a type, one referencing itself — falls back on the JSON it is rather than being lost. Replaces the never released `Joufflu.Data.Shared` generic element model, which conflated the shape with the values and no longer built
- Add `OverlayViewModel`, a base for the content of a modal overlay : it holds the `OverlayOptions` the overlay is shown with, so `IOverlayService.Show` takes nothing but the content, it comes with a `CancelCommand`, and it closes itself rather than whatever sits on top of the stack. `OverlayViewModel<TResult>` adds what the overlay is awaited for — a `Result` set by validating it, and a static `ShowAsync` handing that result back, the default of the type when the overlay was dismissed. Replaces the `CloseTop(true)` / `CloseTop(false)` pair every awaited overlay was writing by hand
- Add `IOverlayService.Close(object content, bool? result)`, closing the overlay showing a given content rather than the top one : an overlay opening another one of its own kind can now close itself whichever is displayed. **Breaking** for anything implementing `IOverlayService` outside the library, `OverlayService` implementing it already

# Version 0.6.5

- Give the text inputs — `TextBox`, `PasswordBox` and `FormatTextBox` — their own `Dimensions.InputPadding` keys (`Xs`/`Sm`/`Md`/`Lg`), half the control padding's horizontal so they read tighter than a button, covering every size variant where the base text input padding used to be fixed whatever the size. Set on a plain `Padding` setter, so a consumer local value or style overrides it; the theme customizer keeps them in lockstep with the control padding rather than editing them on their own
- Replace `Derive.BorderSides`, `Derive.MarginSides` and `Derive.Corners` by `Derive.BorderThicknessFactor`, `Derive.MarginFactor` and `Derive.CornerRadiusFactor` : a `Thickness` or a `CornerRadius` multiplying the derived value side by side, so `0` still drops a side and `1` keeps it, while any other value scales it — a doubled margin or a halved radius no longer needs its own dimension. Drops the `ThicknessSides` and `Corners` flags enums
- Let the header of a `TreeViewItem` fill the width of the row it is highlighted on, so what is seen as the item is what answers to the mouse — a drag from a `DragSource` in the item template included. Its height follows `VerticalContentAlignment`, still centered by default : a header covering the row entirely sets it to `Stretch` and centers its own content

# Version 0.6.3

- Pass a `DropData` to `DropTarget.Command` rather than the bare `IDataObject` : it is an `IDataObject` of the dragged data, so the commands taking one keep working, and it adds where the pointer is (`Position`) on the element the drop landed on (`Target`), for the targets placing what they receive
- Offer the data wrapped by `DragSource.Data` under the base classes and the interfaces of its type too, so a target can ask for what the data is instead of having to know the exact kind it is given

# Version 0.5.1

- Add a standard confirm overlay, `IOverlayService.Confirm`, with an `EnumConfirmationType` colouring its confirm button
- Add `DropTarget.Command`, turning any element into a drop target
- Add `DragSource.Data`, turning any element into a drag source, with `DragSource.AllowedEffects` and `DragSource.IsDragging`
- Add a `FullContainer` to simplify content placement when using `AllowContentOverTitleBar`
- Support `Paging` without a known total
- Move `Dropdown` from `Joufflu.Navigation` to `Joufflu.Inputs` (namespace `Joufflu.Inputs.Controls`)
- Turn `Dropdown` from a wrapper control into attached properties (`Dropdown.Popup`, `Dropdown.Placement`, `Dropdown.PopupStyle`, offsets) set on a `ToggleButton` you own, so the button accepts any `ToggleButton` style and attached property — `Sizing.IsSquare` included — with nothing to forward. Replaces `Header`, `ButtonStyle` and `PopupPlacement`. Adds `Dropdown.CloseOnClick`, dismissing the popup when a button inside it is clicked. `Dropdown.PopupStyle` now styles the `DropdownPopupHost` drawing the popup chrome, so its padding, background, border and radius are reachable — a `Popup` itself has none of those
- Remove the `Dimensions.BorderThicknessRight` key, now owned by `NavigationMenu` which was its only user
- Add the `Derive.BorderThickness` and `Derive.CornerRadius` attached properties, deriving per-side thicknesses and per-corner radii from a scalar dimension so they follow a runtime theme change
- Derive the partial borders and radii of `NavigationMenu`, `TabControl`, `GroupBox`, `DataGrid` and `ListView` through `Derive`, dropping the `NavigationMenuBorderThickness` key

# Version 0.4.0

- Change the navigation to use types instead of string keys
- Split `OverlayContainer` into separate overlay and `ToastContainer` containers
- Move the tooltip into the `Joufflu` core package

# Version 0.2.0

- Add the `Joufflu.FileExplorer` package : `Explorer`, `ExplorerList`, `ExplorerTree` and `ExplorerControlBar` sharing an `IExplorerSource`, with node visuals and context menus keyed on the node type, drag and drop, keyboard shortcuts, and file operations handed over to the Windows shell
- Add the `xl` control size and its `FontSizeXl` dimension
- Size `FontIcon` from the design system instead of a fixed value
- Improve the toasts look, with a progress bar of their remaining duration
- Restyle the native `ListView` and `TreeView` (rounded border, centered cell content)
- Add `MoreVisualTreeHelper.FindSelfOrParent`, and a logical tree fallback to its parent lookup

# Version 0.1.2

- Move `Badge`, `Spinner`, toasts and the tooltip attached properties out of the core `Joufflu` package into a new `Joufflu.Feedback` package (namespace `Joufflu.Feedback.Controls`)

# Version 0.1.1

- Add tooltip
- Add soft and outline button styles
- Improve theme manager custom themes handling
- Improve `ThemedWindow` handling of `AllowContentOverTitleBar`

# Version 0.1.0

- First version