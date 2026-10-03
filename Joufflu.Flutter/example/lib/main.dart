import 'package:flutter/material.dart';
import 'package:joufflu/joufflu.dart';

import 'customizer_page.dart';
import 'samples_page.dart';

void main() => runApp(const GalleryApp());

class GalleryApp extends StatefulWidget {
  const GalleryApp({super.key});

  @override
  State<GalleryApp> createState() => _GalleryAppState();
}

class _GalleryAppState extends State<GalleryApp> {
  final _controller = JouffluThemeController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => ListenableBuilder(
    listenable: _controller,
    builder: (context, _) => MaterialApp(
      title: 'Joufflu',
      debugShowCheckedModeBanner: false,
      theme: _controller.lightTheme,
      darkTheme: _controller.darkTheme,
      themeMode: _controller.mode,
      home: GalleryHome(controller: _controller),
    ),
  );
}

class GalleryHome extends StatefulWidget {
  const GalleryHome({super.key, required this.controller});

  final JouffluThemeController controller;

  @override
  State<GalleryHome> createState() => _GalleryHomeState();
}

class _GalleryHomeState extends State<GalleryHome> {
  int _page = 0;

  static const _modes = [ThemeMode.system, ThemeMode.light, ThemeMode.dark];
  static const _modeIcons = {
    ThemeMode.system: Icons.brightness_auto_outlined,
    ThemeMode.light: Icons.light_mode_outlined,
    ThemeMode.dark: Icons.dark_mode_outlined,
  };

  void _cycleMode() {
    final controller = widget.controller;
    controller.mode = _modes[(_modes.indexOf(controller.mode) + 1) % _modes.length];
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: Text(_page == 0 ? 'Joufflu' : 'Customize theme'),
      actions: [
        IconButton(tooltip: 'Theme: ${widget.controller.mode.name}', icon: Icon(_modeIcons[widget.controller.mode]), onPressed: _cycleMode),
      ],
    ),
    body: IndexedStack(
      index: _page,
      children: [
        const SamplesPage(),
        CustomizerPage(controller: widget.controller),
      ],
    ),
    bottomNavigationBar: NavigationBar(
      selectedIndex: _page,
      onDestinationSelected: (page) => setState(() => _page = page),
      destinations: const [
        NavigationDestination(icon: Icon(Icons.widgets_outlined), label: 'Samples'),
        NavigationDestination(icon: Icon(Icons.palette_outlined), label: 'Customize'),
      ],
    ),
  );
}
