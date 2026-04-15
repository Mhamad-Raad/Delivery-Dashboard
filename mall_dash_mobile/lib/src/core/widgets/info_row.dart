import 'package:flutter/material.dart';
import '../design/design_system.dart';
import '../theme/custom_theme_extension.dart';

/// A row displaying an icon and text label, commonly used for displaying info.
class InfoRow extends StatelessWidget {
  const InfoRow({
    super.key,
    required this.icon,
    required this.text,
    this.iconSize,
    this.textStyle,
    this.iconColor,
    this.spacing,
  });

  /// Icon to display.
  final IconData icon;

  /// Text to display.
  final String text;

  /// Icon size. Defaults to [AppSpacing.iconSm].
  final double? iconSize;

  /// Text style.
  final TextStyle? textStyle;

  /// Icon color. Defaults to theme secondary text color.
  final Color? iconColor;

  /// Spacing between icon and text.
  final double? spacing;

  @override
  Widget build(BuildContext context) {
    final theme = context.appTheme;

    return Row(
      children: [
        Icon(
          icon,
          size: iconSize ?? AppSpacing.iconSm,
          color: iconColor ?? theme.textTertiary,
        ),
        SizedBox(width: spacing ?? AppSpacing.xs),
        Expanded(
          child: Text(
            text,
            style: textStyle ??
                TextStyle(
                  color: theme.textSecondary,
                  fontSize: AppTypography.sizeBody,
                ),
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}

/// A labeled info row with label on left and value on right.
class LabeledInfoRow extends StatelessWidget {
  const LabeledInfoRow({
    super.key,
    required this.label,
    required this.value,
    this.icon,
  });

  final String label;
  final String value;
  final IconData? icon;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final appTheme = context.appTheme;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: AppSpacing.xs),
      child: Row(
        children: [
          if (icon != null) ...[
            Icon(
              icon,
              size: AppSpacing.iconSm,
              color: appTheme.textTertiary,
            ),
            AppSpacing.horizontalGapXs,
          ],
          Text(
            label,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: appTheme.textSecondary,
            ),
          ),
          const Spacer(),
          Text(
            value,
            style: theme.textTheme.bodyMedium?.copyWith(
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

/// A vertical info block with label above and value below.
class InfoBlock extends StatelessWidget {
  const InfoBlock({
    super.key,
    required this.label,
    required this.value,
    this.icon,
    this.crossAxisAlignment = CrossAxisAlignment.start,
  });

  final String label;
  final String value;
  final IconData? icon;
  final CrossAxisAlignment crossAxisAlignment;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final appTheme = context.appTheme;

    return Column(
      crossAxisAlignment: crossAxisAlignment,
      children: [
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (icon != null) ...[
              Icon(
                icon,
                size: AppSpacing.iconSm,
                color: appTheme.textTertiary,
              ),
              AppSpacing.horizontalGapXxs,
            ],
            Text(
              label,
              style: theme.textTheme.bodySmall?.copyWith(
                color: appTheme.textTertiary,
              ),
            ),
          ],
        ),
        AppSpacing.verticalGapXxs,
        Text(
          value,
          style: theme.textTheme.bodyLarge?.copyWith(
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }
}
