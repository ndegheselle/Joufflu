import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:joufflu/joufflu.dart';

String toHex(Color color) => '#${color.toARGB32().toRadixString(16).padLeft(8, '0').substring(2).toUpperCase()}';

/// Parses `#RRGGBB` or `#AARRGGBB`, null when invalid.
Color? fromHex(String value) {
  final hex = value.replaceFirst('#', '');
  if (hex.length != 6 && hex.length != 8) return null;
  final parsed = int.tryParse(hex, radix: 16);
  if (parsed == null) return null;
  return Color(hex.length == 6 ? 0xFF000000 | parsed : parsed);
}

/// Opens a bottom sheet editing [color], [onChanged] is called live on every change.
Future<void> showColorEditor(BuildContext context, {required String label, required Color color, required ValueChanged<Color> onChanged}) =>
    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (context) => _ColorEditor(label: label, color: color, onChanged: onChanged),
    );

class _ColorEditor extends StatefulWidget {
  const _ColorEditor({required this.label, required this.color, required this.onChanged});

  final String label;
  final Color color;
  final ValueChanged<Color> onChanged;

  @override
  State<_ColorEditor> createState() => _ColorEditorState();
}

class _ColorEditorState extends State<_ColorEditor> {
  late HSVColor _hsv = HSVColor.fromColor(widget.color);
  late final _hex = TextEditingController(text: toHex(widget.color));

  @override
  void dispose() {
    _hex.dispose();
    super.dispose();
  }

  void _setHsv(HSVColor hsv) {
    setState(() => _hsv = hsv);
    final color = hsv.toColor();
    _hex.text = toHex(color);
    widget.onChanged(color);
  }

  void _setHex(String value) {
    final color = fromHex(value);
    if (color == null) return;
    setState(() => _hsv = HSVColor.fromColor(color));
    widget.onChanged(color);
  }

  @override
  Widget build(BuildContext context) {
    final d = context.jouffluDimensions;
    final colors = context.jouffluColors;
    final color = _hsv.toColor();

    Widget slider(String label, double value, double max, ValueChanged<double> onChanged) => Row(
      children: [
        SizedBox(width: 88, child: Text(label)),
        Expanded(
          child: Slider(value: value, max: max, onChanged: onChanged),
        ),
      ],
    );

    return Padding(
      padding: EdgeInsets.fromLTRB(d.spacing, 0, d.spacing, d.spacing + MediaQuery.viewInsetsOf(context).bottom),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        spacing: d.spacing / 2,
        children: [
          Row(
            spacing: d.spacing,
            children: [
              Container(
                width: d.heightOf(JouffluSize.lg),
                height: d.heightOf(JouffluSize.lg),
                decoration: BoxDecoration(
                  color: color,
                  borderRadius: d.borderRadius,
                  border: Border.all(color: colors.border100, width: d.thickness),
                ),
              ),
              Expanded(child: Text(widget.label, style: Theme.of(context).textTheme.titleMedium)),
              SizedBox(
                width: 120,
                child: TextField(
                  controller: _hex,
                  onChanged: _setHex,
                  inputFormatters: [FilteringTextInputFormatter.allow(RegExp('[#0-9a-fA-F]'))],
                  decoration: const InputDecoration(hintText: '#RRGGBB'),
                ),
              ),
            ],
          ),
          slider('Hue', _hsv.hue, 360, (value) => _setHsv(_hsv.withHue(value))),
          slider('Saturation', _hsv.saturation, 1, (value) => _setHsv(_hsv.withSaturation(value))),
          slider('Brightness', _hsv.value, 1, (value) => _setHsv(_hsv.withValue(value))),
        ],
      ),
    );
  }
}
