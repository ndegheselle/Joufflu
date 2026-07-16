---
title: Home
nav_order: 1
---

# Joufflu WPF Components

WPF inputs and reusable components for .NET, built on a small design system of
themed brushes, dimensions and layout helpers.

These pages mirror the controls shown in the **`Joufflu.Samples`** gallery app —
same titles, descriptions and code snippets. Each control page lists the example
markup you can paste into your own views.

{: .note }
> The gallery's **Natives** group (WPF's built-in controls restyled to match the
> design system — buttons, text boxes, combo boxes, data grid, and so on) is not
> documented here. These pages cover only the custom controls the library adds.

## Sections

| Section | What's inside |
|---|---|
| [Inputs]({{ site.baseurl }}/inputs/) | `NumericUpDown`, `DecimalUpDown`, `TimeSpanPicker`, `FormatTextBox`, `Search`, `ComboBoxSearch`, `ComboBoxTags`, `TextEditable`, `FilePicker`, `ColorPicker` |
| [Navigation]({{ site.baseurl }}/navigation/) | `NavigationMenu`, overlays (modal dialogs) |
| [Custom controls]({{ site.baseurl }}/custom-controls/) | `FontIcon`, `Badge`, `Spinner`, toasts |
| [Toolkit]({{ site.baseurl }}/toolkit/) | Sizing, spacing, theme customization, application shell |

## Getting started

1. Reference the `Joufflu` projects (`Joufflu`, `Joufflu.Inputs`,
   `Joufflu.Navigation`) from your WPF app.
2. Merge the resource dictionaries so the themed styles and the design-system
   keys (`joufflu:Brushes`, `joufflu:Dimensions`, `joufflu:Spacing`,
   `joufflu:ControlProperties`) are available.
3. Use the controls as shown in the pages above.

To explore everything interactively, run the **`Joufflu.Samples`** project.
