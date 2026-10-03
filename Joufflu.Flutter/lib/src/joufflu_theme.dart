import 'package:flutter/material.dart';

import 'joufflu_colors.dart';
import 'joufflu_dimensions.dart';

/// Builds the Material [ThemeData] of a Joufflu palette, so the built-in widgets match the
/// design system out of the box.
abstract final class JouffluTheme {
  static ThemeData build(JouffluColors colors, [JouffluDimensions dimensions = JouffluDimensions.mobile]) {
    final c = colors;
    final d = dimensions;
    final brightness = c.brightness;
    final shape = RoundedRectangleBorder(borderRadius: d.borderRadius);
    final borderSide = BorderSide(color: c.border, width: d.thickness);
    final largeRadius = Radius.circular(d.radius * 2);

    final scheme = ColorScheme(
      brightness: brightness,
      primary: c.primary.color,
      onPrimary: c.primary.content,
      primaryContainer: Color.alphaBlend(c.primary.soft, c.background),
      onPrimaryContainer: c.foreground,
      secondary: c.secondary.color,
      onSecondary: c.secondary.content,
      secondaryContainer: Color.alphaBlend(c.secondary.softStrong, c.background),
      onSecondaryContainer: c.foreground,
      tertiary: c.info.color,
      onTertiary: c.info.content,
      error: c.danger.color,
      onError: c.danger.content,
      errorContainer: Color.alphaBlend(c.danger.soft, c.background),
      onErrorContainer: c.foreground,
      surface: c.background,
      onSurface: c.foreground,
      onSurfaceVariant: c.foreground100,
      surfaceContainerLowest: c.background,
      surfaceContainerLow: c.background100,
      surfaceContainer: c.background100,
      surfaceContainerHigh: c.background100,
      surfaceContainerHighest: c.background200,
      surfaceTint: Colors.transparent,
      outline: c.border100,
      outlineVariant: c.border,
      inverseSurface: c.foreground,
      onInverseSurface: c.background,
      inversePrimary: c.primary.content,
    );

    final baseText = ThemeData(brightness: brightness).textTheme;
    final text = baseText
        .copyWith(
          headlineSmall: baseText.headlineSmall?.copyWith(fontSize: d.fontSizeXl, fontWeight: FontWeight.w600),
          titleLarge: baseText.titleLarge?.copyWith(fontSize: (d.fontSizeXl * 0.8).roundToDouble(), fontWeight: FontWeight.w600),
          titleMedium: baseText.titleMedium?.copyWith(fontSize: d.fontSizeOf(JouffluSize.lg), fontWeight: FontWeight.w600),
          titleSmall: baseText.titleSmall?.copyWith(fontSize: d.fontSize, fontWeight: FontWeight.w600),
          bodyLarge: baseText.bodyLarge?.copyWith(fontSize: d.fontSizeOf(JouffluSize.lg)),
          bodyMedium: baseText.bodyMedium?.copyWith(fontSize: d.fontSize),
          bodySmall: baseText.bodySmall?.copyWith(fontSize: d.fontSizeOf(JouffluSize.sm)),
          labelLarge: baseText.labelLarge?.copyWith(fontSize: d.fontSize, fontWeight: FontWeight.w500),
          labelMedium: baseText.labelMedium?.copyWith(fontSize: d.fontSizeOf(JouffluSize.sm), fontWeight: FontWeight.w500),
          labelSmall: baseText.labelSmall?.copyWith(fontSize: d.fontSizeOf(JouffluSize.xs), fontWeight: FontWeight.w500),
        )
        .apply(bodyColor: c.foreground, displayColor: c.foreground);

    WidgetStateProperty<Color> selected(Color on, Color off) =>
        WidgetStateProperty.resolveWith((states) => states.contains(WidgetState.selected) ? on : off);

    return ThemeData(
      useMaterial3: true,
      brightness: brightness,
      colorScheme: scheme,
      textTheme: text,
      scaffoldBackgroundColor: c.background,
      canvasColor: c.background,
      dividerColor: c.border,
      splashFactory: InkRipple.splashFactory,
      materialTapTargetSize: MaterialTapTargetSize.padded,
      extensions: [c, d],
      appBarTheme: AppBarThemeData(
        backgroundColor: c.background,
        foregroundColor: c.foreground,
        elevation: 0,
        scrolledUnderElevation: 0,
        surfaceTintColor: Colors.transparent,
        titleTextStyle: text.titleMedium,
        shape: Border(bottom: borderSide),
      ),
      cardTheme: CardThemeData(
        color: c.background100,
        elevation: 0,
        margin: EdgeInsets.zero,
        shape: shape.copyWith(side: borderSide),
      ),
      filledButtonTheme: FilledButtonThemeData(style: JouffluButtonStyle.solidOf(c, d, JouffluVariant.primary)),
      elevatedButtonTheme: ElevatedButtonThemeData(style: JouffluButtonStyle.solidOf(c, d, JouffluVariant.secondary)),
      outlinedButtonTheme: OutlinedButtonThemeData(style: JouffluButtonStyle.neutralOf(c, d)),
      textButtonTheme: TextButtonThemeData(style: JouffluButtonStyle.ghostOf(c, d)),
      iconButtonTheme: IconButtonThemeData(style: IconButton.styleFrom(foregroundColor: c.foreground)),
      floatingActionButtonTheme: FloatingActionButtonThemeData(
        backgroundColor: c.primary.color,
        foregroundColor: c.primary.content,
        elevation: 2,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.all(largeRadius)),
      ),
      segmentedButtonTheme: SegmentedButtonThemeData(
        style: ButtonStyle(
          backgroundColor: selected(c.background200, c.background),
          foregroundColor: WidgetStatePropertyAll(c.foreground),
          side: WidgetStatePropertyAll(borderSide),
          shape: WidgetStatePropertyAll(shape),
          minimumSize: WidgetStatePropertyAll(Size.fromHeight(d.height)),
          textStyle: WidgetStatePropertyAll(text.labelLarge),
        ),
      ),
      inputDecorationTheme: InputDecorationThemeData(
        filled: true,
        fillColor: c.background100,
        isDense: true,
        contentPadding: d.inputPaddingOf(JouffluSize.md).copyWith(top: d.paddingVertical * 1.5, bottom: d.paddingVertical * 1.5),
        hintStyle: TextStyle(color: c.foreground200),
        labelStyle: TextStyle(color: c.foreground100),
        helperStyle: TextStyle(color: c.foreground100),
        prefixIconColor: c.foreground100,
        suffixIconColor: c.foreground100,
        border: OutlineInputBorder(
          borderRadius: d.borderRadius,
          borderSide: BorderSide(color: c.border100, width: d.thickness),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: d.borderRadius,
          borderSide: BorderSide(color: c.border100, width: d.thickness),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: d.borderRadius,
          borderSide: BorderSide(color: c.foreground100, width: d.thickness + 1),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: d.borderRadius,
          borderSide: BorderSide(color: c.danger.color, width: d.thickness),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: d.borderRadius,
          borderSide: BorderSide(color: c.danger.color, width: d.thickness + 1),
        ),
        disabledBorder: OutlineInputBorder(
          borderRadius: d.borderRadius,
          borderSide: BorderSide(color: c.border, width: d.thickness),
        ),
      ),
      textSelectionTheme: TextSelectionThemeData(
        cursorColor: c.foreground,
        selectionColor: c.info.softStrong,
        selectionHandleColor: c.info.color,
      ),
      checkboxTheme: CheckboxThemeData(
        fillColor: selected(c.primary.color, Colors.transparent),
        checkColor: WidgetStatePropertyAll(c.primary.content),
        side: BorderSide(color: c.border100, width: d.thickness + 1),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(d.radius / 2)),
      ),
      radioTheme: RadioThemeData(fillColor: selected(c.primary.color, c.border100)),
      switchTheme: SwitchThemeData(
        thumbColor: selected(c.primary.content, c.foreground100),
        trackColor: selected(c.primary.color, c.background200),
        trackOutlineColor: selected(Colors.transparent, c.border100),
      ),
      sliderTheme: SliderThemeData(
        activeTrackColor: c.primary.color,
        inactiveTrackColor: c.background200,
        thumbColor: c.primary.color,
        overlayColor: c.primary.soft,
        valueIndicatorColor: c.primary.color,
        valueIndicatorTextStyle: TextStyle(color: c.primary.content),
      ),
      progressIndicatorTheme: ProgressIndicatorThemeData(
        color: c.primary.color,
        linearTrackColor: c.background200,
        circularTrackColor: Colors.transparent,
        borderRadius: d.borderRadius,
      ),
      chipTheme: ChipThemeData(
        backgroundColor: c.background,
        selectedColor: c.background200,
        checkmarkColor: c.foreground,
        labelStyle: text.labelMedium,
        side: borderSide,
        shape: shape,
        padding: d.paddingOf(JouffluSize.xs),
      ),
      badgeTheme: BadgeThemeData(backgroundColor: c.danger.color, textColor: c.danger.content),
      dividerTheme: DividerThemeData(color: c.border, thickness: d.thickness, space: d.spacing),
      listTileTheme: ListTileThemeData(
        iconColor: c.foreground100,
        textColor: c.foreground,
        selectedColor: c.foreground,
        selectedTileColor: c.background200,
        contentPadding: EdgeInsets.symmetric(horizontal: d.spacing),
        shape: shape,
      ),
      tabBarTheme: TabBarThemeData(
        labelColor: c.foreground,
        unselectedLabelColor: c.foreground100,
        indicatorColor: c.primary.color,
        dividerColor: c.border,
        labelStyle: text.labelLarge,
        unselectedLabelStyle: text.labelLarge,
      ),
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: c.background100,
        surfaceTintColor: Colors.transparent,
        indicatorColor: c.background200,
        indicatorShape: shape,
        elevation: 0,
        iconTheme: WidgetStateProperty.resolveWith(
          (states) => IconThemeData(color: states.contains(WidgetState.selected) ? c.foreground : c.foreground100),
        ),
        labelTextStyle: WidgetStateProperty.resolveWith(
          (states) => text.labelMedium?.copyWith(color: states.contains(WidgetState.selected) ? c.foreground : c.foreground100),
        ),
      ),
      bottomSheetTheme: BottomSheetThemeData(
        backgroundColor: c.background100,
        surfaceTintColor: Colors.transparent,
        dragHandleColor: c.border100,
        showDragHandle: true,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: largeRadius)),
      ),
      dialogTheme: DialogThemeData(
        backgroundColor: c.background100,
        surfaceTintColor: Colors.transparent,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.all(largeRadius), side: borderSide),
        titleTextStyle: text.titleMedium,
        contentTextStyle: text.bodyMedium?.copyWith(color: c.foreground100),
      ),
      popupMenuTheme: PopupMenuThemeData(
        color: c.background100,
        surfaceTintColor: Colors.transparent,
        shape: shape.copyWith(side: borderSide),
        textStyle: text.bodyMedium,
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: c.foreground,
        contentTextStyle: text.bodyMedium?.copyWith(color: c.background),
        actionTextColor: c.background,
        behavior: SnackBarBehavior.floating,
        shape: shape,
      ),
      tooltipTheme: TooltipThemeData(
        decoration: BoxDecoration(color: c.foreground, borderRadius: d.borderRadius),
        textStyle: text.bodySmall?.copyWith(color: c.background),
        padding: d.paddingOf(JouffluSize.xs),
      ),
    );
  }
}

