import 'package:flutter/material.dart';

import 'joufflu_colors.dart';
import 'joufflu_dimensions.dart';
import 'joufflu_theme.dart';

/// Holds the active palettes and dimensions, rebuilds the app theme live when they change.
///
/// ```dart
/// ListenableBuilder(
///   listenable: controller,
///   builder: (context, _) => MaterialApp(
///     theme: controller.lightTheme,
///     darkTheme: controller.darkTheme,
///     themeMode: controller.mode,
///   ),
/// )
/// ```
class JouffluThemeController extends ChangeNotifier {
  JouffluThemeController({
    this._mode = ThemeMode.system,
    JouffluColors? light,
    JouffluColors? dark,
    this._dimensions = JouffluDimensions.mobile,
  }) : _light = light ?? JouffluColors.light,
       _dark = dark ?? JouffluColors.dark {
    _rebuild();
  }

  ThemeMode _mode;
  JouffluColors _light;
  JouffluColors _dark;
  JouffluDimensions _dimensions;
  late ThemeData _lightTheme;
  late ThemeData _darkTheme;

  ThemeMode get mode => _mode;
  set mode(ThemeMode value) {
    if (value == _mode) return;
    _mode = value;
    notifyListeners();
  }

  /// Palette used when the app is light.
  JouffluColors get light => _light;
  set light(JouffluColors value) => _update(() => _light = value);

  /// Palette used when the app is dark.
  JouffluColors get dark => _dark;
  set dark(JouffluColors value) => _update(() => _dark = value);

  JouffluDimensions get dimensions => _dimensions;
  set dimensions(JouffluDimensions value) => _update(() => _dimensions = value);

  ThemeData get lightTheme => _lightTheme;
  ThemeData get darkTheme => _darkTheme;

  /// Slot in use: the [mode], or the platform brightness when following the system.
  ///
  /// Not the [ThemeData] brightness, a slot can hold a palette of the other brightness.
  Brightness activeBrightness(BuildContext context) => switch (_mode) {
    ThemeMode.light => Brightness.light,
    ThemeMode.dark => Brightness.dark,
    ThemeMode.system => MediaQuery.platformBrightnessOf(context),
  };

  /// Palette of the given [brightness] slot.
  JouffluColors colorsFor(Brightness brightness) => brightness == Brightness.dark ? _dark : _light;

  /// Replaces the palette of the given [brightness] slot.
  void setColorsFor(Brightness brightness, JouffluColors colors) => brightness == Brightness.dark ? dark = colors : light = colors;

  void _update(VoidCallback change) {
    change();
    _rebuild();
    notifyListeners();
  }

  void _rebuild() {
    _lightTheme = JouffluTheme.build(_light, _dimensions);
    _darkTheme = JouffluTheme.build(_dark, _dimensions);
  }
}
