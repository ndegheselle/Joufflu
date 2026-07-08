# Joufflu — WPF components

A small WPF design system plus a set of reusable inputs.

![preview](./images/preview.gif)

## Projects

The solution builds two shippable libraries and a gallery app:

- **`Joufflu`** — the design system: colors/brushes, dimensions and spacing, restyled default
  WPF controls (dark & light themes), a few custom controls (`Card`, `Badge`, `Spinner`,
  `ThemedWindow`), the Lucide icon font, and an MVVM navigation stack (navigator, modal
  overlays, toasts, side menu).
- **`Joufflu.Inputs`** — reusable inputs built on top of `Joufflu` (see below).
- **`Joufflu.Samples`** — a gallery app that demonstrates both. Start this project to explore.

### Inputs (`Joufflu.Inputs`)

- `FormatTextBox` — accepts a format string of groups (e.g. `"{numeric|max:99|padded} and {decimal|max:99}"`),
  parses each group into an individual value, handles navigation between groups and clear /
  increment / decrement buttons. Used as the base for the numeric pickers.
- `TimeSpanPicker` — selects a `TimeSpan`; a `FormatTextBox` with `"{max:365}d {max:23}h {max:59}m {max:59}s"`.
- `NumericUpDown` — selects an `int`; a `FormatTextBox` with `"{numeric|noGlobalSelection}"`.
- `DecimalUpDown` — selects a `decimal`; a `FormatTextBox` with `"{decimal|noGlobalSelection}"`.
- `ComboBoxSearch` — a `ComboBox` that filters its items; accepts a `FilterMemberPath` that
  behaves like `DisplayMemberPath` for the filter.
- `ComboBoxTags` — multiple-item selection, built on `ComboBoxSearch`.

To use the inputs, reference `Joufflu.Inputs` and merge its resource dictionary:

```xml
<ResourceDictionary Source="pack://application:,,,/Joufflu.Inputs;component/Resources.xaml" />
```

## Getting started

- Requires the **.NET Desktop development** workload and the .NET 10 SDK.
- Open `WpfComponents.sln` and run `Joufflu.Samples`.

## Dependencies

- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) for MVVM primitives.
- [Lucide](https://lucide.dev/) icon font (`Joufflu/Assets/Fonts/lucide.ttf`).

## Experimental

The `Experimental` solution folder holds parked, unsupported work that is not part of the
shippable libraries and is excluded from the default build where it does not compile:
`Joufflu.Data` / `Joufflu.Data.Shared` (a generic-object schema editor), `Joufflu.Layouts`
(`FlexibleGrid`), `Usuel` / `Usuel.History` (helpers and an undo/redo handler), and `Bariole`
(a syntax-highlighting stub).

### Why this exists

To explore building generic, reusable WPF components. There are several ways to do this in
WPF; this repo settles on deriving from a base control and attaching a styled template so the
control ships with a default look while consumers can still restyle it.
