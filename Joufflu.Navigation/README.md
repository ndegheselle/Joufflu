# Joufflu.Navigation

**Navigation and modal overlays for [Joufflu](https://www.nuget.org/packages/Joufflu).**

A small, view-model-first navigation layer that follows the Joufflu design system:
a navigation menu, a page container, and awaitable modal dialogs.

[![Joufflu.Navigation on NuGet](https://img.shields.io/nuget/v/Joufflu.Navigation?label=Joufflu.Navigation&logo=nuget)](https://www.nuget.org/packages/Joufflu.Navigation)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

| Piece | Purpose |
|---|---|
| `NavigationMenu` | A themed navigation menu for moving between sections. |
| `NavigationContainer` | A view-model-first page container that hosts the current page. |
| Modal overlays | Awaitable modal dialogs driven by a `Navigator`, so you can `await` a dialog and get its result. |

## Getting started

`Joufflu.Navigation` builds on the core `Joufflu` package. Add it (the core comes
along as a dependency):

```sh
dotnet add package Joufflu.Navigation
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

Then wire up a `Navigator`, host a `NavigationContainer` in your window, and drive
navigation and modal overlays from your view models.

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/>