/// Emphasis of a semantic button.
enum JouffluEmphasis {
  /// Filled with the semantic colour.
  solid,

  /// Tinted with the semantic colour at low opacity.
  soft,

  /// Coloured border and text, transparent fill.
  outline,
}

/// Button styles of the design system, to pass to any Material button's `style`.
///
/// ```dart
/// FilledButton(style: JouffluButtonStyle.of(context, JouffluVariant.danger, emphasis: JouffluEmphasis.soft), ...)
/// ```
abstract final class JouffluButtonStyle {
  /// A semantic button.
  static ButtonStyle of(
    BuildContext context,
    JouffluVariant variant, {
    JouffluEmphasis emphasis = JouffluEmphasis.solid,
    JouffluSize size = JouffluSize.md,
  }) {
    final c = context.jouffluColors;
    final d = context.jouffluDimensions;
    return switch (emphasis) {
      JouffluEmphasis.solid => solidOf(c, d, variant, size: size),
      JouffluEmphasis.soft => softOf(c, d, variant, size: size),
      JouffluEmphasis.outline => outlineOf(c, d, variant, size: size),
    };
  }

  /// The default button: background, border and foreground. Default style of [OutlinedButton].
  static ButtonStyle neutral(BuildContext context, {JouffluSize size = JouffluSize.md}) =>
      neutralOf(context.jouffluColors, context.jouffluDimensions, size: size);

