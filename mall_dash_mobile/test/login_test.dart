import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:delivery_dash_mobile/main.dart';
import 'package:delivery_dash_mobile/src/core/providers/shared_preferences_provider.dart';

void main() {
  testWidgets('LoginPage renders correctly', (WidgetTester tester) async {
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

    // Verify Title
    expect(find.text('Login'), findsOneWidget); // AppBar title
    
    // Verify TextFields
    expect(find.widgetWithText(TextFormField, 'Username'), findsOneWidget);
    expect(find.widgetWithText(TextFormField, 'Password'), findsOneWidget);
    
    // Verify Button
    expect(find.widgetWithText(FilledButton, 'Sign In'), findsOneWidget);
  });
}
