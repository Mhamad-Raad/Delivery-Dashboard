import 'package:flutter/material.dart';

/// Centralized spacing constants for consistent layout.
/// Uses a 4px base grid system.
abstract final class AppSpacing {
  // ==========================================================================
  // BASE SPACING VALUES
  // ==========================================================================
  
  /// 4px - Extra extra small
  static const double xxs = 4.0;
  
  /// 8px - Extra small
  static const double xs = 8.0;
  
  /// 12px - Small
  static const double sm = 12.0;
  
  /// 16px - Medium (default)
  static const double md = 16.0;
  
  /// 20px - Medium large
  static const double ml = 20.0;
  
  /// 24px - Large
  static const double lg = 24.0;
  
  /// 32px - Extra large
  static const double xl = 32.0;
  
  /// 48px - Extra extra large
  static const double xxl = 48.0;
  
  /// 64px - Extra extra extra large
  static const double xxxl = 64.0;
  
  // ==========================================================================
  // EDGE INSETS PRESETS
  // ==========================================================================
  
  /// No padding
  static const EdgeInsets zero = EdgeInsets.zero;
  
  /// All sides: 4px
  static const EdgeInsets allXxs = EdgeInsets.all(xxs);
  
  /// All sides: 8px
  static const EdgeInsets allXs = EdgeInsets.all(xs);
  
  /// All sides: 12px
  static const EdgeInsets allSm = EdgeInsets.all(sm);
  
  /// All sides: 16px (default page padding)
  static const EdgeInsets allMd = EdgeInsets.all(md);
  
  /// All sides: 24px
  static const EdgeInsets allLg = EdgeInsets.all(lg);
  
  /// All sides: 32px
  static const EdgeInsets allXl = EdgeInsets.all(xl);
  
  // ==========================================================================
  // HORIZONTAL PADDING
  // ==========================================================================
  
  static const EdgeInsets horizontalXs = EdgeInsets.symmetric(horizontal: xs);
  static const EdgeInsets horizontalSm = EdgeInsets.symmetric(horizontal: sm);
  static const EdgeInsets horizontalMd = EdgeInsets.symmetric(horizontal: md);
  static const EdgeInsets horizontalLg = EdgeInsets.symmetric(horizontal: lg);
  
  // ==========================================================================
  // VERTICAL PADDING
  // ==========================================================================
  
  static const EdgeInsets verticalXs = EdgeInsets.symmetric(vertical: xs);
  static const EdgeInsets verticalSm = EdgeInsets.symmetric(vertical: sm);
  static const EdgeInsets verticalMd = EdgeInsets.symmetric(vertical: md);
  static const EdgeInsets verticalLg = EdgeInsets.symmetric(vertical: lg);
  
  // ==========================================================================
  // COMMON COMBINATIONS
  // ==========================================================================
  
  /// Card content padding: 16px horizontal, 12px vertical
  static const EdgeInsets cardContent = EdgeInsets.symmetric(
    horizontal: md,
    vertical: sm,
  );
  
  /// List item padding: 16px horizontal, 12px vertical
  static const EdgeInsets listItem = EdgeInsets.symmetric(
    horizontal: md,
    vertical: sm,
  );
  
  /// Button padding: 16px horizontal, 12px vertical
  static const EdgeInsets buttonPadding = EdgeInsets.symmetric(
    horizontal: md,
    vertical: sm,
  );
  
  /// Form field padding: 16px all sides
  static const EdgeInsets formField = EdgeInsets.all(md);
  
  /// Page padding: 16px all sides
  static const EdgeInsets pagePadding = EdgeInsets.all(md);
  
  /// Dialog content padding
  static const EdgeInsets dialogContent = EdgeInsets.fromLTRB(lg, ml, lg, lg);
  
  /// Status badge padding: 12px horizontal, 6px vertical
  static const EdgeInsets statusBadge = EdgeInsets.symmetric(
    horizontal: sm,
    vertical: 6.0,
  );
  
  /// Small status badge padding: 8px horizontal, 4px vertical
  static const EdgeInsets statusBadgeSmall = EdgeInsets.symmetric(
    horizontal: xs,
    vertical: xxs,
  );
  
  // ==========================================================================
  // SIZED BOXES (for use between widgets)
  // ==========================================================================
  
  /// Vertical gap: 4px
  static const SizedBox verticalGapXxs = SizedBox(height: xxs);
  
  /// Vertical gap: 8px
  static const SizedBox verticalGapXs = SizedBox(height: xs);
  
  /// Vertical gap: 12px
  static const SizedBox verticalGapSm = SizedBox(height: sm);
  
  /// Vertical gap: 16px
  static const SizedBox verticalGapMd = SizedBox(height: md);
  
  /// Vertical gap: 24px
  static const SizedBox verticalGapLg = SizedBox(height: lg);
  
  /// Vertical gap: 32px
  static const SizedBox verticalGapXl = SizedBox(height: xl);
  
  /// Horizontal gap: 4px
  static const SizedBox horizontalGapXxs = SizedBox(width: xxs);
  
  /// Horizontal gap: 8px
  static const SizedBox horizontalGapXs = SizedBox(width: xs);
  
  /// Horizontal gap: 12px
  static const SizedBox horizontalGapSm = SizedBox(width: sm);
  
  /// Horizontal gap: 16px
  static const SizedBox horizontalGapMd = SizedBox(width: md);
  
  /// Horizontal gap: 24px
  static const SizedBox horizontalGapLg = SizedBox(width: lg);
  
  // ==========================================================================
  // ICON SIZES
  // ==========================================================================
  
  /// Small icon: 16px
  static const double iconSm = 16.0;
  
  /// Medium icon: 24px (default)
  static const double iconMd = 24.0;
  
  /// Large icon: 32px
  static const double iconLg = 32.0;
  
  /// Extra large icon: 48px
  static const double iconXl = 48.0;
  
  /// Hero icon: 64px
  static const double iconHero = 64.0;
  
  /// Avatar icon: 80px
  static const double iconAvatar = 80.0;
}
