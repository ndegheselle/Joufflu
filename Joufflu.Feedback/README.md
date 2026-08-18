# Joufflu.Feedback

**Feedback controls for the [Joufflu](https://www.nuget.org/packages/Joufflu) WPF
design system — badges, a spinner, toasts and themed tooltips.**

Small presentational controls that read their colours from the Joufflu design
system, so they re-theme live between Light and Dark along with the rest of your
UI.

[![Joufflu.Feedback on NuGet](https://img.shields.io/nuget/v/Joufflu.Feedback?label=Joufflu.Feedback&logo=nuget)](https://www.nuget.org/packages/Joufflu.Feedback)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

- 🏷️ **`Badge`** — a themed pill in the semantic colours, sized via `Sizing.Size`.
- ⏳ **`Spinner`** — an indeterminate loading indicator.
- 🔔 **Toasts** — stackable, auto-dismissing notifications shown from an injected `IToastService` and rendered by a `ToastContainer` wrapping your app, in any corner.
- 💬 **Tooltip** — the `Tooltip.Content` and `Tooltip.Placement` attached properties for themed tooltips on any element.

This package builds on the core [`Joufflu`](https://www.nuget.org/packages/Joufflu)
package.

## Getting started

1. Add the package (it pulls in `Joufflu`):

   ```sh
   dotnet add package Joufflu.Feedback
   ```

2. Merge the styles in `App.xaml`, after the core `Joufflu` resources:

   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
               <ResourceDictionary Source="pack://application:,,,/Joufflu.Feedback;component/Resources.xaml" />
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

3. Reference the controls from the `Joufflu.Feedback.Controls` namespace:

   ```xml
   xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
   ```

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/>

## Acknowledgments

- [Lucide](https://lucide.dev/) icon font
- [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) for MVVM boilerplate
