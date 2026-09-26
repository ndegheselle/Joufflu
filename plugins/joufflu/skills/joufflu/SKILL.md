---
name: joufflu
description: >-
  Usage reference for the Joufflu WPF component library (.NET, net10.0-windows):
  core styles and theming (`Joufflu`), inputs (`Joufflu.Inputs`: NumericUpDown,
  DecimalUpDown, TimeSpanPicker, FormatTextBox, Search, ComboBoxSearch,
  ComboBoxTags, TextEditable, FilePicker, ColorPicker, Dropdown), navigation
  (`Joufflu.Navigation`: NavigationMenu, Navigator, OverlayService/overlays,
  Paging, FullContainer), feedback (`Joufflu.Feedback`: Badge, Spinner, toasts),
  the file explorer (`Joufflu.FileExplorer`), the toolkit attached properties
  (Sizing, Spacing.Gap, Derive, Tooltip, Animate, DropTarget/DragSource),
  ThemedWindow, FontIcon/Lucide icons, named styles (PrimaryButton, Card, H1…) and
  the design tokens (joufflu:Brushes / Colors / Dimensions). Use it whenever a
  project references Joufflu (a `PackageReference`/`ProjectReference` to Joufflu*,
  `xmlns:...="clr-namespace:Joufflu...;assembly=Joufflu..."`, `using Joufflu.*`,
  `pack://application:,,,/Joufflu;component/Resources.xaml`), and check it BEFORE
  hand-writing a WPF control, style, spacing margin, tooltip, modal dialog, toast,
  drag-and-drop handler or hardcoded colour: Joufflu most likely provides it
  already. Do not use it for WPF projects that do not reference Joufflu.
---

# Joufflu

Joufflu is a WPF component library built on a small design system: themed brushes,
dimensions, sizing and spacing helpers. Every control reads its colours through
`DynamicResource`, so the whole UI re-themes live (System / Light / Dark / custom).

Goal of this skill: **reuse Joufflu instead of rewriting**, call it with the right
names and namespaces, and keep new UI on the design system (tokens, named styles,
attached properties) so it follows theme changes.

## 1. Check that Joufflu is available

Before writing Joufflu code, confirm the project references it:

- `.csproj`: `<PackageReference Include="Joufflu" …/>` (or `Joufflu.Inputs`,
  `Joufflu.Navigation`, `Joufflu.Feedback`, `Joufflu.FileExplorer`), or a
  `ProjectReference` to `Joufflu*.csproj`.
- `App.xaml` merges `pack://application:,,,/Joufflu;component/Resources.xaml`.
- `xmlns:…="clr-namespace:Joufflu…;assembly=Joufflu…"` in XAML, `using Joufflu.…;` in C#.

Only use a satellite package's types if **that** package is referenced; offer
`dotnet add package <name>` otherwise. The installed assembly is the source of truth:
never invent a property or method that is not listed here or visible in the package.

## 2. Packages

| Package | Contents | Depends on |
|---|---|---|
| `Joufflu` | Styles of the native WPF controls, `ThemeManager`, `ThemedWindow`, `FontIcon` + Lucide glyphs, toolkit attached properties, converters, design tokens | CommunityToolkit.Mvvm |
| `Joufflu.Inputs` | Input controls and `Dropdown` | Joufflu |
| `Joufflu.Navigation` | `NavigationMenu`, `Navigator`, overlays, `Paging`, `FullContainer` | Joufflu |
| `Joufflu.Feedback` | `Badge`, `Spinner`, toasts, `ToastContainer` | Joufflu |
| `Joufflu.FileExplorer` | `Explorer`, `ExplorerList`, `ExplorerTree`, `ExplorerControlBar`, sources | Joufflu, Joufflu.Feedback |

`Joufflu.Data` (JSON Schema editors) is work in progress and its API is unstable:
do not use it unless the project already does, and then read its source.

## 3. Required wiring (the most common mistake)

1. `App.xaml` merges **only the core** `Resources.xaml`. The satellite packages
   ship their control styles through `Themes/Generic.xaml`, so nothing else is
   needed. Do **not** merge `Themes/Light.xaml` / `Dark.xaml` yourself.

   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <!-- ThemeManager inserts the active Light/Dark palette ahead of this at runtime -->
               <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
               <!-- Your own overrides / theme tweaks go AFTER -->
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

2. Initialize the theme once in `App.OnStartup`, **before** the first window shows
   (and after registering any custom theme):

   ```csharp
   using Joufflu.Themes;
   ThemeManager.Instance.Initialize();
   ```

Without step 1 controls render unstyled; without step 2 every themed brush is missing.

## 4. XML namespaces and C# namespaces

