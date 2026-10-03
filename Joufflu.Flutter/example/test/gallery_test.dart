import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:joufflu/joufflu.dart';
import 'package:joufflu_example/main.dart';

void main() {
  testWidgets('applying a preset re-themes the app', (tester) async {
    await tester.pumpWidget(const GalleryApp());
    expect(find.text('Buttons'), findsOneWidget);

    await tester.tap(find.text('Customize'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Dracula'));
    await tester.pumpAndSettle();

    final theme = Theme.of(tester.element(find.text('Customize theme')));
    expect(theme.scaffoldBackgroundColor, const Color(0xFF282A36));
    expect(theme.extension<JouffluColors>()!.primary.color, const Color(0xFFFF79C6));
  });
}
