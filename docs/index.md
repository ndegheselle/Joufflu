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

{: .note }
> **Using an LLM agent?** Point it at the machine-readable
> [`llms.txt`](https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/llms.txt)
> index — a single file linking every page's raw Markdown source, or
> [`llms-full.txt`](https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/llms-full.txt)
> for every page in one file. Use those raw URLs rather
> than the rendered site, which can return `403` to automated fetchers.

## Highlights

- 🌗 **Live Light / Dark theming** — flip the theme at runtime and every control follows instantly.
- 🎨 **A real design system, built to be customized** — semantic colours, dimensions, sizing and spacing exposed as override-able resource keys; the gallery's **Customize theme** window (palette button in its title bar) edits them live and generates the dictionary.
- 🧩 **Ready-to-use inputs** — numeric, decimal and timespan pickers, searchable and tag combo boxes, file and colour pickers, inline-editable text, dropdowns.
- 🧭 **Navigation & overlays** — a navigation menu, a view-model-first page container, awaitable modal dialogs and a paging selector.
- 🗂️ **A file explorer** — breadcrumb, folder tree and file list sharing a source, with shell-backed file operations and nodes of your own.
- 🧾 **JSON data trees** — fill a value in against a JSON Schema, or build one from scratch.
- 🪟 **Custom-chrome window & natives** — a themed application shell plus restyled built-in WPF controls that match out of the box.
- 📦 **Modular packages** — take just the core styles, or add inputs and navigation only where you need them.

{: .note }
> The gallery's **Native controls** group — WPF's built-in controls restyled to match the
> design system (text boxes, combo boxes, data grid, …) — is mostly undocumented
> here; only natives with named style variants are (see
> [Native controls](natives/index.md)). Otherwise these pages cover the
> custom controls the library adds.

## Sections

| Section | What's inside |
|---|---|
| [Themes](themes/index.md) | The application shell (`ThemedWindow`), `ThemeManager`, the design tokens and the live theme customizer |
| [Native controls](natives/index.md) | Text sizes and muted text, `FontIcon`, the `Card` styles and buttons (solid, soft & outline variants) |
| [Toolkit](toolkit/index.md) | Sizing, spacing, derived dimensions, tooltips, animation, drag and drop, shared width, creating a custom control |
| [Joufflu controls](controls/index.md) | The optional packages: [Feedback](feedback/index.md) (`Badge`, `Spinner`, toasts), [Inputs](inputs/index.md) (numeric, search, combo boxes, pickers, `Dropdown`…), [Navigation](navigation/index.md) (`NavigationMenu`, overlays, `Paging`), [Data](data/index.md) (`DataFill`, `DataEdit`, `DataDisplay`) and [File explorer](file-explorer/index.md) |

## Getting started

1. Add the packages you need. `Joufflu` is the core (styles & theming);
   `Joufflu.Data`, `Joufflu.Feedback`, `Joufflu.FileExplorer`, `Joufflu.Inputs` and
   `Joufflu.Navigation` are optional and all build on it:

   ```sh
   dotnet add package Joufflu
   dotnet add package Joufflu.Data          # optional: JSON data trees (needs Joufflu.Inputs)
   dotnet add package Joufflu.Feedback      # optional: badges, spinner, toasts
   dotnet add package Joufflu.FileExplorer  # optional: file explorer (needs Joufflu.Feedback)
   dotnet add package Joufflu.Inputs        # optional: input controls
   dotnet add package Joufflu.Navigation    # optional: navigation & overlays
   ```

2. Merge the control styles in `App.xaml` so the themed styles and the
   `joufflu:Dimensions` keys are available (the `joufflu:Brushes` come with the
   theme, step 3):

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

New to the library? The [Tutorial](tutorial.md) builds a full
navigable app shell — side menu, pages, modals and toasts — from scratch, and the
[Recipes](recipes.md) give copy-paste screens. Choosing between UI libraries? See
[Joufflu vs other WPF UI libraries](comparison.md).

To explore everything interactively, run the **`Joufflu.Samples`** project.
