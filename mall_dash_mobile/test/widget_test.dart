import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:mall_dash_mobile/main.dart';
import 'package:mall_dash_mobile/src/core/providers/shared_preferences_provider.dart';

void main() {
  testWidgets('App starts and shows title', (WidgetTester tester) async {
    // Mock SharedPreferences
    SharedPreferences.setMockInitialValues({});
    final prefs = await SharedPreferences.getInstance();

    await tester.pumpWidget(
      ProviderScope(overrides: [sharedPreferencesProvider.overrideWithValue(prefs)], child: const MyApp()),
    );

    // Wait for localizations
    await tester.pumpAndSettle();

    // Verify title is present. Note: find.text might find multiple if used in AppBar and Body.
    // In our case, it's in AppBar and Body.
    expect(find.text('Mall Dash'), findsAtLeastNWidgets(1));

    // Verify theme toggle button exists
    // We expect to find the theme toggle button.
    // Logic in ThemeToggleButton:
    // child: themeMode == ThemeMode.light ? sunny : night
    // Default load is ThemeMode.system.
    // So (System == Light) is false -> shows night icon.
    expect(find.byIcon(Icons.nightlight_round), findsOneWidget);
  });
}
