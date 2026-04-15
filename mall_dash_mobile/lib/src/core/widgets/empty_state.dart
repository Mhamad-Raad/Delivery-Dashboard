import 'package:flutter/material.dart';
import '../design/design_system.dart';
import '../theme/custom_theme_extension.dart';

/// Empty state placeholder widget.
class EmptyState extends StatelessWidget {
  const EmptyState({
    super.key,
    required this.icon,
    required this.title,
    this.subtitle,
    this.action,
    this.iconSize,
    this.iconColor,
  });

  /// Icon to display.
  final IconData icon;

  /// Main message.
  final String title;

  /// Secondary message.
  final String? subtitle;

  /// Optional action button.
  final Widget? action;

  /// Icon size. Defaults to [AppSpacing.iconHero].
  final double? iconSize;

  /// Icon color.
  final Color? iconColor;

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
              icon,
              size: iconSize ?? AppSpacing.iconHero,
              color: iconColor ?? appTheme.textTertiary,
            ),
            AppSpacing.verticalGapMd,
            Text(
              title,
              style: theme.textTheme.titleMedium?.copyWith(
                color: appTheme.textSecondary,
              ),
              textAlign: TextAlign.center,
            ),
            if (subtitle != null) ...[
              AppSpacing.verticalGapXs,
              Text(
                subtitle!,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: appTheme.textTertiary,
                ),
                textAlign: TextAlign.center,
              ),
            ],
            if (action != null) ...[
              AppSpacing.verticalGapLg,
              action!,
            ],
          ],
        ),
      ),
    );
  }
}
