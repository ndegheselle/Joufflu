---
title: Joufflu vs other WPF UI libraries
nav_order: 10
---

# Joufflu vs other WPF UI libraries

Which WPF UI library should you pick for a modern-looking desktop app? This page
says where Joufflu fits and, as importantly, where another library is the better
choice. Check each project's own repository for its current status before deciding:
this page does not track their releases.

## At a glance

| You want | Reach for |
|---|---|
| A neutral, modern look that you can make your own by overriding tokens, with live Light / Dark theming, a themed window, and inputs, navigation, modals and toasts that already match | **Joufflu** |
| A Material Design look | MaterialDesignInXaml |
| A Windows 11 / Fluent look | WPF-UI, ModernWpf, or the Fluent theme built into recent .NET versions of WPF |
| A Metro-style window and controls | MahApps.Metro |
| The widest catalogue of ready-made controls | HandyControl, Extended WPF Toolkit |
| Plain WPF with only a few touch-ups | Hand-written styles |

## What Joufflu is

- **One design system, not a bag of controls.** Colours, corner radius, spacing,
  control heights and font sizes are resource keys (`joufflu:Brushes.*`,
  `joufflu:Dimensions.*`). Every Joufflu control and every restyled native control
  reads them through `DynamicResource`, so a theme change reaches all of them at once.
- **Customizable by design.** Brand colours, corner radius, spacing, control heights
  and font sizes are all tokens: override a few in a dictionary merged after the Joufflu
  resources and every control follows, with no `ControlTemplate` copied or edited.
  Register a whole palette by name (`ThemeManager.Register`) to make it selectable next
  to Light and Dark. The gallery's **Customize theme** page edits the tokens live,
  starts from presets and generates the dictionary to paste into your app
  ([Customize theme](toolkit/customize-theme.md)).
- **Restyled natives.** Buttons, text boxes, combo boxes, data grids and the rest of the
  built-in WPF controls pick up the style from the implicit styles, with no
  `Style=` needed on each one.
- **The pieces an app shell needs.** `ThemedWindow` (custom chrome), `NavigationMenu` and
  a view-model-first `Navigator`, awaitable modal overlays (`await overlays.Confirm(...)`),
  stacking toasts, paging, badges, a spinner, a file explorer and JSON-schema driven
  data trees.
- **Typed, explicit APIs.** Layout and size helpers are attached properties
  (`Spacing.Gap`, `Sizing.Size`, `Derive.Margin`) checked by the XAML compiler, with no
  reflection or string-based conventions.
- **Modular.** Core styles in `Joufflu`; inputs, navigation, feedback, file explorer and
  data are separate NuGet packages you add only when needed.
- **Built to be read by AI agents.** [`llms.txt`](https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/llms.txt),
  [`llms-full.txt`](https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/llms-full.txt)
  and a Claude Code plugin give an agent the exact namespaces, names and wiring.

## When not to pick Joufflu

Be honest about the trade-offs:

- **It is young and has a single maintainer.** Expect API changes between 0.x versions
  (see the changelog), and a smaller community than the long-established libraries.
- **It targets `net10.0-windows` only.** Older .NET Framework and earlier .NET versions
  are not supported.
- **It has its own look.** Tokens let you re-colour and re-proportion it, but it does
  not reproduce Material Design or Windows 11 Fluent visuals. If your product must
  follow one of those design languages, use a library built for it.
- **The control catalogue is focused.** There is no ribbon, docking manager, charting
  or property grid. Combine Joufflu with a dedicated library for those, or write them
  against the same tokens.

## Migrating or mixing

- To move an existing view onto Joufflu, replace hardcoded colours and sizes with the
  tokens and named styles, and delete local styles that fight the implicit ones. The
  `joufflu-restyle` skill of the [Claude Code plugin](https://github.com/ndegheselle/Joufflu/tree/main/plugins/joufflu)
  automates this audit.
- Two libraries that both restyle the native controls will fight over the implicit
  styles. If you mix, keep one of them in charge of the natives and use the other only
  for controls it alone provides.

## Next steps

- [Getting started](index.md#getting-started)
- [Tutorial: a navigable app shell](tutorial.md)
- [Recipes](recipes.md): complete, copy-paste screens
