# Joufflu WPF Components

WPF inputs and reusable components for .NET, built on a small design system of
themed brushes, dimensions and layout helpers. Every control reads its colours
through `DynamicResource`, so the whole UI re-themes live between Light and Dark.

![preview](./images/preview.PNG)

📖 **Full documentation:** <https://ndegheselle.github.io/Joufflu/>

## What's inside

| Section | Contents |
|---|---|
| **Inputs** (`Joufflu.Inputs`) | `NumericUpDown`, `DecimalUpDown`, `TimeSpanPicker`, `FormatTextBox`, `Search`, `ComboBoxSearch`, `ComboBoxTags`, `TextEditable`, `FilePicker`, `ColorPicker` |
| **Navigation** (`Joufflu.Navigation`) | `NavigationMenu`, `NavigationContainer` and modal overlays driven by a `Navigator` |
| **Custom controls** (`Joufflu`) | `FontIcon`, `Badge`, `Spinner`, toasts |
| **Toolkit** (`Joufflu`) | Sizing and spacing attached properties, `ThemeManager` / `ThemeSwitcher`, live theme customization, and the application shell (`ThemedWindow`) |

The **Natives** — WPF's built-in controls (buttons, text boxes, combo boxes,
data grid, …) restyled to match the design system — come along with the core
`Joufflu` styles.

## Getting started

1. Reference the `Joufflu` projects you need (`Joufflu`, `Joufflu.Inputs`,
   `Joufflu.Navigation`) from your WPF app.
2. Merge the control styles in `App.xaml`:

   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

3. Initialize the theme manager once at startup, before the first window shows:

   ```csharp
   // App.xaml.cs — OnStartup
   ThemeManager.Instance.Initialize();
   ```

4. Use the controls as shown in the [documentation](https://ndegheselle.github.io/Joufflu/).

### Design system

The design system is exposed as resource keys you can override in your own
dictionary (merged **after** the Joufflu resources):

- **Colours / brushes** — `joufflu:Colors.*` and `joufflu:Brushes.*`, including
  the semantic families (primary, secondary, success, info, warning, danger).
- **Dimensions** — `joufflu:Dimensions.*` (corner radius, border thickness,
  spacing, control heights, font sizes and padding per size).

Run the gallery and open **Customize theme** to tweak these interactively and
generate a ready-to-merge dictionary.

## Running the samples

- Created with Visual Studio Community 2022, requires the *.NET Desktop
  development* workload.
- Open `Joufflu.sln` and start the `Joufflu.Samples` project to explore
  every control, theme and toolkit helper interactively.

## Why does this exist

To train myself to create generic components that can easily be reused across
projects, and to find a clean way to build them — WPF offers several approaches,
each with trade-offs:

- Derive from `ContentControl` (or another base control), customized with a
  resource dictionary — binding events and data to the UI is painful (even with
  commands).
- Customize with a XAML class attached directly to the control — doesn't let the
  consumer easily customize the control's content.

## Acknowledgments

- [Lucide](https://lucide.dev/) icon font
- [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) for MVVM boilerplate

I know [Extended WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit) has
already done all the inputs and much more, but you can't fully appreciate
something without knowing how hard it is to do.
