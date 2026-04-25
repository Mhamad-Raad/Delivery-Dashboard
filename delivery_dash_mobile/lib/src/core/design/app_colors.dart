import 'package:flutter/material.dart';

/// Premium luxury color palette for Delivery Dash app.
/// Rich, warm tones with gold accents for a high-end feel.
abstract final class AppColors {
  // ==========================================================================
  // PRIMARY COLORS - Deep Royal Indigo
  // ==========================================================================
  
  static const Color primary = Color(0xFF1A1A2E);
  static const Color primaryLight = Color(0xFF16213E);
  static const Color primaryMedium = Color(0xFF0F3460);
  static const Color primaryDark = Color(0xFF0A0A1A);
  
  static const Color primaryContainer = Color(0xFFE8EAF6);
  static const Color primaryContainerDark = Color(0xFF1A237E);
  
  static const Color onPrimary = Colors.white;
  static const Color onPrimaryDark = Color(0xFFE8EAF6);
  
  // ==========================================================================
  // ACCENT COLORS - Premium Gold
  // ==========================================================================
  
  static const Color accent = Color(0xFFE2B93B);
  static const Color accentLight = Color(0xFFF0C75E);
  static const Color accentDark = Color(0xFFC9A227);
  static const Color accentSoft = Color(0xFFFFF8E1);
  
  // ==========================================================================
  // SECONDARY COLORS
  // ==========================================================================
  
  static const Color secondary = Color(0xFF545F71);
  static const Color secondaryDark = Color(0xFFBCC7DB);
  static const Color secondaryContainer = Color(0xFFE8EDF5);
  static const Color secondaryContainerDark = Color(0xFF3C4758);
  
  // ==========================================================================
  // STATUS COLORS
  // ==========================================================================
  
  static const Color success = Color(0xFF10B981);
  static const Color successLight = Color(0xFF34D399);
  static const Color successContainer = Color(0xFFD1FAE5);
  static const Color successContainerDark = Color(0xFF064E3B);
  static const Color onSuccess = Colors.white;
  
  static const Color warning = Color(0xFFF59E0B);
  static const Color warningLight = Color(0xFFFBBF24);
  static const Color warningContainer = Color(0xFFFEF3C7);
  static const Color warningContainerDark = Color(0xFF92400E);
  static const Color onWarning = Colors.white;
  
  static const Color error = Color(0xFFEF4444);
  static const Color errorLight = Color(0xFFF87171);
  static const Color errorContainer = Color(0xFFFEE2E2);
  static const Color errorContainerDark = Color(0xFF991B1B);
  static const Color onError = Colors.white;
  
  static const Color info = Color(0xFF3B82F6);
  static const Color infoLight = Color(0xFF60A5FA);
  static const Color infoContainer = Color(0xFFDBEAFE);
  static const Color infoContainerDark = Color(0xFF1E3A5F);
  static const Color onInfo = Colors.white;
  
  // ==========================================================================
  // ORDER STATUS COLORS
  // ==========================================================================
  
  static const Color orderPending = Color(0xFFF59E0B);
  static const Color orderConfirmed = Color(0xFF3B82F6);
  static const Color orderPreparing = Color(0xFF8B5CF6);
  static const Color orderOutForDelivery = Color(0xFF6366F1);
  static const Color orderDelivered = Color(0xFF10B981);
  static const Color orderCancelled = Color(0xFFEF4444);
  
  // ==========================================================================
  // NEUTRAL COLORS - LIGHT THEME
  // ==========================================================================
  
  static const Color surface = Color(0xFFFFFBFE);
  static const Color surfaceVariant = Color(0xFFF3F0F7);
  static const Color background = Color(0xFFF8F6FA);
  
  static const Color surfaceDark = Color(0xFF1A1A24);
  static const Color surfaceVariantDark = Color(0xFF2A2A3A);
  static const Color backgroundDark = Color(0xFF0F0F18);
  
  static const Color outline = Color(0xFFD1D5DB);
  static const Color outlineVariant = Color(0xFFE5E7EB);
  static const Color outlineDark = Color(0xFF4B5563);
  static const Color outlineVariantDark = Color(0xFF374151);
  
  static const Color textPrimary = Color(0xFF111827);
  static const Color textSecondary = Color(0xFF4B5563);
  static const Color textTertiary = Color(0xFF9CA3AF);
  static const Color textDisabled = Color(0xFFD1D5DB);
  
  static const Color textPrimaryDark = Color(0xFFF9FAFB);
  static const Color textSecondaryDark = Color(0xFFD1D5DB);
  static const Color textTertiaryDark = Color(0xFF9CA3AF);
  static const Color textDisabledDark = Color(0xFF4B5563);
  
  static const Color iconPrimary = Color(0xFF374151);
  static const Color iconSecondary = Color(0xFF6B7280);
  static const Color iconPrimaryDark = Color(0xFFD1D5DB);
  static const Color iconSecondaryDark = Color(0xFF9CA3AF);
  
  static const Color divider = Color(0xFFF3F4F6);
  static const Color dividerDark = Color(0xFF2A2A3A);
  
  // ==========================================================================
  // SPECIAL COLORS
  // ==========================================================================
  
  static const Color shimmerBase = Color(0xFFE5E7EB);
  static const Color shimmerHighlight = Color(0xFFF9FAFB);
  static const Color shimmerBaseDark = Color(0xFF374151);
  static const Color shimmerHighlightDark = Color(0xFF4B5563);
  
  static const Color overlay = Color(0x52000000);
  static const Color overlayLight = Color(0x0F000000);
  
  static const Color cardBackground = Colors.white;
  static const Color cardBackgroundDark = Color(0xFF1E1E2D);
  
  // ==========================================================================
  // GRADIENT DEFINITIONS
  // ==========================================================================
  
  static const LinearGradient primaryGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFF1A1A2E), Color(0xFF16213E), Color(0xFF0F3460)],
  );
  
  static const LinearGradient accentGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFE2B93B), Color(0xFFF0C75E)],
  );
  
  static const LinearGradient buttonGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFF1A1A2E), Color(0xFF0F3460)],
  );
  
  static const LinearGradient splashGradient = LinearGradient(
    begin: Alignment.topCenter,
    end: Alignment.bottomCenter,
    colors: [Color(0xFF0A0A1A), Color(0xFF1A1A2E), Color(0xFF16213E)],
  );
  
  static const LinearGradient cardGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFFFFFFF), Color(0xFFF8F6FA)],
  );
  
  static const LinearGradient cardGradientDark = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFF1E1E2D), Color(0xFF2A2A3A)],
  );
  
  static const LinearGradient successGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFF10B981), Color(0xFF34D399)],
  );
  
  // ==========================================================================
  // HELPER METHODS
  // ==========================================================================
  
  static Color getOrderStatusColor(int status) {
    switch (status) {
      case 1: return orderPending;
      case 2: return orderConfirmed;
      case 3: return orderPreparing;
      case 4: return orderOutForDelivery;
      case 5: return orderDelivered;
      case 6: return orderCancelled;
      default: return textTertiary;
    }
  }
  
  static Color getOrderStatusBackground(int status) {
    return getOrderStatusColor(status).withAlpha(25);
  }
}
