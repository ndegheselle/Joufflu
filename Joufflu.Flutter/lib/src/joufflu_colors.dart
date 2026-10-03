import 'package:flutter/material.dart';

/// Every palette colour of the design system, same names as the WPF `Joufflu.Colors` keys.
enum JouffluColor {
  foreground,
  foreground100,
  foreground200,
  background,
  // Elevated surfaces (cards, sheets, dialogs).
  background100,
  // Selected and hovered surfaces.
  background200,
  border,
  border100,
  primary,
  primary100,
  primaryContent,
  secondary,
  secondary100,
  secondaryContent,
  success,
  success100,
  successContent,
  info,
  info100,
  infoContent,
  warning,
  warning100,
  warningContent,
  danger,
  danger100,
  dangerContent,
}

/// Semantic intent of a coloured element (button, badge, alert…).
enum JouffluVariant { primary, secondary, success, info, warning, danger }

/// The colours of one [JouffluVariant]: the solid [color], its pressed/hover shade [color100]
/// and the [content] drawn on top of it.
@immutable
class JouffluTone {
  const JouffluTone(this.color, this.color100, this.content, {this.softOpacity = 0.14, this.softStrongOpacity = 0.24});

  final Color color;
  final Color color100;
  final Color content;
  final double softOpacity;
  final double softStrongOpacity;

  /// Low-opacity tint of [color], the resting fill of soft elements.
  Color get soft => color.withValues(alpha: softOpacity);

  /// Stronger tint of [color], the pressed fill of soft elements.
  Color get softStrong => color.withValues(alpha: softStrongOpacity);
}

/// The colour palette, exposed to widgets as a [ThemeExtension].
///
/// Read it with `context.jouffluColors`. Build a custom palette by passing every
/// [JouffluColor], or derive one from a built-in with [copyWith].
@immutable
class JouffluColors extends ThemeExtension<JouffluColors> {
  JouffluColors(Map<JouffluColor, Color> values, {this.disabledOpacity = 0.5})
    : assert(values.length == JouffluColor.values.length, 'Every JouffluColor must be set.'),
      values = Map.unmodifiable(values);

  final Map<JouffluColor, Color> values;

  /// Opacity applied to disabled controls.
  final double disabledOpacity;

  Color operator [](JouffluColor color) => values[color]!;

  Color get foreground => this[JouffluColor.foreground];
  Color get foreground100 => this[JouffluColor.foreground100];
  Color get foreground200 => this[JouffluColor.foreground200];
  Color get background => this[JouffluColor.background];
  Color get background100 => this[JouffluColor.background100];
  Color get background200 => this[JouffluColor.background200];
  Color get border => this[JouffluColor.border];
  Color get border100 => this[JouffluColor.border100];

  JouffluTone get primary => tone(JouffluVariant.primary);
  JouffluTone get secondary => tone(JouffluVariant.secondary);
  JouffluTone get success => tone(JouffluVariant.success);
  JouffluTone get info => tone(JouffluVariant.info);
  JouffluTone get warning => tone(JouffluVariant.warning);
  JouffluTone get danger => tone(JouffluVariant.danger);

  /// Colours of a semantic [variant].
  JouffluTone tone(JouffluVariant variant) {
    // Variants are declared in the same order as their 3 palette entries.
    final index = JouffluColor.primary.index + variant.index * 3;
    final colors = JouffluColor.values;
    // Secondary is achromatic, its soft tint is a lighter wash.
    final isSecondary = variant == JouffluVariant.secondary;
    return JouffluTone(
      this[colors[index]],
      this[colors[index + 1]],
      this[colors[index + 2]],
      softOpacity: isSecondary ? 0.08 : 0.14,
      softStrongOpacity: isSecondary ? 0.14 : 0.24,
    );
  }

  /// Brightness inferred from the background, so a dark palette always gets a dark [ThemeData].
  Brightness get brightness => ThemeData.estimateBrightnessForColor(background);

  @override
  JouffluColors copyWith({Map<JouffluColor, Color>? values, double? disabledOpacity}) =>
      JouffluColors({...this.values, ...?values}, disabledOpacity: disabledOpacity ?? this.disabledOpacity);

