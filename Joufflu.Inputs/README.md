# Joufflu.Inputs

**Themed WPF input controls for [Joufflu](https://www.nuget.org/packages/Joufflu).**

A set of ready-to-use input controls that follow the Joufflu design system and
re-theme live between Light and Dark along with the rest of your UI.

[![Joufflu.Inputs on NuGet](https://img.shields.io/nuget/v/Joufflu.Inputs?label=Joufflu.Inputs&logo=nuget)](https://www.nuget.org/packages/Joufflu.Inputs)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## Controls

| Control | Purpose |
|---|---|
| `NumericUpDown` | Integer input with increment/decrement. |
| `DecimalUpDown` | Decimal input with increment/decrement. |
| `TimeSpanPicker` | Edit a `TimeSpan` value. |
| `FormatTextBox` | Text box with masked / formatted input. |
| `Search` | Debounced search box, reported once typing settles. |
| `ComboBoxSearch` | Combo box with searchable / filterable items. |
| `ComboBoxTags` | Multi-select combo box rendering choices as tags. |
| `TextEditable` | Label that turns into an inline editor on double click. |
| `FilePicker` | Pick a file through the system dialog. |
| `ColorPicker` | Pick a colour. |
| `Dropdown` | Attached properties opening a popup of any content off a `ToggleButton`. |

## Getting started

`Joufflu.Inputs` builds on the core `Joufflu` package. Add it (the core comes
along as a dependency):

```sh
dotnet add package Joufflu.Inputs
```

Merge the Joufflu control styles in `App.xaml` and initialize the theme manager
once at startup:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

```csharp
// App.xaml.cs — OnStartup
ThemeManager.Instance.Initialize();
```

The controls' own styles load from the package's `Themes/Generic.xaml`, so nothing
else needs merging. Reference them from the `Joufflu.Inputs.Controls` namespace:

```xml
xmlns:inputs="clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs"
```

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/>