  /// Transparent until pressed. Default style of [TextButton].
  static ButtonStyle ghost(BuildContext context, {JouffluSize size = JouffluSize.md}) =>
      ghostOf(context.jouffluColors, context.jouffluDimensions, size: size);

  static ButtonStyle solidOf(JouffluColors c, JouffluDimensions d, JouffluVariant variant, {JouffluSize size = JouffluSize.md}) {
    final tone = c.tone(variant);
    return _style(c, d, size, background: tone.color, pressed: tone.color100, foreground: tone.content);
  }

  static ButtonStyle softOf(JouffluColors c, JouffluDimensions d, JouffluVariant variant, {JouffluSize size = JouffluSize.md}) {
    final tone = c.tone(variant);
    return _style(c, d, size, background: tone.soft, pressed: tone.softStrong, foreground: _tinted(tone, variant));
  }

  static ButtonStyle outlineOf(JouffluColors c, JouffluDimensions d, JouffluVariant variant, {JouffluSize size = JouffluSize.md}) {
    final tone = c.tone(variant);
    return _style(
      c,
      d,
      size,
      background: Colors.transparent,
      pressed: tone.soft,
      foreground: _tinted(tone, variant),
      border: variant == JouffluVariant.secondary ? tone.color100 : tone.color,
    );
  }

