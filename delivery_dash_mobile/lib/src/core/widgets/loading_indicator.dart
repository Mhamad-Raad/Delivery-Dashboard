import 'package:flutter/material.dart';
import '../design/design_system.dart';

/// Standardized loading indicator.
class LoadingIndicator extends StatelessWidget {
  const LoadingIndicator({
    super.key,
    this.size,
    this.strokeWidth,
    this.color,
    this.message,
  });

  /// Size of the loading indicator.
  final double? size;

  /// Stroke width.
  final double? strokeWidth;

  /// Indicator color.
  final Color? color;

  /// Optional loading message.
  final String? message;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final indicatorSize = size ?? 40.0;

    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          SizedBox(
            width: indicatorSize,
            height: indicatorSize,
            child: CircularProgressIndicator(
              strokeWidth: strokeWidth ?? 3.0,
              color: color,
            ),
          ),
          if (message != null) ...[
            AppSpacing.verticalGapMd,
            Text(
              message!,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ],
      ),
    );
  }
}

/// Small inline loading indicator for buttons.
class LoadingIndicatorSmall extends StatelessWidget {
  const LoadingIndicatorSmall({
    super.key,
    this.size = 20.0,
    this.strokeWidth = 2.0,
    this.color,
  });

  final double size;
  final double strokeWidth;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: size,
      height: size,
      child: CircularProgressIndicator(
        strokeWidth: strokeWidth,
        color: color ?? Colors.white,
      ),
    );
  }
}
