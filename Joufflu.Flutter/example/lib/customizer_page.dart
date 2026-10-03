import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:joufflu/joufflu.dart';

import 'color_editor.dart';
import 'presets.dart';
import 'section.dart';

const _colorGroups = {
  'Surface': [JouffluColor.background, JouffluColor.background100, JouffluColor.background200, JouffluColor.border, JouffluColor.border100],
  'Text': [JouffluColor.foreground, JouffluColor.foreground100, JouffluColor.foreground200],
  'Primary': [JouffluColor.primary, JouffluColor.primary100, JouffluColor.primaryContent],
  'Secondary': [JouffluColor.secondary, JouffluColor.secondary100, JouffluColor.secondaryContent],
  'Success': [JouffluColor.success, JouffluColor.success100, JouffluColor.successContent],
  'Info': [JouffluColor.info, JouffluColor.info100, JouffluColor.infoContent],
  'Warning': [JouffluColor.warning, JouffluColor.warning100, JouffluColor.warningContent],
  'Danger': [JouffluColor.danger, JouffluColor.danger100, JouffluColor.dangerContent],
};

/// `primaryContent` → `Primary content`, `border100` → `Border 100`.
String _label(JouffluColor color) {
  final words = color.name.replaceAllMapped(RegExp('([A-Z]|[0-9]+)'), (match) => ' ${match[0]!.toLowerCase()}');
  return words[0].toUpperCase() + words.substring(1);
}

/// Tweaks the palette and dimensions with the whole app updating live, then exports them as Dart.
class CustomizerPage extends StatelessWidget {
  const CustomizerPage({super.key, required this.controller});

  final JouffluThemeController controller;

  @override
  Widget build(BuildContext context) {
    final slot = controller.activeBrightness(context);
    final colors = controller.colorsFor(slot);
    final d = controller.dimensions;
    final spacing = context.jouffluDimensions.spacing;

    void setColor(JouffluColor key, Color color) =>
        controller.setColorsFor(slot, controller.colorsFor(slot).copyWith(values: {key: color}));

    String steps(double Function(JouffluSize size) of) => JouffluSize.values.map((size) => '${size.name} ${_num(of(size))}').join(' · ');

    return ListView(
      padding: EdgeInsets.only(bottom: spacing),
      children: [
        Section(
          title: 'Preset',
          description: 'Editing the ${slot.name} palette, switch the theme from the top bar to edit the other one.',
          children: [
            SizedBox(
              height: 88,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                itemCount: presets.length,
                separatorBuilder: (_, _) => SizedBox(width: spacing / 2),
                itemBuilder: (context, index) => _PresetCard(
                  preset: presets[index],
                  selected: presets[index].colors == colors,
                  onTap: () => controller.setColorsFor(slot, presets[index].colors),
                ),
              ),
            ),
          ],
        ),
        for (final group in _colorGroups.entries)
          Section(
            title: group.key,
            children: [
              Card(
                clipBehavior: Clip.antiAlias,
                child: Column(
                  children: [
                    for (final key in group.value)
                      ListTile(
                        dense: true,
                        leading: _Swatch(colors[key]),
                        title: Text(_label(key)),
                        trailing: Text(toHex(colors[key]), style: const TextStyle(fontFamily: 'monospace')),
                        onTap: () =>
                            showColorEditor(context, label: _label(key), color: colors[key], onChanged: (color) => setColor(key, color)),
                      ),
                  ],
                ),
              ),
            ],
          ),
        Section(
          title: 'Shape',
          children: [
            _DimensionSlider('Corner radius', d.radius, 0, 24, (value) => controller.dimensions = d.copyWith(radius: value)),
            _DimensionSlider('Border thickness', d.thickness, 0, 4, (value) => controller.dimensions = d.copyWith(thickness: value)),
            _DimensionSlider('Spacing', d.spacing, 0, 32, (value) => controller.dimensions = d.copyWith(spacing: value)),
          ],
        ),
        Section(
          title: 'Sizes',
          description: 'Each scale is driven by its md value, the other sizes keep a fixed ratio to it.',
          children: [
            _DimensionSlider(
              'Control height',
              d.height,
              24,
              72,
              (value) => controller.dimensions = d.copyWith(height: value),
              steps: steps(d.heightOf),
            ),
            _DimensionSlider(
              'Font size',
              d.fontSize,
              10,
              24,
              (value) => controller.dimensions = d.copyWith(fontSize: value),
              steps: '${steps(d.fontSizeOf)} · xl ${_num(d.fontSizeXl)}',
            ),
            _DimensionSlider(
              'Padding H',
              d.paddingHorizontal,
              0,
              40,
              (value) => controller.dimensions = d.copyWith(paddingHorizontal: value),
              steps: steps((size) => d.paddingOf(size).left),
            ),
            _DimensionSlider(
              'Padding V',
              d.paddingVertical,
              0,
              24,
              (value) => controller.dimensions = d.copyWith(paddingVertical: value),
              steps: steps((size) => d.paddingOf(size).top),
            ),
          ],
        ),
        Section(
          title: 'Export',
          description: 'Generates the Dart code of both palettes and the dimensions.',
          children: [
            Spaced(
              children: [
                FilledButton.icon(onPressed: () => _showExport(context), icon: const Icon(Icons.code), label: const Text('Export')),
                OutlinedButton.icon(
                  onPressed: () {
                    controller
                      ..light = JouffluColors.light
                      ..dark = JouffluColors.dark
                      ..dimensions = JouffluDimensions.mobile;
                  },
                  icon: const Icon(Icons.restart_alt),
                  label: const Text('Reset'),
                ),
              ],
            ),
          ],
        ),
      ],
    );
  }

  void _showExport(BuildContext context) {
    final code = generateCode(controller);
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (context) {
        final d = context.jouffluDimensions;
        return DraggableScrollableSheet(
          expand: false,
          initialChildSize: 0.7,
          builder: (context, scroll) => ListView(
            controller: scroll,
            padding: EdgeInsets.fromLTRB(d.spacing, 0, d.spacing, d.spacing),
            children: [
              Row(
                children: [
                  Expanded(child: Text('Theme code', style: Theme.of(context).textTheme.titleMedium)),
                  TextButton.icon(
                    onPressed: () {
                      Clipboard.setData(ClipboardData(text: code));
                      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Copied to clipboard')));
                    },
                    icon: const Icon(Icons.copy),
                    label: const Text('Copy'),
                  ),
                ],
              ),
              SizedBox(height: d.spacing / 2),
              SelectableText(code, style: const TextStyle(fontFamily: 'monospace', fontSize: 12)),
            ],
          ),
        );
      },
    );
  }
}