  static ButtonStyle neutralOf(JouffluColors c, JouffluDimensions d, {JouffluSize size = JouffluSize.md}) =>
      _style(c, d, size, background: c.background, pressed: c.background200, foreground: c.foreground, border: c.border);

  static ButtonStyle ghostOf(JouffluColors c, JouffluDimensions d, {JouffluSize size = JouffluSize.md}) =>
      _style(c, d, size, background: Colors.transparent, pressed: c.background200, foreground: c.foreground);

  // Secondary is achromatic, its tint is too pale to be read as text.
  static Color _tinted(JouffluTone tone, JouffluVariant variant) => variant == JouffluVariant.secondary ? tone.content : tone.color;

  static ButtonStyle _style(
    JouffluColors c,
    JouffluDimensions d,
    JouffluSize size, {
    required Color background,
    required Color pressed,
    required Color foreground,
    Color? border,
  }) {
    Color disabled(Color color) => color.withValues(alpha: color.a * c.disabledOpacity);
    final height = d.heightOf(size);

    return ButtonStyle(
      backgroundColor: WidgetStateProperty.resolveWith((states) {
        if (states.contains(WidgetState.disabled)) return disabled(background);
        if (states.contains(WidgetState.pressed) || states.contains(WidgetState.hovered)) return pressed;
        return background;
      }),
      foregroundColor: WidgetStateProperty.resolveWith(
        (states) => states.contains(WidgetState.disabled) ? disabled(foreground) : foreground,
      ),
      iconColor: WidgetStateProperty.resolveWith((states) => states.contains(WidgetState.disabled) ? disabled(foreground) : foreground),
      // The pressed state is shown by the background, no ink on top of it.
      overlayColor: const WidgetStatePropertyAll(Colors.transparent),
      side: border == null
          ? null
          : WidgetStateProperty.resolveWith(
              (states) => BorderSide(color: states.contains(WidgetState.disabled) ? disabled(border) : border, width: d.thickness),
            ),
      shape: WidgetStatePropertyAll(RoundedRectangleBorder(borderRadius: d.borderRadius)),
      elevation: const WidgetStatePropertyAll(0),
      shadowColor: const WidgetStatePropertyAll(Colors.transparent),
      minimumSize: WidgetStatePropertyAll(Size(height, height)),
      padding: WidgetStatePropertyAll(d.paddingOf(size)),
      iconSize: WidgetStatePropertyAll((d.fontSizeOf(size) * 1.25).roundToDouble()),
      textStyle: WidgetStatePropertyAll(TextStyle(fontSize: d.fontSizeOf(size), fontWeight: FontWeight.w500)),
      visualDensity: VisualDensity.standard,
    );
  }
}

/// Access to the Joufflu tokens of the current theme.
extension JouffluContext on BuildContext {
  JouffluColors get jouffluColors => Theme.of(this).extension<JouffluColors>() ?? JouffluColors.light;

  JouffluDimensions get jouffluDimensions => Theme.of(this).extension<JouffluDimensions>() ?? JouffluDimensions.mobile;
}
