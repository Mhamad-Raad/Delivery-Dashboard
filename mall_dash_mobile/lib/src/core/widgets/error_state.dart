import 'package:flutter/material.dart';
import '../design/design_system.dart';
import '../theme/custom_theme_extension.dart';

/// Error state widget with retry functionality.
class ErrorState extends StatelessWidget {
  const ErrorState({
    super.key,
    this.icon,
    this.title,
    this.message,
    this.onRetry,
    this.retryLabel,
  });

  /// Icon to display. Defaults to error icon.
  final IconData? icon;

  /// Main error title.
  final String? title;

  /// Error message details.
  final String? message;

  /// Retry callback.
  final VoidCallback? onRetry;

  /// Retry button label.
  final String? retryLabel;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final appTheme = context.appTheme;

    return Center(
      child: Padding(
        padding: AppSpacing.allLg,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              icon ?? Icons.error_outline,
              size: AppSpacing.iconXl,
              color: appTheme.errorColor,
            ),
            AppSpacing.verticalGapMd,
            Text(
              title ?? 'Something went wrong',
              style: theme.textTheme.titleMedium?.copyWith(
                color: appTheme.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
            if (message != null) ...[
              AppSpacing.verticalGapXs,
              Text(
                message!,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: appTheme.textTertiary,
                ),
                textAlign: TextAlign.center,
                maxLines: 3,
                overflow: TextOverflow.ellipsis,
              ),
            ],
            if (onRetry != null) ...[
              AppSpacing.verticalGapLg,
              ElevatedButton.icon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh),
                label: Text(retryLabel ?? 'Retry'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
