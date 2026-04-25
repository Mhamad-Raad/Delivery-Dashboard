import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/localization_provider.dart';

class LanguageSelector extends ConsumerWidget {
  const LanguageSelector({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // ref.watch(localeProvider) gives us the current overridden locale, or null if system.
    // For the UI, if it's null, we might want to show "System" or the current actual locale.
    // But for selection, we just pick a specific one.

    return PopupMenuButton<Locale>(
      icon: const Icon(Icons.language),
      onSelected: (Locale newLocale) {
        ref.read(localeProvider.notifier).setLocale(newLocale);
      },
      itemBuilder: (context) => [
        const PopupMenuItem(value: Locale('en'), child: Text('English')),
        const PopupMenuItem(value: Locale('ar'), child: Text('العربية')),
        const PopupMenuItem(value: Locale('fa'), child: Text('Kurdî (Sorani)')),
      ],
    );
  }
}
