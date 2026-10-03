import 'package:flutter/material.dart';
import 'package:joufflu/joufflu.dart';

/// A titled group of samples.
class Section extends StatelessWidget {
  const Section({super.key, required this.title, this.description, required this.children});

  final String title;
  final String? description;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    final spacing = context.jouffluDimensions.spacing;
    final text = Theme.of(context).textTheme;
    return Padding(
      padding: EdgeInsets.fromLTRB(spacing, spacing, spacing, 0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        spacing: spacing / 2,
        children: [
          Text(title, style: text.titleMedium),
          if (description != null) Text(description!, style: text.bodySmall?.copyWith(color: context.jouffluColors.foreground100)),
          SizedBox(height: spacing / 4),
          ...children,
        ],
      ),
    );
  }
}

/// Wrapped row of items with the theme spacing.
class Spaced extends StatelessWidget {
  const Spaced({super.key, required this.children});

  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    final spacing = context.jouffluDimensions.spacing / 2;
    return Wrap(spacing: spacing, runSpacing: spacing, crossAxisAlignment: WrapCrossAlignment.center, children: children);
  }
}
