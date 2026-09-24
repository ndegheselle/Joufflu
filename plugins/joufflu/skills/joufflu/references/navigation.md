# App shell and navigation (`Joufflu` + `Joufflu.Navigation`)

```xml
xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
```

```csharp
using Joufflu.Navigation;              // Navigator, IOverlayService, OverlayOptions, OverlayViewModel, EnumConfirmationType, IPage
using Joufflu.Navigation.Controls;     // OverlayService
using Joufflu.Feedback;                // ToastService, IToastService
```

## Architecture

Three services, owned by a shell view model and shared with every page:

| Service | Role | Host control |
|---|---|---|
| `Navigator` | Holds `CurrentPage` (a view model) | `ContentControl Content="{Binding Navigator.CurrentPage}"` + `nav:NavigationMenu` |
| `OverlayService` (`IOverlayService`) | Stack of modal overlays | `nav:OverlayContainer Overlays="…"` |
| `ToastService` (`IToastService`) | Notifications | `feedback:ToastContainer Toasts="…"` |

Navigation is **view-model-first**: navigate to a view model, and an implicit
`DataTemplate` (DataType, no `x:Key`, typically in `App.xaml`) renders its view. Overlay
content is resolved the same way.

Nesting order (outer → inner): `ThemedWindow` → `ToastContainer` → `OverlayContainer` →
app layout. Wrapping the whole window lets a full-screen overlay cover the menu, and keeps
toasts above overlays.

## ThemedWindow (`Joufflu.Controls`)

Custom window with a styled title bar and caption buttons. A plain `Window` is also
themed implicitly.

```xml
<controls:ThemedWindow x:Class="MyApp.ShellWindow" … Title="My App"
                       AllowContentOverTitleBar="True"
                       IconVisibility="Collapsed" TitleVisibility="Collapsed">
```

The code-behind class must derive from `ThemedWindow` too
(`public partial class ShellWindow : ThemedWindow`).

With `AllowContentOverTitleBar="True"` the transparent, draggable title bar covers the top
strip of content: anything interactive there is unclickable. Push pages down with
`Margin="{StaticResource {x:Static joufflu:Dimensions.TitleBarHeightOffset}}"` or host
pages in `FullContainer`; keep the overlay/toast containers full-bleed.

## Shell window

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}">
    <nav:OverlayContainer Overlays="{Binding Overlays}">
        <DockPanel>
            <nav:NavigationMenu DockPanel.Dock="Left" Navigator="{Binding Navigator}">
                <nav:NavigationTitle>Main</nav:NavigationTitle>
                <nav:NavigationItem TargetType="{x:Type vm:HomeViewModel}">
                    <nav:NavigationItem.Icon>
                        <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Home}" />
                    </nav:NavigationItem.Icon>
                    Home
                </nav:NavigationItem>
                <nav:NavigationGroup Header="Settings">
                    <nav:NavigationItem TargetType="{x:Type vm:ProfileViewModel}">Profile</nav:NavigationItem>
                </nav:NavigationGroup>
            </nav:NavigationMenu>
            <ContentControl Content="{Binding Navigator.CurrentPage}" />
        </DockPanel>
    </nav:OverlayContainer>
</feedback:ToastContainer>
```

## NavigationMenu

- `NavigationItem` (`TargetType` = page view-model type, `Icon`, content = label),
  `NavigationGroup` (`Header`, expands to children, nestable), `NavigationTitle` (section label).
- A chevron collapses the menu to an icon rail; labels then show as right tooltips.
- An item is selected while `CurrentPage` is of its `TargetType`: give each entry its own
  page type.

## Navigator

```csharp
private readonly Dictionary<Type, object> _pages;
public Navigator Navigator { get; }

