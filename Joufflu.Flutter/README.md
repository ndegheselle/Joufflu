# Joufflu for Flutter

**Joufflu's design system for Flutter mobile apps.** Same tokens and palettes as the
WPF library, applied to Material 3 so the built-in widgets match out of the box.

> Preview: only the theme is ported for now, no custom widgets yet.

- 🌗 **Live Light / Dark theming**: change the palette or dimensions at runtime and the whole app follows.
- 🎨 **Design tokens** as `ThemeExtension`s: `JouffluColors` (same 26 colours as the WPF keys) and `JouffluDimensions`.
- 🧩 **Themed Material widgets**: buttons, inputs, cards, chips, sheets, dialogs, navigation bar…
- 🔘 **Semantic buttons**: solid, soft or outline, for every variant (primary, secondary, success, info, warning, danger), in 4 sizes.
- 📱 **Tuned for touch**: larger default sizes than the desktop library, 48dp tap targets.

## Getting started

```yaml
dependencies:
  joufflu:
    path: ../Joufflu.Flutter  # not published yet
```

```dart
final controller = JouffluThemeController();

ListenableBuilder(
  listenable: controller,
  builder: (context, _) => MaterialApp(
    theme: controller.lightTheme,
    darkTheme: controller.darkTheme,
    themeMode: controller.mode,
    home: const HomePage(),
  ),
);
```

Without a controller, build a theme directly: `JouffluTheme.build(JouffluColors.light)`.

## Design tokens

```dart
final colors = context.jouffluColors;
final dimensions = context.jouffluDimensions;

Container(
  padding: EdgeInsets.all(dimensions.spacing),
  decoration: BoxDecoration(
    color: colors.background100,
    border: Border.all(color: colors.border, width: dimensions.thickness),
    borderRadius: dimensions.borderRadius,
  ),
  child: Text('Saved', style: TextStyle(color: colors.success.color)),
);
```

- **Colours**: `foreground`, `background`, `border` families plus a `JouffluTone` per
  semantic variant (`color`, `color100`, `content`, derived `soft` / `softStrong` tints).
- **Dimensions**: `radius`, `thickness`, `spacing`, and the `height`, `fontSize`, `padding`
  scales. Each scale is driven by its `md` value, read a size with `heightOf(JouffluSize.sm)`,
  `fontSizeOf`, `paddingOf` or `inputPaddingOf`.

Custom palette, from scratch or from a built-in:

```dart
final brand = JouffluColors.light.copyWith(values: {
  JouffluColor.primary: const Color(0xFF4B6BFB),
  JouffluColor.primary100: const Color(0xFF405BD5),
  JouffluColor.primaryContent: Colors.white,
});
```

## Buttons

Material buttons are themed by default: `FilledButton` is primary, `ElevatedButton`
secondary, `OutlinedButton` the neutral default and `TextButton` ghost. Give any of them a
semantic variant, emphasis and size with `JouffluButtonStyle`:

```dart
FilledButton(
  style: JouffluButtonStyle.of(context, JouffluVariant.danger, emphasis: JouffluEmphasis.soft, size: JouffluSize.sm),
  onPressed: delete,
  child: const Text('Delete'),
);
```

## Example app

`example/` is the gallery: themed samples and a **theme customizer**. Pick a preset
(the WPF gallery's themes), edit any colour or dimension with the whole app updating live,
then export the Dart code of your theme.

```sh
cd example
flutter run
```
