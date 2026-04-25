import 'package:flutter/material.dart';
import '../design/design_system.dart';

/// Standardized card widget with consistent styling.
class AppCard extends StatelessWidget {
  const AppCard({
    super.key,
    required this.child,
    this.onTap,
    this.padding,
    this.margin,
    this.color,
    this.elevation,
    this.borderRadius,
  });

  /// The content of the card.
  final Widget child;

  /// Tap callback.
  final VoidCallback? onTap;

  /// Internal padding. Defaults to [AppSpacing.allMd].
  final EdgeInsetsGeometry? padding;

  /// External margin.
  final EdgeInsetsGeometry? margin;

  /// Background color. Defaults to theme card color.
  final Color? color;

  /// Elevation. Defaults to theme card elevation.
  final double? elevation;

  /// Border radius. Defaults to [AppRadius.radiusMd].
  final BorderRadius? borderRadius;

  @override
  Widget build(BuildContext context) {
    final cardContent = Padding(
      padding: padding ?? AppSpacing.allMd,
      child: child,
    );

    return Card(
      margin: margin ?? EdgeInsets.zero,
      color: color,
      elevation: elevation,
      shape: borderRadius != null
          ? RoundedRectangleBorder(borderRadius: borderRadius!)
          : null,
      child: onTap != null
          ? InkWell(
              onTap: onTap,
              borderRadius: borderRadius ?? AppRadius.radiusMd,
              child: cardContent,
            )
          : cardContent,
    );
  }
}

/// A card with list-style layout (icon + content + trailing).
class AppListCard extends StatelessWidget {
  const AppListCard({
    super.key,
    this.leading,
    required this.title,
    this.subtitle,
    this.trailing,
    this.onTap,
    this.margin,
  });

  final Widget? leading;
  final String title;
  final String? subtitle;
  final Widget? trailing;
  final VoidCallback? onTap;
  final EdgeInsetsGeometry? margin;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return AppCard(
      margin: margin,
      padding: EdgeInsets.zero,
      onTap: onTap,
      child: Padding(
        padding: AppSpacing.allMd,
        child: Row(
          children: [
            if (leading != null) ...[
              leading!,
              AppSpacing.horizontalGapMd,
            ],
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: theme.textTheme.titleSmall,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  if (subtitle != null) ...[
                    AppSpacing.verticalGapXxs,
                    Text(
                      subtitle!,
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ],
              ),
            ),
            if (trailing != null) ...[
              AppSpacing.horizontalGapXs,
              trailing!,
            ],
          ],
        ),
      ),
    );
  }
}
