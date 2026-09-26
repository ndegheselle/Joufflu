---
name: joufflu-new-app
description: >-
  Scaffold or bootstrap a WPF application on Joufflu: add the NuGet packages, wire
  App.xaml and ThemeManager, and build an MVVM app shell (ThemedWindow,
  ToastContainer, OverlayContainer, NavigationMenu, Navigator, view-model-first
  pages with DataTemplates, a first page, a modal overlay and a toast). Use when the
  user asks to create a new Joufflu app, add Joufflu to an existing WPF project,
  set up navigation / side menu / modals / toasts with Joufflu, or says
  "/joufflu-new-app". For API details of individual controls use the `joufflu` skill.
---

# Scaffold a Joufflu app

Produces a running WPF app wired the way the `Joufflu.Samples` gallery is: a themed
window, a collapsible side menu switching pages, shared overlay and toast services.
Read the `joufflu` skill's `references/navigation.md` if you need more than what is here.

## 0. Inspect first

- Is there an existing WPF project (`<UseWPF>true</UseWPF>` in a `.csproj`)? Then add
  Joufflu to it instead of creating a new one, and keep its root namespace and folders.
- Joufflu targets `net10.0-windows`: the app's `TargetFramework` must be
  `net10.0-windows` or later. If it is older, tell the user and ask before retargeting.
- Ask only what changes the output: app name (if creating), which pages to start with,
  and whether they want the title bar over content (a modern full-height side menu) or a
  classic title bar. Default: pages `Home` and `Settings`, classic title bar.

## 1. Project and packages

New project:

```bash
dotnet new wpf -n MyApp -f net10.0
```

Then ensure the `.csproj` has `<TargetFramework>net10.0-windows</TargetFramework>`,
`<UseWPF>true</UseWPF>`, `<Nullable>enable</Nullable>`.

```bash
dotnet add package Joufflu
dotnet add package Joufflu.Navigation
dotnet add package Joufflu.Feedback
dotnet add package CommunityToolkit.Mvvm
```

Add `Joufflu.Inputs` / `Joufflu.FileExplorer` / `Joufflu.Data` only if the pages need them.

## 2. App.xaml

Merge only the core resources, remove `StartupUri` (the window is created in code), and
map each page view model to its view:

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:MyApp.ViewModels"
             xmlns:views="clr-namespace:MyApp.Views">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- ThemeManager inserts the Light/Dark palette ahead of this at runtime -->
                <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <!-- One implicit DataTemplate per page and per overlay view model -->
            <DataTemplate DataType="{x:Type vm:HomeViewModel}"><views:HomeView /></DataTemplate>
            <DataTemplate DataType="{x:Type vm:SettingsViewModel}"><views:SettingsView /></DataTemplate>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## 3. App.xaml.cs

```csharp
using System.Windows;
using Joufflu.Themes;
using MyApp.ViewModels;

namespace MyApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.Instance.Initialize();   // before the first window

        var shell = new ShellViewModel();
        new ShellWindow { DataContext = shell }.Show();
    }
}
```

Delete the template's `MainWindow.xaml(.cs)` if it is no longer used.

## 4. ViewModels/ShellViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.Navigation;
using Joufflu.Navigation.Controls;

namespace MyApp.ViewModels;

public class ShellViewModel : ObservableObject
{
    public Navigator Navigator { get; }
    public OverlayService Overlays { get; } = new();
    public ToastService Toasts { get; } = new();

    // Pages keyed by their own type: what each NavigationItem.TargetType points at.
    private readonly Dictionary<Type, object> _pages;

