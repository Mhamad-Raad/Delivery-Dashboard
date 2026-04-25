import 'package:flutter/material.dart';
import '../design/app_colors.dart';

/// Extended theme data for Delivery Dash app.
/// Contains semantic colors for status indicators, order statuses, and UI elements.
@immutable
class CustomThemeExtension extends ThemeExtension<CustomThemeExtension> {
  const CustomThemeExtension({
    // Status colors
    required this.successColor,
    required this.successContainerColor,
    required this.warningColor,
    required this.warningContainerColor,
    required this.errorColor,
    required this.errorContainerColor,
    required this.infoColor,
    required this.infoContainerColor,
    // Order status colors
    required this.orderPendingColor,
    required this.orderConfirmedColor,
    required this.orderPreparingColor,
    required this.orderOutForDeliveryColor,
    required this.orderDeliveredColor,
    required this.orderCancelledColor,
    // Surface colors
    required this.cardBackground,
    required this.surfaceVariant,
    // Text colors
    required this.textSecondary,
    required this.textTertiary,
    // Divider
    required this.dividerColor,
  });

  // Status colors
  final Color successColor;
  final Color successContainerColor;
  final Color warningColor;
  final Color warningContainerColor;
  final Color errorColor;
  final Color errorContainerColor;
  final Color infoColor;
  final Color infoContainerColor;

  // Order status colors
  final Color orderPendingColor;
  final Color orderConfirmedColor;
  final Color orderPreparingColor;
  final Color orderOutForDeliveryColor;
  final Color orderDeliveredColor;
  final Color orderCancelledColor;

  // Surface colors
  final Color cardBackground;
  final Color surfaceVariant;

  // Text colors
  final Color textSecondary;
  final Color textTertiary;

  // Divider
  final Color dividerColor;

  /// Get color for order status by status code
  Color getOrderStatusColor(int status) {
    switch (status) {
      case 1:
        return orderPendingColor;
      case 2:
        return orderConfirmedColor;
      case 3:
        return orderPreparingColor;
      case 4:
        return orderOutForDeliveryColor;
      case 5:
        return orderDeliveredColor;
      case 6:
        return orderCancelledColor;
      default:
        return textTertiary;
    }
  }

  /// Get background color for order status (with opacity)
  Color getOrderStatusBackground(int status) {
    return getOrderStatusColor(status).withOpacity(0.15);
  }

  @override
  CustomThemeExtension copyWith({
    Color? successColor,
    Color? successContainerColor,
    Color? warningColor,
    Color? warningContainerColor,
    Color? errorColor,
    Color? errorContainerColor,
    Color? infoColor,
    Color? infoContainerColor,
    Color? orderPendingColor,
    Color? orderConfirmedColor,
    Color? orderPreparingColor,
    Color? orderOutForDeliveryColor,
    Color? orderDeliveredColor,
    Color? orderCancelledColor,
    Color? cardBackground,
    Color? surfaceVariant,
    Color? textSecondary,
    Color? textTertiary,
    Color? dividerColor,
  }) {
    return CustomThemeExtension(
      successColor: successColor ?? this.successColor,
      successContainerColor: successContainerColor ?? this.successContainerColor,
      warningColor: warningColor ?? this.warningColor,
      warningContainerColor: warningContainerColor ?? this.warningContainerColor,
      errorColor: errorColor ?? this.errorColor,
      errorContainerColor: errorContainerColor ?? this.errorContainerColor,
      infoColor: infoColor ?? this.infoColor,
      infoContainerColor: infoContainerColor ?? this.infoContainerColor,
      orderPendingColor: orderPendingColor ?? this.orderPendingColor,
      orderConfirmedColor: orderConfirmedColor ?? this.orderConfirmedColor,
      orderPreparingColor: orderPreparingColor ?? this.orderPreparingColor,
      orderOutForDeliveryColor: orderOutForDeliveryColor ?? this.orderOutForDeliveryColor,
      orderDeliveredColor: orderDeliveredColor ?? this.orderDeliveredColor,
      orderCancelledColor: orderCancelledColor ?? this.orderCancelledColor,
      cardBackground: cardBackground ?? this.cardBackground,
      surfaceVariant: surfaceVariant ?? this.surfaceVariant,
      textSecondary: textSecondary ?? this.textSecondary,
      textTertiary: textTertiary ?? this.textTertiary,
      dividerColor: dividerColor ?? this.dividerColor,
    );
  }

