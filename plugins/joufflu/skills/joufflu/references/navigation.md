# App shell and navigation (`Joufflu` + `Joufflu.Navigation`)

```xml
xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
```

```csharp
using Joufflu.Navigation;              // Navigator, Overlayer, IOverlayer, OverlayOptions, IOverlayContent, EnumConfirmationType, IPage
using Joufflu.Feedback;                // ToastService, IToastService
```

## Architecture

Three services, owned by a shell view model and shared with every page:

| Service | Role | Host control |
|---|---|---|
| `Navigator` | Holds `CurrentPage` (a view model) | `ContentControl Content="{Binding Navigator.CurrentPage}"` + `nav:NavigationMenu` |
| `Overlayer` (`IOverlayer`) | Stack of modal overlays | `nav:OverlayContainer Overlays="…"` |
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
                       AllowContentOverTitleBar="True" TitleVisibility="Collapsed">
```

The title bar has no icon by default: put `<controls:AppIcon />` in
`ThemedWindow.TitleBarContent` (application icon or `Source`; click opens the system menu,
double click closes).

The code-behind class must derive from `ThemedWindow` too
(`public partial class ShellWindow : ThemedWindow`).

With `AllowContentOverTitleBar="True"` the transparent, draggable title bar covers the top
strip of content: anything interactive there is unclickable. Push pages down with
`Margin="{StaticResource {x:Static joufflu:Dimensions.TitleBarHeightOffset}}"`; keep the overlay/toast containers full-bleed.

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

## Overlays (modals)

`IOverlayer`:

| Member | Purpose |
|---|---|
| `Task<bool?> ShowAsync(object content, OverlayOptions? options = null)` | Push content; completes when closed: `true` validated, `false` cancelled, `null` ignored |
| `Task<TResult?> ShowAsync<TResult>(IOverlayContent<TResult> content, OverlayOptions? options = null)` | Same, handing back `content.Result` when validated, `default` otherwise |
| `Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = Neutral)` | Built-in Cancel/Confirm dialog |
| `Validate(object content)` / `Cancel(object content)` / `Ignore(object content)` | Close the overlay showing that content (not whichever is on top) with `true` / `false` / `null`. The close cross and a click away ignore. |

`OverlayOptions`: `Title`, `ShowCloseButton` (true), `CloseOnClickAway` (true; set false
to force the action buttons), `FullScreen` (false).

`EnumConfirmationType`: `Neutral`, `Info`, `Success`, `Warning`, `Danger` (colours the
confirm button).

```csharp
if (await _overlays.Confirm("Delete this item? This can't be undone.", "Please confirm",
                            EnumConfirmationType.Danger) == true)
    Delete();
```

### Custom overlay content: `IOverlayContent`

Any object works as content, with `OverlayOptions` passed to `ShowAsync`. Content
implementing `IOverlayContent` supplies its own `Options` instead;
`IOverlayContent<TResult>` adds the `Result` handed back when validated. The content owns
its buttons and closes itself through the overlayer. Add a `DataTemplate` for it like any
page.

```csharp
public partial class EditNameViewModel : ObservableObject, IOverlayContent<string>
{
    private readonly IOverlayer _overlays;

    public EditNameViewModel(IOverlayer overlays, string name)
    {
        _overlays = overlays;
        _name = name;
    }

    public OverlayOptions Options { get; } = new() { Title = "Rename", CloseOnClickAway = false };

    [ObservableProperty] private string _name;

    public string? Result => Name;

    [RelayCommand]
    private void Save() => _overlays.Validate(this);

    [RelayCommand]
    private void Cancel() => _overlays.Cancel(this);
}

// Caller — null (default of TResult) when cancelled or ignored
string? name = await _overlays.ShowAsync(new EditNameViewModel(_overlays, current));
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

`IOverlayContent` derives from `IPage`, so content gets `OnNavigatedTo` / `OnNavigatedFrom`
when shown and closed.

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
