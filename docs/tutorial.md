---
title: Tutorial
nav_order: 2
---

# Tutorial: a navigable app shell

This walkthrough builds a small MVVM app on top of `Joufflu.Navigation`: a
collapsible side menu that switches between pages, a shared shell view model that
owns the navigation services, an example page, and — from that page — a modal
overlay and a toast.

By the end you will have the same wiring the **`Joufflu.Samples`** gallery uses.

{: .note }
> This assumes the packages are installed and `App.xaml` merges the Joufflu
> resource dictionaries, with `ThemeManager.Instance.Initialize()` called at
> startup. See [Getting started]({{ site.baseurl }}/#getting-started) if you
> haven't done that yet.

The snippets use these namespaces:

```xml
xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
```

## How it fits together

Three services drive everything, and they are shared between the shell window and
the pages:

| Service | Role |
|---|---|
| `Navigator` | Holds the current page (a view model) and switches between pages. |
| `OverlayService` | Shows modal overlays on top of the current page. |
| `ToastService` | Shows stacking, auto-dismissing notifications. |

Navigation is **view-model-first**: you navigate to a *view model*, and WPF
resolves the matching *view* through an implicit `DataTemplate`. The
`NavigationContainer` renders the current page and hosts the overlay and toast
stacks; the `NavigationMenu` drives the same `Navigator` from the side.

## Step 1 — The shared shell view model

The shell view model owns the three services so the window **and** every page can
use the same instances. It also maps each menu item's text `Target` to the page
to navigate to.

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Controls;
using Joufflu.Navigation;

public class ShellViewModel : ObservableObject
{
    // Shared with the NavigationContainer and NavigationMenu in the shell window,
    // and injected into pages that need them.
    public Navigator Navigator { get; } = new();
    public OverlayService Overlays { get; } = new();
    public ToastService Toasts { get; } = new();

    // Pages keyed by the text target used on the menu's NavigationItems.
    private readonly Dictionary<string, object> _pages;

    // Bound to NavigationMenu.TargetResolver so the menu can resolve its targets.
    public Func<string, object?> ResolveTarget { get; }

    public ShellViewModel()
    {
        _pages = new()
        {
            ["home"] = new HomeViewModel(Overlays, Toasts),
            ["settings"] = new SettingsViewModel(),
        };

        ResolveTarget = target => _pages.GetValueOrDefault(target);

        // Land on a page at startup.
        Navigator.Navigate(_pages["home"]);
    }
}
```

Pages that need to open overlays or raise toasts receive the services through
their constructor (see `HomeViewModel` below). Because the shell created the
services, the page and the shell window share the exact same instances.

## Step 2 — Map view models to views

Navigation resolves a view model to its view through an implicit `DataTemplate`
(a `DataTemplate` with a `DataType` but no `x:Key`). Declare one per page,
typically in `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Joufflu resource dictionaries merged here (see Getting started) -->
        </ResourceDictionary.MergedDictionaries>

        <!-- Each page view model resolves to its view. -->
        <DataTemplate DataType="{x:Type vm:HomeViewModel}">
            <views:HomeView />
        </DataTemplate>
        <DataTemplate DataType="{x:Type vm:SettingsViewModel}">
            <views:SettingsView />
        </DataTemplate>
    </ResourceDictionary>
</Application.Resources>
```

The same mechanism resolves overlay content, so add a `DataTemplate` for every
overlay view model too.

## Step 3 — The shell window

The shell window is a `ThemedWindow` with the side `NavigationMenu` and the
`NavigationContainer`. Bind both to the shell view model's services so they stay
in sync — selecting a menu item navigates the container, and the container's
overlays/toasts use the shared services.

```xml
<controls:ThemedWindow
    x:Class="MyApp.ShellWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
    xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
    xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
    Title="My App"
    Width="1000"
    Height="640">
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
            <nav:NavigationItem Target="settings">
                <nav:NavigationItem.Icon>
                    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Settings}" />
                </nav:NavigationItem.Icon>
                Settings
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

Set the window's `DataContext` to the shell view model when you show it, e.g. in
`App.xaml.cs`:

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
> Each menu item's `Target` string is passed to `TargetResolver`, which returns
> the page to navigate to. A `NavigationGroup` expands to reveal children instead
> of navigating, and a `NavigationTitle` is a section label — see
> [Navigation menu]({{ site.baseurl }}/navigation/navigation-menu/) for the full
> menu markup.

## Step 4 — An example page

A page is just a view model and a matching view. The view model takes the shared
services so it can open overlays and raise toasts:

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

Register the page and add its menu item (already done in Steps 1–3): put it in
the shell's `_pages` dictionary under `"home"`, add the matching `DataTemplate`,
and point a `NavigationItem` at `Target="home"`.

## Step 5 — Modals and toasts from the page

### A toast

Toasts are one call on the shared `ToastService`. They stack in the top-right and
auto-dismiss:

```csharp
private void SayHello() => _toasts.Success("Hello!", "Greetings");
```

### A modal overlay, then a toast with the result

`OverlayService.Show` returns a `Task<bool?>` that completes when the overlay
closes — `true`/`false` from the action buttons, or `null` when dismissed. The
overlay content owns its buttons and closes itself through the service.

First the overlay content view model:

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

Now open it from the page and react to the result with a toast:

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

That's the full loop: the menu navigates the `Navigator`, the page opens a modal
through the shared `OverlayService`, awaits its result, and confirms with the
shared `ToastService`.

## Where to go next

- [Navigation menu]({{ site.baseurl }}/navigation/navigation-menu/) — groups,
  nesting, the collapsible rail and the menu `Header`.
- [Overlays]({{ site.baseurl }}/navigation/overlays/) — overlay options and
  stacking.
- [Toasts]({{ site.baseurl }}/custom-controls/toasts/) — toast types and sticky
  toasts.