  @override
  CustomThemeExtension lerp(ThemeExtension<CustomThemeExtension>? other, double t) {
    if (other is! CustomThemeExtension) {
      return this;
    }
    return CustomThemeExtension(
      successColor: Color.lerp(successColor, other.successColor, t)!,
      successContainerColor: Color.lerp(successContainerColor, other.successContainerColor, t)!,
      warningColor: Color.lerp(warningColor, other.warningColor, t)!,
      warningContainerColor: Color.lerp(warningContainerColor, other.warningContainerColor, t)!,
      errorColor: Color.lerp(errorColor, other.errorColor, t)!,
      errorContainerColor: Color.lerp(errorContainerColor, other.errorContainerColor, t)!,
      infoColor: Color.lerp(infoColor, other.infoColor, t)!,
      infoContainerColor: Color.lerp(infoContainerColor, other.infoContainerColor, t)!,
      orderPendingColor: Color.lerp(orderPendingColor, other.orderPendingColor, t)!,
      orderConfirmedColor: Color.lerp(orderConfirmedColor, other.orderConfirmedColor, t)!,
      orderPreparingColor: Color.lerp(orderPreparingColor, other.orderPreparingColor, t)!,
      orderOutForDeliveryColor: Color.lerp(orderOutForDeliveryColor, other.orderOutForDeliveryColor, t)!,
      orderDeliveredColor: Color.lerp(orderDeliveredColor, other.orderDeliveredColor, t)!,
      orderCancelledColor: Color.lerp(orderCancelledColor, other.orderCancelledColor, t)!,
      cardBackground: Color.lerp(cardBackground, other.cardBackground, t)!,
      surfaceVariant: Color.lerp(surfaceVariant, other.surfaceVariant, t)!,
      textSecondary: Color.lerp(textSecondary, other.textSecondary, t)!,
      textTertiary: Color.lerp(textTertiary, other.textTertiary, t)!,
      dividerColor: Color.lerp(dividerColor, other.dividerColor, t)!,
    );
  }

  // ==========================================================================
  // LIGHT THEME
  // ==========================================================================
  static const light = CustomThemeExtension(
    // Status colors
    successColor: AppColors.success,
    successContainerColor: AppColors.successContainer,
    warningColor: AppColors.warning,
    warningContainerColor: AppColors.warningContainer,
    errorColor: AppColors.error,
    errorContainerColor: AppColors.errorContainer,
    infoColor: AppColors.info,
    infoContainerColor: AppColors.infoContainer,
    // Order status colors
    orderPendingColor: AppColors.orderPending,
    orderConfirmedColor: AppColors.orderConfirmed,
    orderPreparingColor: AppColors.orderPreparing,
    orderOutForDeliveryColor: AppColors.orderOutForDelivery,
    orderDeliveredColor: AppColors.orderDelivered,
    orderCancelledColor: AppColors.orderCancelled,
    // Surface colors
    cardBackground: AppColors.cardBackground,
    surfaceVariant: AppColors.surfaceVariant,
    // Text colors
    textSecondary: AppColors.textSecondary,
    textTertiary: AppColors.textTertiary,
    // Divider
    dividerColor: AppColors.divider,
  );

  // ==========================================================================
  // DARK THEME
  // ==========================================================================
  static const dark = CustomThemeExtension(
    // Status colors - slightly lighter for dark theme
    successColor: AppColors.successLight,
    successContainerColor: AppColors.successContainerDark,
    warningColor: AppColors.warningLight,
    warningContainerColor: AppColors.warningContainerDark,
    errorColor: AppColors.errorLight,
    errorContainerColor: AppColors.errorContainerDark,
    infoColor: AppColors.infoLight,
    infoContainerColor: AppColors.infoContainerDark,
    // Order status colors (same, they're vivid enough)
    orderPendingColor: AppColors.orderPending,
    orderConfirmedColor: AppColors.orderConfirmed,
    orderPreparingColor: AppColors.orderPreparing,
    orderOutForDeliveryColor: AppColors.orderOutForDelivery,
    orderDeliveredColor: AppColors.orderDelivered,
    orderCancelledColor: AppColors.orderCancelled,
    // Surface colors
    cardBackground: AppColors.cardBackgroundDark,
    surfaceVariant: AppColors.surfaceVariantDark,
    // Text colors
    textSecondary: AppColors.textSecondaryDark,
    textTertiary: AppColors.textTertiaryDark,
    // Divider
    dividerColor: AppColors.dividerDark,
  );
}

/// Extension on BuildContext for easy theme access.
extension CustomThemeContext on BuildContext {
  /// Get the custom theme extension from the current theme.
  CustomThemeExtension get appTheme {
    return Theme.of(this).extension<CustomThemeExtension>() ?? CustomThemeExtension.light;
  }
  
  /// Shortcut to check if current theme is dark.
  bool get isDarkMode {
    return Theme.of(this).brightness == Brightness.dark;
  }
}
