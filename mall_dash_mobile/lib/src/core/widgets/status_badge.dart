import 'package:flutter/material.dart';
import '../design/design_system.dart';
import '../theme/custom_theme_extension.dart';

/// Standard status badge widget for displaying order/ticket status.
/// Replaces all duplicate `_getStatusColor()` implementations.
class StatusBadge extends StatelessWidget {
  const StatusBadge({
    super.key,
    required this.label,
    required this.statusCode,
    this.size = StatusBadgeSize.medium,
  }) : _customColor = null;

  /// Create a status badge with custom color (not based on status code).
  const StatusBadge.custom({
    super.key,
    required this.label,
    required Color color,
    this.size = StatusBadgeSize.medium,
  })  : statusCode = null,
        _customColor = color;

  /// The text to display in the badge.
  final String label;

  /// The status code (1-6) to determine color.
  /// If null, uses [_customColor].
  final int? statusCode;

  /// Badge size variant.
  final StatusBadgeSize size;

  /// Custom color when not using status code.
  final Color? _customColor;

  @override
  Widget build(BuildContext context) {
    final theme = context.appTheme;
    final color = _customColor ?? theme.getOrderStatusColor(statusCode ?? 0);
    final backgroundColor = color.withOpacity(0.15);

    final padding = size == StatusBadgeSize.small
        ? AppSpacing.statusBadgeSmall
        : AppSpacing.statusBadge;

    final fontSize = size == StatusBadgeSize.small
        ? AppTypography.sizeLabelSm
        : AppTypography.sizeLabel;

    return Container(
      padding: padding,
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: AppRadius.radiusMd,
      ),
      child: Text(
        label,
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.bold,
          fontSize: fontSize,
        ),
      ),
    );
  }
}

/// Size variants for status badges.
enum StatusBadgeSize {
  small,
  medium,
}

/// Status badge with a solid background (for dark backgrounds).
class StatusBadgeSolid extends StatelessWidget {
  const StatusBadgeSolid({
    super.key,
    required this.label,
    required this.statusCode,
    this.size = StatusBadgeSize.medium,
  });

  final String label;
  final int statusCode;
  final StatusBadgeSize size;

  @override
  Widget build(BuildContext context) {
    final theme = context.appTheme;
    final color = theme.getOrderStatusColor(statusCode);

    final padding = size == StatusBadgeSize.small
        ? AppSpacing.statusBadgeSmall
        : AppSpacing.statusBadge;

    final fontSize = size == StatusBadgeSize.small
        ? AppTypography.sizeLabelSm
        : AppTypography.sizeLabel;

    return Container(
      padding: padding,
      decoration: BoxDecoration(
        color: color,
        borderRadius: AppRadius.radiusPill,
      ),
      child: Text(
        label,
        style: TextStyle(
          color: Colors.white,
          fontWeight: FontWeight.bold,
          fontSize: fontSize,
        ),
      ),
    );
  }
}
