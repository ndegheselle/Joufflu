---
title: Recipes
nav_order: 3
---

# Recipes

Complete, copy-paste building blocks for the screens most apps need. They assume the
packages are installed and `App.xaml` merges the Joufflu resources with
`ThemeManager.Instance.Initialize()` called at startup (see
[Getting started](index.md#getting-started)). For a whole app shell, follow the
[Tutorial](tutorial.md).

Every snippet uses these namespaces:

```xml
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
xmlns:inputs="clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs"
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
```

## A settings page

A `Card` per group of settings, headings from the typography scale, `Spacing.Gap`
instead of per-element margins, and a toast to confirm the save.

```xml
<StackPanel Margin="24" toolkit:Spacing.Gap="16">
    <TextBlock Style="{StaticResource H1}" Text="Settings" />

    <Border Style="{StaticResource Card}">
        <StackPanel toolkit:Spacing.Gap="12">
            <TextBlock Style="{StaticResource H4}" Text="General" />
            <TextBlock Style="{StaticResource Muted}" Text="Applies to every project." />

            <TextBox Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />
            <inputs:NumericUpDown Value="{Binding MaxItems, Mode=TwoWay}" />
            <ComboBox ItemsSource="{Binding Languages}" SelectedItem="{Binding Language}" />
            <CheckBox Content="Check for updates at startup" IsChecked="{Binding CheckUpdates}" />
        </StackPanel>
    </Border>

    <StackPanel HorizontalAlignment="Right" Orientation="Horizontal" toolkit:Spacing.Gap="8">
        <Button Command="{Binding ResetCommand}" Content="Reset" Style="{StaticResource SecondaryButton}" />
        <Button Command="{Binding SaveCommand}" Content="Save" Style="{StaticResource PrimaryButton}" />
    </StackPanel>
</StackPanel>
```

```csharp
// SaveCommand, with the shared ToastService injected
private void Save()
{
    _settings.Save();
    _toasts.Success("Settings saved.", "Done");
}
```

## Brand it: a custom theme

Customizing is overriding tokens, not restyling controls. Create a dictionary with
the keys you want to change, merge it **after** the Joufflu resources, and every
control, restyled native and Joufflu control picks the change up. Use the gallery's
**Customize theme** page to pick colours and dimensions live and copy the generated
dictionary rather than writing it by hand.

```xml
<!-- App.xaml -->
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
    <ResourceDictionary Source="MyTheme.xaml" />   <!-- your overrides, after Joufflu -->
</ResourceDictionary.MergedDictionaries>
```

To make a palette selectable by name next to System / Light / Dark, register it
before `Initialize()`:

```csharp
ThemeManager.Instance.Register(
    "Ocean",
    new Uri("pack://application:,,,/MyApp;component/Themes/Ocean.xaml"),
    isDark: true);

ThemeManager.Instance.Initialize();
```

The keys to override are listed in [Design tokens](toolkit/tokens.md) and the full
flow in [Customize theme](toolkit/customize-theme.md) and [Theme](toolkit/theme.md).

## A theme switcher

`ThemeManager.Theme` takes `System`, `Light`, `Dark` or the name of a registered
custom theme and re-themes the whole UI live. Persisting the choice is up to the app;
see [Theme](toolkit/theme.md#persisting-the-theme).

```csharp
using Joufflu.Themes;

ThemeManager.Instance.Theme = ThemeManager.Dark;
```

## A searchable, paged table

`Search` debounces typing before it raises `SearchChanged`, `Paging` tells the view
model which slice to show. The view model filters, counts and slices.

```xml
<DockPanel Margin="24">
    <inputs:Search x:Name="Search" DockPanel.Dock="Top" Margin="0,0,0,12" />

    <nav:Paging
        DockPanel.Dock="Bottom"
        Margin="0,12,0,0"
        Total="{Binding Total}"
        PageNumber="{Binding PageNumber, Mode=TwoWay}"
        Capacity="{Binding Capacity, Mode=TwoWay}" />

    <DataGrid AutoGenerateColumns="False" ItemsSource="{Binding PageItems}">
        <DataGrid.Columns>
            <DataGridTextColumn Binding="{Binding Id}" Header="Id" />
            <DataGridTextColumn Binding="{Binding Name}" Header="Name" />
            <DataGridTemplateColumn Header="Status">
                <DataGridTemplateColumn.CellTemplate>
                    <DataTemplate>
                        <feedback:Badge Variant="Success">Active</feedback:Badge>
                    </DataTemplate>
                </DataGridTemplateColumn.CellTemplate>
            </DataGridTemplateColumn>
        </DataGrid.Columns>
    </DataGrid>
</DockPanel>
```

```csharp
// code-behind: route the debounced text to the view model
Search.SearchChanged += text => ((ItemsViewModel)DataContext).Filter = text;
```

See [Paging](navigation/paging.md#paged-data-grid) for the view-model side.

## Confirm before deleting

One awaited call shows a modal over the whole window, with the confirm button
coloured for a destructive action. The `Overlayer` is the one the window's
`OverlayContainer` is bound to.

```csharp
bool? confirmed = await _overlays.Confirm(
    "Delete the selected item? This action cannot be undone.",
    "Please confirm",
    EnumConfirmationType.Danger);

if (confirmed == true)
{
    Delete();
    _toasts.Success("Item deleted.");
}
```

## A status row with badges and a spinner

```xml
<StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
    <feedback:Spinner />
    <TextBlock VerticalAlignment="Center" Text="Syncing..." />
    <feedback:Badge Variant="Info">12 pending</feedback:Badge>
    <feedback:Badge Variant="Danger">3 errors</feedback:Badge>
</StackPanel>
```

## A form with a different density

`Sizing.Size` (`xs`, `sm`, `md`, `lg`...) is inherited, so setting it once on a panel
resizes every control inside it.

```xml
<StackPanel toolkit:Sizing.Size="sm" toolkit:Spacing.Gap="8">
    <TextBox />
    <ComboBox />
    <Button Content="Apply" Style="{StaticResource PrimaryButton}" />
</StackPanel>
```

## Rules these recipes follow

- Look changes are token overrides in a dictionary merged after Joufflu (or a registered
  theme), never edits to control styles.
- Colours come from the theme (`{DynamicResource {x:Static joufflu:Brushes.Foreground100Brush}}`),
  never from hex values, so Light / Dark and custom themes keep working.
- Gaps between children use `Spacing.Gap`, not a `Margin` on each child.
- Roles are named styles (`PrimaryButton`, `DangerButton`, `Card`, `H1`, `Muted`),
  not local setters.
- Modals and toasts go through the shared `Overlayer` and `ToastService`, never
  `MessageBox`.