  @override
  JouffluColors lerp(JouffluColors? other, double t) {
    if (other == null) return this;
    return JouffluColors({
      for (final color in JouffluColor.values) color: Color.lerp(this[color], other[color], t)!,
    }, disabledOpacity: disabledOpacity + (other.disabledOpacity - disabledOpacity) * t);
  }

  @override
  bool operator ==(Object other) =>
      other is JouffluColors &&
      other.disabledOpacity == disabledOpacity &&
      JouffluColor.values.every((color) => other[color] == this[color]);

  @override
  int get hashCode => Object.hash(disabledOpacity, Object.hashAll(JouffluColor.values.map((color) => this[color])));

  static final light = JouffluColors({
    JouffluColor.foreground: const Color(0xFF0A0A0A),
    JouffluColor.foreground100: const Color(0xFF737373),
    JouffluColor.foreground200: const Color(0xFF737373),
    JouffluColor.background: const Color(0xFFF5F5F5),
    JouffluColor.background100: const Color(0xFFFFFFFF),
    JouffluColor.background200: const Color(0xFFE6E6E6),
    JouffluColor.border: const Color(0xFFE6E6E6),
    JouffluColor.border100: const Color(0xFFC5C5C5),
    JouffluColor.primary: const Color(0xFF18181B),
    JouffluColor.primary100: const Color(0xFF27272A),
    JouffluColor.primaryContent: const Color(0xFFFAFAFA),
    JouffluColor.secondary: const Color(0xFFE4E4E7),
    JouffluColor.secondary100: const Color(0xFFD4D4D8),
    JouffluColor.secondaryContent: const Color(0xFF27272A),
    JouffluColor.success: const Color(0xFF00D390),
    JouffluColor.success100: const Color(0xFF00BD81),
    JouffluColor.successContent: const Color(0xFF004C39),
    JouffluColor.info: const Color(0xFF3B82F6),
    JouffluColor.info100: const Color(0xFF1D6FF4),
    JouffluColor.infoContent: const Color(0xFF172554),
    JouffluColor.warning: const Color(0xFFEEAF00),
    JouffluColor.warning100: const Color(0xFFD69D00),
    JouffluColor.warningContent: const Color(0xFF411E03),
    JouffluColor.danger: const Color(0xFFFF627D),
    JouffluColor.danger100: const Color(0xFFFF3E5F),
    JouffluColor.dangerContent: const Color(0xFF4D0218),
  });

  static final dark = JouffluColors({
    JouffluColor.foreground: const Color(0xFFFAFAFA),
    JouffluColor.foreground100: const Color(0xFFA1A1A1),
    JouffluColor.foreground200: const Color(0xFFA1A1A1),
    JouffluColor.background: const Color(0xFF0A0A0A),
    JouffluColor.background100: const Color(0xFF171717),
    JouffluColor.background200: const Color(0xFF282828),
    JouffluColor.border: const Color(0xFF2B2B2B),
    JouffluColor.border100: const Color(0xFF383838),
    JouffluColor.primary: const Color(0xFFFAFAFA),
    JouffluColor.primary100: const Color(0xFFE5E5E5),
    JouffluColor.primaryContent: const Color(0xFF18181B),
    JouffluColor.secondary: const Color(0xFF27272A),
    JouffluColor.secondary100: const Color(0xFF3F3F46),
    JouffluColor.secondaryContent: const Color(0xFFFAFAFA),
    JouffluColor.success: const Color(0xFF00D390),
    JouffluColor.success100: const Color(0xFF00BD80),
    JouffluColor.successContent: const Color(0xFF004C39),
    JouffluColor.info: const Color(0xFF3B82F6),
    JouffluColor.info100: const Color(0xFF2563EB),
    JouffluColor.infoContent: const Color(0xFF172554),
    JouffluColor.warning: const Color(0xFFEAB308),
    JouffluColor.warning100: const Color(0xFFCA8A04),
    JouffluColor.warningContent: const Color(0xFF422006),
    JouffluColor.danger: const Color(0xFFFF627D),
    JouffluColor.danger100: const Color(0xFFF54A6A),
    JouffluColor.dangerContent: const Color(0xFF4D0218),
  });
}