    public ShellViewModel()
    {
        _pages = new object[]
        {
            new HomeViewModel(Overlays, Toasts),
            new SettingsViewModel(),
        }.ToDictionary(page => page.GetType());

        Navigator = new Navigator(type => _pages.GetValueOrDefault(type));
        Navigator.Navigate(typeof(HomeViewModel));
    }
}
```

If the project uses dependency injection, resolve pages from the container in the
navigator's resolver instead of the dictionary, and register `OverlayService` /
`ToastService` as singletons behind `IOverlayService` / `IToastService`.

## 5. ShellWindow.xaml(.cs)

```xml
<controls:ThemedWindow
    x:Class="MyApp.ShellWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
    xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
    xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
    xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
    xmlns:vm="clr-namespace:MyApp.ViewModels"
    Title="My App" Width="1000" Height="640"
    d:DataContext="{d:DesignInstance Type=vm:ShellViewModel}"
    mc:Ignorable="d">
    <feedback:ToastContainer Toasts="{Binding Toasts}">
        <nav:OverlayContainer Overlays="{Binding Overlays}">
            <DockPanel>
                <nav:NavigationMenu DockPanel.Dock="Left" Navigator="{Binding Navigator}">
                    <nav:NavigationItem TargetType="{x:Type vm:HomeViewModel}">
                        <nav:NavigationItem.Icon>
                            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Home}" />
                        </nav:NavigationItem.Icon>
                        Home
                    </nav:NavigationItem>
                    <nav:NavigationItem TargetType="{x:Type vm:SettingsViewModel}">
                        <nav:NavigationItem.Icon>
                            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Settings}" />
                        </nav:NavigationItem.Icon>
                        Settings
                    </nav:NavigationItem>
                </nav:NavigationMenu>
                <ContentControl Content="{Binding Navigator.CurrentPage}" />
            </DockPanel>
        </nav:OverlayContainer>
    </feedback:ToastContainer>
</controls:ThemedWindow>
```

```csharp
using Joufflu.Controls;
namespace MyApp;
public partial class ShellWindow : ThemedWindow
{
    public ShellWindow() => InitializeComponent();
}
```

Title bar over content variant: add `AllowContentOverTitleBar="True"`
`IconVisibility="Collapsed"` `TitleVisibility="Collapsed"` on the window, and give the page
host `Margin="{StaticResource {x:Static joufflu:Dimensions.TitleBarHeightOffset}}"`
(`xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"`), or wrap each page's content in
`<nav:FullContainer Header="…">`. Never offset the Toast/Overlay containers.

## 6. A page: view model + view

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Feedback;
using Joufflu.Navigation;

namespace MyApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;
    private readonly IToastService _toasts;

    public HomeViewModel(IOverlayService overlays, IToastService toasts)
    {
        _overlays = overlays;
        _toasts = toasts;
    }

    [RelayCommand]
    private void SayHello() => _toasts.Success("Hello!", "Greetings");

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (await _overlays.Confirm("Delete this item? This can't be undone.", "Please confirm",
                                    EnumConfirmationType.Danger) == true)
            _toasts.Success("Item deleted.");
    }
}
```

```xml
<UserControl x:Class="MyApp.Views.HomeView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
             xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu">
    <StackPanel Margin="{DynamicResource {x:Static joufflu:Dimensions.SpacingThickness}}" toolkit:Spacing.Gap="12">
        <TextBlock Style="{StaticResource H1}" Text="Home" />
        <Border Style="{StaticResource Card}">
            <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
                <Button Command="{Binding SayHelloCommand}" Content="Say hello" Style="{StaticResource PrimaryButton}" />
                <Button Command="{Binding DeleteCommand}" Content="Delete…" Style="{StaticResource DangerButton}" />
            </StackPanel>
        </Border>
    </StackPanel>
</UserControl>
```

(`[RelayCommand]` on `DeleteAsync` generates `DeleteCommand`.)

A Settings page is a good place for a theme switcher: see the `joufflu-theme` skill or
`references/theming.md` of the `joufflu` skill.

Adding a page later, every time:
1. View model + view.
2. `DataTemplate` in `App.xaml`.
3. Register the instance in `_pages` (or the DI container).
4. `NavigationItem TargetType="{x:Type vm:NewViewModel}"` in the menu.

## 7. Verify

Run `dotnet build` and fix every error. Common failures:
- `ThemedWindow` in XAML but `Window` in the code-behind → make the partial class derive
  from `ThemedWindow`.
- Page shows its type name instead of the view → missing `DataTemplate`.
- Two windows open → `StartupUri` still in `App.xaml`.
- Unstyled controls / missing brushes → core `Resources.xaml` not merged, or
  `ThemeManager.Instance.Initialize()` not called before the window is shown.

Then offer to launch the app (`dotnet run`) so the user can check it.
