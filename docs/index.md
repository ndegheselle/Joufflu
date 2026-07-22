---
title: Home
nav_order: 1
---

# Joufflu WPF Components

**A modern WPF component library that makes your desktop apps look good by
default.**

WPF inputs and reusable components for .NET, built on a design system of themed
brushes, dimensions and layout helpers. Every control reads its colours through
`DynamicResource`, so the whole UI re-themes live between Light and Dark — no
restart, no flicker.

These pages mirror the controls in the **`Joufflu.Samples`** gallery app — same
titles, descriptions and code snippets. Each control page lists example markup you
can paste into your views.

## Highlights

- 🌗 **Live Light / Dark theming** — flip the theme at runtime and every control follows instantly.
- 🎨 **A real design system** — semantic colours, dimensions, sizing and spacing exposed as override-able resource keys.
- 🧩 **Ready-to-use inputs** — numeric, decimal and timespan pickers, searchable and tag combo boxes, file and colour pickers, inline-editable text.
- 🧭 **Navigation & overlays** — a navigation menu, a view-model-first page container and awaitable modal dialogs.
- 🪟 **Custom-chrome window & natives** — a themed application shell plus restyled built-in WPF controls that match out of the box.
- 📦 **Modular packages** — take just the core styles, or add inputs and navigation only where you need them.

{: .note }
> The gallery's **Natives** group — WPF's built-in controls restyled to match the
> design system (text boxes, combo boxes, data grid, …) — is mostly undocumented
> here; only natives with named style variants are (see
> [Native controls]({{ site.baseurl }}/natives/)). Otherwise these pages cover the
> custom controls the library adds.

## Sections

| Section | What's inside |
|---|---|
| [Native controls]({{ site.baseurl }}/natives/) | Buttons (solid, soft & outline variants) |
| [Inputs]({{ site.baseurl }}/inputs/) | `NumericUpDown`, `DecimalUpDown`, `TimeSpanPicker`, `FormatTextBox`, `Search`, `ComboBoxSearch`, `ComboBoxTags`, `TextEditable`, `FilePicker`, `ColorPicker` |
| [Navigation]({{ site.baseurl }}/navigation/) | `NavigationMenu`, overlays (modal dialogs) |
| [Custom controls]({{ site.baseurl }}/custom-controls/) | `FontIcon`, `Badge`, `Spinner`, toasts |
| [Toolkit]({{ site.baseurl }}/toolkit/) | Sizing, spacing, tooltips, theme customization, application shell |

## Getting started

1. Add the packages you need. `Joufflu` is the core (styles & theming);
   `Joufflu.Inputs` and `Joufflu.Navigation` are optional and both build on it:

   ```sh
   dotnet add package Joufflu
   dotnet add package Joufflu.Inputs      # optional: input controls
   dotnet add package Joufflu.Navigation  # optional: navigation & overlays
   ```

2. Merge the control styles in `App.xaml` so the themed styles and the
   design-system keys (`joufflu:Brushes`, `joufflu:Dimensions`,
   `joufflu:Spacing`, `joufflu:ControlProperties`) are available:

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

4. Use the controls as shown in the pages above.

New to the library? The [Tutorial]({{ site.baseurl }}/tutorial/) builds a full
navigable app shell — side menu, pages, modals and toasts — from scratch.

To explore everything interactively, run the **`Joufflu.Samples`** project.
