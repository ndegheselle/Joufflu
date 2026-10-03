import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:joufflu/joufflu.dart';

void main() {
  group('JouffluColors', () {
    test('tone maps a variant to its palette entries', () {
      final danger = JouffluColors.light.danger;
      expect(danger.color, JouffluColors.light[JouffluColor.danger]);
      expect(danger.color100, JouffluColors.light[JouffluColor.danger100]);
      expect(danger.content, JouffluColors.light[JouffluColor.dangerContent]);
      expect(danger.soft.a, closeTo(0.14, 0.01));
      expect(JouffluColors.light.secondary.soft.a, closeTo(0.08, 0.01));
    });

    test('brightness follows the background', () {
      expect(JouffluColors.light.brightness, Brightness.light);
      expect(JouffluColors.dark.brightness, Brightness.dark);
    });

    test('copyWith overrides a single colour', () {
      final colors = JouffluColors.light.copyWith(values: {JouffluColor.primary: const Color(0xFF123456)});
      expect(colors.primary.color, const Color(0xFF123456));
      expect(colors.background, JouffluColors.light.background);
      expect(colors == JouffluColors.light, isFalse);
      expect(JouffluColors.light.copyWith() == JouffluColors.light, isTrue);
    });

    test('lerp interpolates every colour', () {
      final middle = JouffluColors.light.lerp(JouffluColors.dark, 0.5);
      expect(middle.background, Color.lerp(JouffluColors.light.background, JouffluColors.dark.background, 0.5));
    });
  });

  group('JouffluDimensions', () {
    // Same ratios as the WPF library: its defaults derive exactly from its md values.
    const desktop = JouffluDimensions(height: 32, fontSize: 13, paddingHorizontal: 12, paddingVertical: 4);

    test('sizes derive from the md value', () {
      expect(JouffluSize.values.map(desktop.heightOf), [24, 28, 32, 40]);
      expect(JouffluSize.values.map(desktop.fontSizeOf), [11, 12, 13, 16]);
      expect(desktop.fontSizeXl, 24);
      expect(desktop.paddingOf(JouffluSize.lg), const EdgeInsets.symmetric(horizontal: 18, vertical: 6));
      expect(desktop.inputPaddingOf(JouffluSize.md), const EdgeInsets.symmetric(horizontal: 6, vertical: 4));
    });
  });

  group('JouffluTheme', () {
    test('exposes the tokens as extensions', () {
      final theme = JouffluTheme.build(JouffluColors.dark);
      expect(theme.extension<JouffluColors>(), JouffluColors.dark);
      expect(theme.extension<JouffluDimensions>(), JouffluDimensions.mobile);
      expect(theme.brightness, Brightness.dark);
      expect(theme.colorScheme.primary, JouffluColors.dark.primary.color);
      expect(theme.scaffoldBackgroundColor, JouffluColors.dark.background);
    });
  });

  group('JouffluThemeController', () {
    test('rebuilds the themes and notifies on change', () {
      final controller = JouffluThemeController();
      var notified = 0;
      controller.addListener(() => notified++);

      controller.setColorsFor(Brightness.light, JouffluColors.dark);
      controller.dimensions = const JouffluDimensions(radius: 0);

      expect(notified, 2);
      expect(controller.lightTheme.brightness, Brightness.dark);
      expect(controller.darkTheme.extension<JouffluDimensions>()!.radius, 0);
    });
  });
}