/// Dart code recreating the controller's current theme.
String generateCode(JouffluThemeController controller) {
  String palette(JouffluColors colors) => [
    'JouffluColors({',
    for (final key in JouffluColor.values)
      '    JouffluColor.${key.name}: Color(0x${colors[key].toARGB32().toRadixString(16).padLeft(8, '0').toUpperCase()}),',
    '  })',
  ].join('\n');

  final d = controller.dimensions;
  return '''
final themeController = JouffluThemeController(
  light: ${palette(controller.light)},
  dark: ${palette(controller.dark)},
  dimensions: const JouffluDimensions(
    radius: ${_num(d.radius)},
    thickness: ${_num(d.thickness)},
    spacing: ${_num(d.spacing)},
    height: ${_num(d.height)},
    fontSize: ${_num(d.fontSize)},
    paddingHorizontal: ${_num(d.paddingHorizontal)},
    paddingVertical: ${_num(d.paddingVertical)},
  ),
);
''';
}

String _num(double value) => value == value.roundToDouble() ? value.toInt().toString() : value.toString();

class _Swatch extends StatelessWidget {
  const _Swatch(this.color);

  final Color color;

  @override
  Widget build(BuildContext context) {
    final d = context.jouffluDimensions;
    return Container(
      width: 28,
      height: 28,
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(d.radius / 2),
        border: Border.all(color: context.jouffluColors.border100, width: d.thickness),
      ),
    );
  }
}

/// A preset rendered in its own colours.
class _PresetCard extends StatelessWidget {
  const _PresetCard({required this.preset, required this.selected, required this.onTap});

  final ThemePreset preset;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final c = preset.colors;
    final d = context.jouffluDimensions;
    return Material(
      color: c.background,
      shape: RoundedRectangleBorder(
        borderRadius: d.borderRadius,
        side: BorderSide(color: selected ? context.jouffluColors.foreground : c.border, width: selected ? 2 : d.thickness),
      ),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Container(
          width: 120,
          padding: EdgeInsets.all(d.spacing / 2),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                preset.name,
                style: TextStyle(color: c.foreground, fontWeight: FontWeight.w600),
              ),
              Row(
                spacing: 4,
                children: [
                  for (final color in [c.primary.color, c.secondary.color, c.info.color, c.background200])
                    Container(
                      width: 18,
                      height: 18,
                      decoration: BoxDecoration(color: color, borderRadius: BorderRadius.circular(4)),
                    ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _DimensionSlider extends StatelessWidget {
  const _DimensionSlider(this.label, this.value, this.min, this.max, this.onChanged, {this.steps});

  final String label;
  final double value;
  final double min;
  final double max;
  final ValueChanged<double> onChanged;

  /// Derived sizes shown under the slider.
  final String? steps;

  @override
  Widget build(BuildContext context) {
    final text = Theme.of(context).textTheme;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          children: [
            Expanded(child: Text(label)),
            Text('${_num(value)} px', style: text.bodyMedium?.copyWith(color: context.jouffluColors.foreground100)),
          ],
        ),
        Slider(
          value: value.clamp(min, max),
          min: min,
          max: max,
          // Every dimension is a whole count of logical pixels.
          onChanged: (value) => onChanged(value.roundToDouble()),
        ),
        if (steps != null) Text(steps!, style: text.bodySmall?.copyWith(color: context.jouffluColors.foreground100)),
      ],
    );
  }
}
