import 'dart:ui' show lerpDouble;

import 'package:flutter/material.dart';

/// Size of a control, scales its height, font size and padding.
enum JouffluSize { xs, sm, md, lg }

/// The metrics of the design system, exposed to widgets as a [ThemeExtension].
///
/// Sized values are driven by a single base (the [JouffluSize.md] value), the other sizes keep a
/// fixed ratio to it, same as the WPF theme customizer scales.
@immutable
class JouffluDimensions extends ThemeExtension<JouffluDimensions> {
  const JouffluDimensions({
    this.radius = 8,
    this.thickness = 1,
    this.spacing = 16,
    this.height = 40,
    this.fontSize = 14,
    this.paddingHorizontal = 16,
    this.paddingVertical = 8,
  });

  /// Defaults tuned for touch: larger controls, text and spacing than the desktop library.
  static const mobile = JouffluDimensions();

  final double radius;
  final double thickness;
  final double spacing;

  /// Control height at [JouffluSize.md].
  final double height;

  /// Font size at [JouffluSize.md].
  final double fontSize;

  /// Control padding at [JouffluSize.md].
  final double paddingHorizontal;
  final double paddingVertical;

  static const _heightRatios = {JouffluSize.xs: 0.75, JouffluSize.sm: 0.875, JouffluSize.md: 1.0, JouffluSize.lg: 1.25};
  static const _fontSizeRatios = {JouffluSize.xs: 0.85, JouffluSize.sm: 0.92, JouffluSize.md: 1.0, JouffluSize.lg: 1.23};
  static const _paddingRatios = {JouffluSize.xs: 0.5, JouffluSize.sm: 0.75, JouffluSize.md: 1.0, JouffluSize.lg: 1.5};

  BorderRadius get borderRadius => BorderRadius.circular(radius);

  double heightOf(JouffluSize size) => (height * _heightRatios[size]!).roundToDouble();

  double fontSizeOf(JouffluSize size) => (fontSize * _fontSizeRatios[size]!).roundToDouble();

  /// Font size of titles, above [JouffluSize.lg].
  double get fontSizeXl => (fontSize * 1.85).roundToDouble();

  EdgeInsets paddingOf(JouffluSize size) {
    final ratio = _paddingRatios[size]!;
    return EdgeInsets.symmetric(
      horizontal: (paddingHorizontal * ratio).roundToDouble(),
      vertical: (paddingVertical * ratio).roundToDouble(),
    );
  }

  /// Text inputs read tighter than a button: half the horizontal padding.
  EdgeInsets inputPaddingOf(JouffluSize size) {
    final padding = paddingOf(size);
    return EdgeInsets.symmetric(horizontal: padding.left / 2, vertical: padding.top);
  }

  @override
  JouffluDimensions copyWith({
    double? radius,
    double? thickness,
    double? spacing,
    double? height,
    double? fontSize,
    double? paddingHorizontal,
    double? paddingVertical,
  }) => JouffluDimensions(
    radius: radius ?? this.radius,
    thickness: thickness ?? this.thickness,
    spacing: spacing ?? this.spacing,
    height: height ?? this.height,
    fontSize: fontSize ?? this.fontSize,
    paddingHorizontal: paddingHorizontal ?? this.paddingHorizontal,
    paddingVertical: paddingVertical ?? this.paddingVertical,
  );

  @override
  JouffluDimensions lerp(JouffluDimensions? other, double t) {
    if (other == null) return this;
    return JouffluDimensions(
      radius: lerpDouble(radius, other.radius, t)!,
      thickness: lerpDouble(thickness, other.thickness, t)!,
      spacing: lerpDouble(spacing, other.spacing, t)!,
      height: lerpDouble(height, other.height, t)!,
      fontSize: lerpDouble(fontSize, other.fontSize, t)!,
      paddingHorizontal: lerpDouble(paddingHorizontal, other.paddingHorizontal, t)!,
      paddingVertical: lerpDouble(paddingVertical, other.paddingVertical, t)!,
    );
  }

  @override
  bool operator ==(Object other) =>
      other is JouffluDimensions &&
      other.radius == radius &&
      other.thickness == thickness &&
      other.spacing == spacing &&
      other.height == height &&
      other.fontSize == fontSize &&
      other.paddingHorizontal == paddingHorizontal &&
      other.paddingVertical == paddingVertical;

  @override
  int get hashCode => Object.hash(radius, thickness, spacing, height, fontSize, paddingHorizontal, paddingVertical);
}