public ShellViewModel()
{
    _pages = new object[] { new HomeViewModel(Overlays, Toasts), new ProfileViewModel() }
        .ToDictionary(p => p.GetType());
    Navigator = new Navigator(type => _pages.GetValueOrDefault(type));   // resolver: Type -> page
    Navigator.Navigate(typeof(HomeViewModel));                           // start page
}
```

- `Navigate(Type)` goes through the resolver (a `null` result keeps the current page);
  `Navigate(object page)` uses the instance directly.
- `CurrentPage`, `Navigated` event. The resolver may also create pages lazily or via DI.
- A page implementing `IPage` gets `OnNavigatedTo()` / `OnNavigatedFrom()` (default no-op
  interface methods; implement only what you need).

## FullContainer

Page host that puts `Header` in the title-bar strip and scrolls content below it (for
`AllowContentOverTitleBar` windows). `Header` is rendered as `H1`; use `HeaderTemplate`
for a title + toolbar.

```xml
<nav:FullContainer Header="Profile"> … page content … </nav:FullContainer>
```

## Overlays (modals)

`IOverlayService`:

| Member | Purpose |
|---|---|
| `Task<bool?> Show(object content, OverlayOptions? options = null)` | Push content; completes when closed (`null` = dismissed) |
| `Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = Neutral)` | Built-in Cancel/Confirm dialog |
| `Close(object content, bool? result = null)` | Close that content (prefer over `CloseTop`) |
| `CloseTop(bool? result = null)` | Close the top overlay |

`OverlayOptions`: `Title`, `ShowCloseButton` (true), `CloseOnClickAway` (true; set false
to force the action buttons), `FullScreen` (false).

`EnumConfirmationType`: `Neutral`, `Info`, `Success`, `Warning`, `Danger` (colours the
confirm button).

```csharp
if (await _overlays.Confirm("Delete this item? This can't be undone.", "Please confirm",
                            EnumConfirmationType.Danger) == true)
    Delete();
```

### Custom overlay content: `OverlayViewModel`

Derive from `OverlayViewModel` (yes/no) or `OverlayViewModel<TResult>` (returns a value).
It carries its own `Options` (set in the constructor), a `CancelCommand`, and
`Close(result)`. Add a `DataTemplate` for it like any page.

```csharp
public partial class EditNameViewModel : OverlayViewModel<string>
{
    public EditNameViewModel(IOverlayService overlays, string name) : base(overlays)
    {
        Options.Title = "Rename";
        Options.CloseOnClickAway = false;
        Name = name;
    }

    [ObservableProperty] private string _name;

    [RelayCommand]
    private void Save() => Close(Name);   // Close(TResult) validates with a result
}

// Caller — default(TResult) when cancelled/dismissed
string? name = await OverlayViewModel<string>.ShowAsync(new EditNameViewModel(_overlays, current));
```

```xml
<!-- View: the content owns its buttons -->
<StackPanel MinWidth="320" toolkit:Spacing.Gap="12">
    <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" />
    <StackPanel HorizontalAlignment="Right" Orientation="Horizontal" toolkit:Spacing.Gap="8">
        <Button Command="{Binding CancelCommand}" Content="Cancel" Style="{StaticResource SecondaryButton}" />
        <Button Command="{Binding SaveCommand}" Content="Save" Style="{StaticResource PrimaryButton}" />
    </StackPanel>
</StackPanel>
```

Any object also works as content: pass `OverlayOptions` to `Show` and close it with
`overlays.Close(content, result)`. Content implementing `IOverlayContent` supplies its own
`Options`.

## Paging

Page selector; it never touches the data.

```xml
<DataGrid ItemsSource="{Binding PageItems}" />
<nav:Paging Total="{Binding Total}"
            PageNumber="{Binding PageNumber, Mode=TwoWay}"
            Capacity="{Binding Capacity, Mode=TwoWay}" />
```

| Property | Default | Notes |
|---|---|---|
| `Total` | -1 | Items in the whole set; -1 = unknown (range label hidden, list grows one page ahead) |
| `PageNumber` | 1 | 1-based, clamped. **Needs `Mode=TwoWay`** |
| `Capacity` | 10 | Items per page from `AvailableCapacities` (5, 10, 25, 50, 100, 200). **Needs `Mode=TwoWay`** |
| `PageMax`, `IntervalMin`, `IntervalMax` | read-only | |

Reload the slice from the view-model setters of `PageNumber`/`Capacity`
(`Skip((PageNumber - 1) * Capacity).Take(Capacity)`). Code-behind alternative:
`paging.PagingChange += (page, capacity) => …`.
