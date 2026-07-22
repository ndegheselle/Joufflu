---
title: Tutorial
nav_order: 2
---

# Tutorial: a navigable app shell

Builds a small MVVM app on `Joufflu.Navigation`: a collapsible side menu that
switches pages, a shared shell view model owning the navigation services, an
example page, and a modal overlay plus a toast raised from it. The result
matches the wiring the **`Joufflu.Samples`** gallery uses.

{: .note }
> Assumes the packages are installed and `App.xaml` merges the Joufflu resource
> dictionaries, with `ThemeManager.Instance.Initialize()` called at startup. See
> [Getting started](index.md#getting-started) first if not.

The snippets use these namespaces:

```xml
xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
```

## How it fits together

Three services drive everything, shared between the shell window and the pages:

| Service | Role |
|---|---|
| `Navigator` | Holds the current page (a view model) and switches between pages. |
| `OverlayService` | Shows modal overlays on top of the current page. |
| `ToastService` | Shows stacking, auto-dismissing notifications. |

Navigation is **view-model-first**: navigate to a *view model* and WPF resolves
the matching *view* through an implicit `DataTemplate`. `NavigationContainer`
renders the current page and hosts the overlay and toast stacks; `NavigationMenu`
drives the same `Navigator` from the side.

## Step 1 — The shared shell view model

The shell view model owns the three services so the window and every page share
the same instances. It also keeps a registry mapping each menu item's text
`Target` to a page. It starts empty; the first page is added in Step 4.

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Controls;
using Joufflu.Navigation;

public class ShellViewModel : ObservableObject
{
    // Shared with the NavigationContainer and NavigationMenu in the shell window,
    // and injected into the pages that need them.
    public Navigator Navigator { get; } = new();
    public OverlayService Overlays { get; } = new();
    public ToastService Toasts { get; } = new();

    // Pages keyed by the text target used on the menu's NavigationItems.
    // Filled in as we add pages (Step 4).
    private readonly Dictionary<string, object> _pages = new();

    // Bound to NavigationMenu.TargetResolver so the menu can turn a target
    // string into the page to navigate to.
    public Func<string, object?> ResolveTarget { get; }

    public ShellViewModel()
    {
        ResolveTarget = target => _pages.GetValueOrDefault(target);
    }
}
```

## Step 2 — Map view models to views

An implicit `DataTemplate` (a `DataType` with no `x:Key`) maps each view model to
its view. Add one per page, typically in `App.xaml`. The `HomeViewModel` /
`HomeView` pair comes in Step 4; here is the mapping it needs:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Joufflu resource dictionaries merged here (see Getting started) -->
        </ResourceDictionary.MergedDictionaries>

        <!-- One DataTemplate per page: the view model resolves to its view. -->
        <DataTemplate DataType="{x:Type vm:HomeViewModel}">
            <views:HomeView />
        </DataTemplate>
    </ResourceDictionary>
</Application.Resources>
```

The same mechanism resolves overlay content — add a `DataTemplate` for every
overlay view model too (Step 5).

## Step 3 — The shell window

A `ThemedWindow` holding the side `NavigationMenu` and the `NavigationContainer`.
Bind both to the shell view model's services so they stay in sync: selecting a
menu item navigates the container, and its overlays/toasts use the shared
services.

The `d:DataContext` line gives the XAML designer the runtime view-model type, so
bindings like `{Binding Navigator}` and `{Binding ResolveTarget}` get IntelliSense
and design-time validation. No runtime effect.

```xml
<controls:ThemedWindow
    x:Class="MyApp.ShellWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
    xmlns:vm="clr-namespace:MyApp.ViewModels"
    Title="My App"
    Width="1000"
    Height="640"
    d:DataContext="{d:DesignInstance Type=vm:ShellViewModel}"
    mc:Ignorable="d">
    <DockPanel>
        <nav:NavigationMenu
            DockPanel.Dock="Left"
            Navigator="{Binding Navigator}"
            TargetResolver="{Binding ResolveTarget}">

            <nav:NavigationItem Target="home">
                <nav:NavigationItem.Icon>
                    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Home}" />
                </nav:NavigationItem.Icon>
                Home
            </nav:NavigationItem>
        </nav:NavigationMenu>

        <!-- Renders the current page and hosts the overlay + toast stacks. -->
        <nav:NavigationContainer
            Navigator="{Binding Navigator}"
            Overlays="{Binding Overlays}"
            Toasts="{Binding Toasts}" />
    </DockPanel>
</controls:ThemedWindow>
```

Create the window in code to pass the shell view model as its `DataContext`,
e.g. in `App.xaml.cs`:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    ThemeManager.Instance.Initialize();

    var shell = new ShellViewModel();
    new ShellWindow { DataContext = shell }.Show();
}
```

{: .note }
> Remove `StartupUri="MainWindow.xaml"` from `App.xaml`, or WPF opens that window
> too and you end up with two.

{: .note }
> Each item's `Target` is passed to `TargetResolver`, which returns the page. Add
> more items with the Step 4 recipe. A `NavigationGroup` expands to reveal
> children instead of navigating; a `NavigationTitle` is a section label. See
> [Navigation menu](navigation/navigation-menu.md) for the full
> markup.

## Step 4 — An example page

A page is a view model plus a matching view. The view model takes the shared
services to open overlays and raise toasts:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Navigation;

public class HomeViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;
    private readonly IToastService _toasts;

    public IRelayCommand SayHelloCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public HomeViewModel(IOverlayService overlays, IToastService toasts)
    {
        _overlays = overlays;
        _toasts = toasts;

        SayHelloCommand = new RelayCommand(SayHello);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    // ... commands implemented in Step 5 ...
}
```

