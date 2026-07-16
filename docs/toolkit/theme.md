---
title: Theme
parent: Toolkit
nav_order: 3
---

# Theme

Joufflu ships two colour palettes — `Themes/Light.xaml` and `Themes/Dark.xaml` — with the
same keys and different values. `ThemeManager` picks the active one at runtime by swapping a
single merged dictionary; because every control reads its colours through `DynamicResource`,
the whole UI re-themes live.

## App setup

Merge only the control styles in `App.xaml`. Do **not** hardcode a theme dictionary — the
manager inserts the matching Light/Dark dictionary ahead of the styles at runtime.

```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Initialize the manager once at startup, before the first window is shown:

```csharp
// App.xaml.cs — OnStartup
ThemeManager.Instance.Initialize();
```

## ThemeManager

The manager owns the active theme. Set `Mode` from anywhere; the choice is persisted (to
`%AppData%\joufflu\settings.json`) and restored on the next launch.

```csharp
using Joufflu.Themes;

ThemeManager.Instance.Mode = ThemeMode.Dark;    // Light / Dark / System

bool isDark = ThemeManager.Instance.IsDark;      // the theme actually on screen
```

In `ThemeMode.System` the manager reads the Windows apps theme and keeps following it while
the app runs, so the UI tracks the OS light/dark switch automatically.

## ThemeSwitcher

A drop-in segmented control (System / Light / Dark) bound to `ThemeManager`:

```xml
<controls:ThemeSwitcher xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu" />
```
