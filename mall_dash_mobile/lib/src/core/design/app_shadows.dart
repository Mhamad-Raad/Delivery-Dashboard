import 'dart:ui';
import 'package:flutter/material.dart';
import 'app_colors.dart';

/// Premium shadow and glass effect definitions.
abstract final class AppShadows {
  static const double elevationNone = 0.0;
  static const double elevationXs = 1.0;
  static const double elevationSm = 2.0;
  static const double elevationMd = 4.0;
  static const double elevationLg = 8.0;
  static const double elevationXl = 16.0;

  static const List<BoxShadow> none = [];

  static const List<BoxShadow> cardShadow = [
    BoxShadow(color: Color(0x08000000), blurRadius: 8, offset: Offset(0, 2), spreadRadius: 0),
    BoxShadow(color: Color(0x05000000), blurRadius: 24, offset: Offset(0, 8), spreadRadius: 0),
  ];

  static const List<BoxShadow> raisedShadow = [
    BoxShadow(color: Color(0x0F000000), blurRadius: 16, offset: Offset(0, 4), spreadRadius: 0),
    BoxShadow(color: Color(0x08000000), blurRadius: 32, offset: Offset(0, 12), spreadRadius: 0),
  ];

  static const List<BoxShadow> modalShadow = [
    BoxShadow(color: Color(0x1A000000), blurRadius: 32, offset: Offset(0, 16), spreadRadius: 0),
  ];

  static List<BoxShadow> coloredShadow(Color color, {double blur = 24, double spread = -4}) => [
    BoxShadow(color: color.withAlpha(40), blurRadius: blur, offset: const Offset(0, 8), spreadRadius: spread),
  ];

  static const List<BoxShadow> cardShadowDark = [
    BoxShadow(color: Color(0x30000000), blurRadius: 8, offset: Offset(0, 2)),
  ];

  static const List<BoxShadow> raisedShadowDark = [
    BoxShadow(color: Color(0x40000000), blurRadius: 16, offset: Offset(0, 8)),
  ];

  /// Glassmorphism decoration
  static BoxDecoration glassDecoration({
    Color? color,
    BorderRadius? borderRadius,
    double opacity = 0.15,
    double blur = 20,
    Border? border,
  }) {
    return BoxDecoration(
      color: (color ?? Colors.white).withAlpha((opacity * 255).round()),
      borderRadius: borderRadius,
      border: border ?? Border.all(
        color: Colors.white.withAlpha(30),
        width: 1,
      ),
    );
  }

  /// Premium card decoration
  static BoxDecoration cardDecoration({
    Color? color,
    BorderRadius? borderRadius,
    bool isDark = false,
  }) {
    return BoxDecoration(
      color: color,
      borderRadius: borderRadius,
      boxShadow: isDark ? cardShadowDark : cardShadow,
    );
  }

  static BoxDecoration raisedDecoration({
    Color? color,
    BorderRadius? borderRadius,
    bool isDark = false,
  }) {
    return BoxDecoration(
      color: color,
      borderRadius: borderRadius,
      boxShadow: isDark ? raisedShadowDark : raisedShadow,
    );
  }

  /// Gradient card decoration
  static BoxDecoration gradientCardDecoration({
    LinearGradient? gradient,
    BorderRadius? borderRadius,
    bool isDark = false,
  }) {
    return BoxDecoration(
      gradient: gradient ?? (isDark ? AppColors.cardGradientDark : AppColors.cardGradient),
      borderRadius: borderRadius,
      boxShadow: isDark ? cardShadowDark : cardShadow,
    );
  }
}