Its view:

```xml
<UserControl
    x:Class="MyApp.Views.HomeView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu">
    <StackPanel Margin="24" joufflu:Spacing.Gap="12">
        <TextBlock Style="{StaticResource H1}" Text="Home" />
        <StackPanel Orientation="Horizontal" joufflu:Spacing.Gap="8">
            <Button Command="{Binding SayHelloCommand}" Content="Say hello" />
            <Button Command="{Binding DeleteCommand}" Content="Delete…"
                    Style="{StaticResource Danger}" />
        </StackPanel>
    </StackPanel>
</UserControl>
```

Register the page in the shell and land on it at startup. In `ShellViewModel`
(Step 1), the constructor builds the page with the shared services and navigates
to it:

```csharp
public ShellViewModel()
{
    // Register the page under the target used by its NavigationItem.
    _pages["home"] = new HomeViewModel(Overlays, Toasts);

    ResolveTarget = target => _pages.GetValueOrDefault(target);

    // Navigate to the default page so the window doesn't start empty.
    Navigator.Navigate(_pages["home"]);
}
```

The recipe per page, repeated for each:

1. Write its view model and view.
2. Map them with a `DataTemplate` (Step 2).
3. Register the view model under a target in `_pages` (above).
4. Point a `NavigationItem` at that target (Step 3).

## Step 5 — Modals and toasts from the page

### A toast

One call on the shared `ToastService`. Toasts stack in the top-right and
auto-dismiss:

```csharp
private void SayHello() => _toasts.Success("Hello!", "Greetings");
```

### A modal overlay, then a toast with the result

`OverlayService.Show` returns a `Task<bool?>` that completes when the overlay
closes: `true`/`false` from the action buttons, `null` when dismissed. The
overlay content owns its buttons and closes itself through the service.

Overlay content view model:

```csharp
public class DeleteConfirmViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;

    public string Message { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand DeleteCommand { get; }

    public DeleteConfirmViewModel(IOverlayService overlays, string message)
    {
        _overlays = overlays;
        Message = message;
        // CloseTop passes the result back to the awaiting Show() call.
        CancelCommand = new RelayCommand(() => _overlays.CloseTop(false));
        DeleteCommand = new RelayCommand(() => _overlays.CloseTop(true));
    }
}
```

Its view (add a matching `DataTemplate` as in Step 2):

```xml
<UserControl
    x:Class="MyApp.Views.DeleteConfirmView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel MinWidth="320">
        <TextBlock Text="{Binding Message}" TextWrapping="Wrap" />
        <StackPanel Margin="0,16,0,0" HorizontalAlignment="Right" Orientation="Horizontal">
            <Button Command="{Binding CancelCommand}" Content="Cancel"
                    Style="{StaticResource Secondary}" />
            <Button Margin="8,0,0,0" Command="{Binding DeleteCommand}" Content="Delete"
                    Style="{StaticResource Danger}" />
        </StackPanel>
    </StackPanel>
</UserControl>
```

Open it from the page and react to the result with a toast:

```csharp
private async Task DeleteAsync()
{
    var content = new DeleteConfirmViewModel(_overlays, "Delete this item? This can't be undone.");
    var options = new OverlayOptions { Title = "Please confirm", CloseOnClickAway = false };

    bool? result = await _overlays.Show(content, options);

    if (result == true)
        _toasts.Success("Item deleted.", "Confirmed");
    else
        _toasts.Info("Cancelled.");
}
```

The full loop: the menu navigates the `Navigator`, the page opens a modal through
the shared `OverlayService`, awaits its result, and confirms with the shared
`ToastService`.

## Where to go next

- [Navigation menu](navigation/navigation-menu.md) — groups,
  nesting, the collapsible rail and the menu `Header`.
- [Overlays](navigation/overlays.md) — overlay options and
  stacking.
- [Toasts](custom-controls/toasts.md) — toast types and sticky
  toasts.
