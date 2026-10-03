import 'package:flutter/material.dart';
import 'package:joufflu/joufflu.dart';

import 'section.dart';

/// Material widgets rendered with the Joufflu theme.
class SamplesPage extends StatefulWidget {
  const SamplesPage({super.key});

  @override
  State<SamplesPage> createState() => _SamplesPageState();
}

class _SamplesPageState extends State<SamplesPage> {
  bool _checked = true;
  bool _switched = true;
  int _radio = 0;
  double _slider = 0.4;
  String _segment = 'Day';
  final Set<String> _chips = {'Flutter'};

  String _label(JouffluVariant variant) => variant.name[0].toUpperCase() + variant.name.substring(1);

  @override
  Widget build(BuildContext context) {
    final colors = context.jouffluColors;
    final spacing = context.jouffluDimensions.spacing;
    final text = Theme.of(context).textTheme;

    return ListView(
      padding: EdgeInsets.only(bottom: spacing),
      children: [
        Section(
          title: 'Buttons',
          description: 'Material buttons are themed by default, JouffluButtonStyle gives them a semantic variant.',
          children: [
            Spaced(
              children: [
                FilledButton(onPressed: () {}, child: const Text('Filled')),
                ElevatedButton(onPressed: () {}, child: const Text('Elevated')),
                OutlinedButton(onPressed: () {}, child: const Text('Outlined')),
                TextButton(onPressed: () {}, child: const Text('Text')),
                const FilledButton(onPressed: null, child: Text('Disabled')),
              ],
            ),
            for (final emphasis in JouffluEmphasis.values)
              Spaced(
                children: [
                  for (final variant in JouffluVariant.values)
                    FilledButton(
                      style: JouffluButtonStyle.of(context, variant, emphasis: emphasis),
                      onPressed: () {},
                      child: Text(_label(variant)),
                    ),
                ],
              ),
            Spaced(
              children: [
                for (final size in JouffluSize.values)
                  FilledButton.icon(
                    style: JouffluButtonStyle.of(context, JouffluVariant.primary, size: size),
                    onPressed: () {},
                    icon: const Icon(Icons.add),
                    label: Text(size.name),
                  ),
              ],
            ),
          ],
        ),
        Section(
          title: 'Inputs',
          children: [
            const TextField(
              decoration: InputDecoration(labelText: 'Name', hintText: 'Jane Doe'),
            ),
            const TextField(
              decoration: InputDecoration(prefixIcon: Icon(Icons.search), hintText: 'Search'),
            ),
            const TextField(
              decoration: InputDecoration(labelText: 'Email', errorText: 'Invalid email address'),
            ),
            CheckboxListTile(
              value: _checked,
              onChanged: (value) => setState(() => _checked = value!),
              title: const Text('Checkbox'),
              controlAffinity: ListTileControlAffinity.leading,
            ),
            SwitchListTile(value: _switched, onChanged: (value) => setState(() => _switched = value), title: const Text('Switch')),
            RadioGroup<int>(
              groupValue: _radio,
              onChanged: (value) => setState(() => _radio = value!),
              child: const Column(
                children: [
                  RadioListTile(value: 0, title: Text('First option')),
                  RadioListTile(value: 1, title: Text('Second option')),
                ],
              ),
            ),
            Slider(value: _slider, onChanged: (value) => setState(() => _slider = value)),
            SegmentedButton<String>(
              segments: const [
                ButtonSegment(value: 'Day', label: Text('Day')),
                ButtonSegment(value: 'Week', label: Text('Week')),
                ButtonSegment(value: 'Month', label: Text('Month')),
              ],
              selected: {_segment},
              onSelectionChanged: (value) => setState(() => _segment = value.first),
            ),
            Spaced(
              children: [
                for (final chip in ['Flutter', 'Dart', 'WPF'])
                  FilterChip(
                    label: Text(chip),
                    selected: _chips.contains(chip),
                    onSelected: (selected) => setState(() => selected ? _chips.add(chip) : _chips.remove(chip)),
                  ),
              ],
            ),
          ],
        ),
        Section(
          title: 'Feedback',
          children: [
            const LinearProgressIndicator(value: 0.6),
            const Center(child: CircularProgressIndicator()),
            Spaced(
              children: [
                OutlinedButton(
                  onPressed: () => ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: const Text('Saved'),
                      action: SnackBarAction(label: 'Undo', onPressed: () {}),
                    ),
                  ),
                  child: const Text('Snackbar'),
                ),
                OutlinedButton(
                  onPressed: () => showDialog<void>(
                    context: context,
                    builder: (context) => AlertDialog(
                      title: const Text('Delete file?'),
                      content: const Text('This action cannot be undone.'),
                      actions: [
                        TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancel')),
                        FilledButton(
                          style: JouffluButtonStyle.of(context, JouffluVariant.danger),
                          onPressed: () => Navigator.pop(context),
                          child: const Text('Delete'),
                        ),
                      ],
                    ),
                  ),
                  child: const Text('Dialog'),
                ),
                OutlinedButton(
                  onPressed: () => showModalBottomSheet<void>(
                    context: context,
                    builder: (context) => ListView(
                      shrinkWrap: true,
                      children: const [
                        ListTile(leading: Icon(Icons.share_outlined), title: Text('Share')),
                        ListTile(leading: Icon(Icons.edit_outlined), title: Text('Rename')),
                        ListTile(leading: Icon(Icons.delete_outline), title: Text('Delete')),
                      ],
                    ),
                  ),
                  child: const Text('Bottom sheet'),
                ),
                const Tooltip(message: 'A tooltip', child: Icon(Icons.info_outline)),
                const Badge(label: Text('3'), child: Icon(Icons.notifications_outlined)),
              ],
            ),
          ],
        ),
        Section(
          title: 'Surfaces',
          children: [
            Card(
              child: Padding(
                padding: EdgeInsets.all(spacing),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  spacing: spacing / 2,
                  children: [
                    Text('Card', style: text.titleSmall),
                    Text('Elevated surface on Background100 with a border.', style: text.bodyMedium?.copyWith(color: colors.foreground100)),
                  ],
                ),
              ),
            ),
            Card(
              clipBehavior: Clip.antiAlias,
              child: Column(
                children: [
                  ListTile(leading: const Icon(Icons.inbox_outlined), title: const Text('Inbox'), selected: true, onTap: () {}),
                  ListTile(leading: const Icon(Icons.send_outlined), title: const Text('Sent'), onTap: () {}),
                  ListTile(
                    leading: const Icon(Icons.drafts_outlined),
                    title: const Text('Drafts'),
                    subtitle: const Text('2 unsent'),
                    onTap: () {},
                  ),
                ],
              ),
            ),
          ],
        ),
        Section(
          title: 'Typography',
          children: [
            Text('Headline', style: text.headlineSmall),
            Text('Title large', style: text.titleLarge),
            Text('Title medium', style: text.titleMedium),
            Text('Body text, the default for paragraphs.', style: text.bodyMedium),
            Text('Muted text for secondary information.', style: text.bodyMedium?.copyWith(color: colors.foreground100)),
            Text('Label', style: text.labelSmall),
          ],
        ),
      ],
    );
  }
}
