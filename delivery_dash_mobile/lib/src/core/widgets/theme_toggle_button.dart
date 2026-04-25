import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/theme_provider.dart';

class ThemeToggleButton extends ConsumerWidget {
  const ThemeToggleButton({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final themeMode = ref.watch(themeModeProvider);

    return IconButton(
      onPressed: () {
        // Simple toggle logic: If Light -> Dark, else -> Light.
        // If System, we assume we want to switch to the opposite of what is currently shown? 
        // Or just explicit Light/Dark cycle. 
        // Let's cycle: System -> Light -> Dark -> System (maybe too complex).
        // Let's just do Light <-> Dark for the button.
        final newMode = themeMode == ThemeMode.light ? ThemeMode.dark : ThemeMode.light;
        ref.read(themeModeProvider.notifier).setTheme(newMode);
      },
      icon: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300),
        transitionBuilder: (child, anim) => RotationTransition(
          turns: child.key == const ValueKey('icon1')
              ? Tween<double>(begin: 1, end: 0.75).animate(anim)
              : Tween<double>(begin: 0.75, end: 1).animate(anim),
          child: ScaleTransition(scale: anim, child: child),
        ),
        child: themeMode == ThemeMode.light
            ? const Icon(Icons.wb_sunny, key: ValueKey('icon1'))
            : const Icon(Icons.nightlight_round, key: ValueKey('icon2')),
      ),
      tooltip: 'Toggle Theme',
    );
  }
}