| Prefix | Holds |
|---|---|
| `joufflu` | `Brushes`, `Colors`, `Dimensions` resource keys |
| `toolkit` | `Sizing`, `Spacing`, `Derive`, `Tooltip`, `Animate`, `DropTarget`, `DragSource` |
| `fonts` | `FontIcon`, `LucideFontIcons` |
| `controls` | `ThemedWindow` |
| `themes` | `ThemeManager` |
| `conv` | converters |

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
xmlns:themes="clr-namespace:Joufflu.Themes;assembly=Joufflu"
xmlns:conv="clr-namespace:Joufflu.Converters;assembly=Joufflu"
xmlns:inputs="clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs"
xmlns:format="clr-namespace:Joufflu.Inputs.Controls.Format;assembly=Joufflu.Inputs"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
xmlns:fileExplorer="clr-namespace:Joufflu.FileExplorer.Controls;assembly=Joufflu.FileExplorer"
```

C# namespaces (these are the real ones; some doc snippets are off):

| Type | Namespace |
|---|---|
| `ThemeManager` | `Joufflu.Themes` |
| `Navigator`, `IOverlayService`, `OverlayOptions`, `OverlayViewModel`, `EnumConfirmationType`, `INavigator` | `Joufflu.Navigation` |
| `OverlayService` | `Joufflu.Navigation.Controls` |
| `ToastService`, `IToastService`, `ToastOptions`, `ToastType` | `Joufflu.Feedback` |
| `ToastContainer`, `ToastPosition`, `Badge`, `BadgeVariant`, `Spinner` | `Joufflu.Feedback.Controls` |
| `FileSystemSource`, `FileSystemWatcherSource`, `IExplorerSource` | `Joufflu.FileExplorer.Sources` |

## 5. Rules of thumb for new UI

- **Colours**: never hardcode. Use `{DynamicResource {x:Static joufflu:Brushes.XBrush}}`
  (`Background`, `Background100`, `Background200`, `Border`, `Border100`, `Foreground`,
  `Foreground100`, `Foreground200`, and `Primary|Secondary|Success|Info|Warning|Danger`
  with `X`, `X100`, `XContent`). Always `DynamicResource`, never `StaticResource`, for
  theme values.
- **Spacing**: `toolkit:Spacing.Gap="8"` on the panel instead of margins on each child.
  Page padding: `{DynamicResource {x:Static joufflu:Dimensions.SpacingThickness}}`.
- **Size**: `toolkit:Sizing.Size="xs|sm|md|lg"` (inherited, `md` default); icon-only
  button: `toolkit:Sizing.IsSquare="True"`.
- **Text**: `Style="{StaticResource H1}"`…`H6`, `Lead`, `Muted`, `Small`.
- **Surfaces**: `<Border Style="{StaticResource Card}">`, nested `CardSecondary`.
- **Buttons**: `PrimaryButton`, `SecondaryButton`, `GhostButton`, `SuccessButton`,
  `InfoButton`, `WarningButton`, `DangerButton`, plus `Soft*` and `Outline*` variants.
- **Icons**: `<fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Plus}" />`.
- **Tooltips**: `toolkit:Tooltip.Content="…"` (instant, themed) rather than `ToolTip`.
- **Modals / notifications**: `IOverlayService` and `IToastService`, not `MessageBox`
  or a new `Window`.
- **Drag and drop**: `toolkit:DropTarget.Command` / `toolkit:DragSource.Data`, not
  hand-written `Drop` / `MouseMove` handlers.
- Plain native controls (`TextBox`, `ComboBox`, `DataGrid`, `CheckBox`, `TabControl`,
  `Slider`, `DatePicker`, `ProgressBar`…) are already styled implicitly: don't restyle
  them, and base any local style on the default:
  `BasedOn="{StaticResource {x:Type TextBox}}"`.
- MVVM with CommunityToolkit.Mvvm (`ObservableObject`, `RelayCommand`) is the
  library's own idiom.

## 6. Reference files

Read the one matching the task before writing code:

| File | Covers |
|---|---|
| [references/natives.md](references/natives.md) | Named styles: buttons, cards, typography, menu items; `FontIcon`; styled natives |
| [references/toolkit.md](references/toolkit.md) | `Sizing`, `Spacing`, `Derive`, `Tooltip`, `Animate`, `DropTarget`/`DragSource`, converters, helpers |
| [references/theming.md](references/theming.md) | `ThemeManager`, custom themes, persistence, switcher, every design token |
| [references/inputs.md](references/inputs.md) | All `Joufflu.Inputs` controls and `Dropdown` |
| [references/navigation.md](references/navigation.md) | `ThemedWindow` shell, `NavigationMenu`, `Navigator`, overlays, `Paging`, `FullContainer` |
| [references/feedback.md](references/feedback.md) | `Badge`, `Spinner`, toasts |
| [references/file-explorer.md](references/file-explorer.md) | Explorer controls, sources, custom nodes, context menus |

For details not covered here, the raw Markdown docs are indexed at
<https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/llms.txt> (use the raw
URLs; the rendered site can return 403 to fetchers). The `Joufflu.Samples` project in the
repository is the live gallery of every control.

Related skills in this plugin: `joufflu-new-app` (scaffold a Joufflu app shell),
`joufflu-theme` (create and register a custom theme), `joufflu-restyle` (move an
existing view onto the design system), `joufflu-custom-input` (build an input control
of your own that matches the Joufflu ones).
