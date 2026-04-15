import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:mall_dash_mobile/main.dart';
import 'package:mall_dash_mobile/src/core/providers/shared_preferences_provider.dart';

void main() {
  testWidgets('Theme toggles and persists', (WidgetTester tester) async {
    // Mock SharedPreferences
    SharedPreferences.setMockInitialValues({});
    final prefs = await SharedPreferences.getInstance();

    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          sharedPreferencesProvider.overrideWithValue(prefs),
        ],
        child: const MyApp(),
      ),
    );

    await tester.pumpAndSettle();

    // Initial state: System -> which defaults to Dark Icon because (System == Light) is false
    expect(find.byIcon(Icons.nightlight_round), findsOneWidget);

    // Tap toggle button
    await tester.tap(find.byIcon(Icons.nightlight_round));
    await tester.pumpAndSettle();

    // Should switch to Light mode (logic: if Light -> Dark, else -> Light).
    // System (else) -> Light.
    // If Light, it shows Sunny icon.
    expect(find.byIcon(Icons.wb_sunny), findsOneWidget);

    // Verify persistence
    expect(prefs.getString('theme_mode'), 'light');
    
    // Tap again -> Dark
    await tester.tap(find.byIcon(Icons.wb_sunny));
    await tester.pumpAndSettle();
    
    // If Dark, shows Night icon
    expect(find.byIcon(Icons.nightlight_round), findsOneWidget);
    expect(prefs.getString('theme_mode'), 'dark');
  });
}
