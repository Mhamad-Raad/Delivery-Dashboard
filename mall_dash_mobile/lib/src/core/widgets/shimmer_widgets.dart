import 'package:flutter/material.dart';
import 'package:shimmer/shimmer.dart';
import '../design/design_system.dart';
import '../theme/custom_theme_extension.dart';

/// Shimmer loading card placeholder.
class ShimmerCard extends StatelessWidget {
  const ShimmerCard({super.key, this.height = 120, this.width, this.borderRadius});

  final double height;
  final double? width;
  final BorderRadius? borderRadius;

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;
    return Shimmer.fromColors(
      baseColor: isDark ? AppColors.shimmerBaseDark : AppColors.shimmerBase,
      highlightColor: isDark ? AppColors.shimmerHighlightDark : AppColors.shimmerHighlight,
      child: Container(
        height: height,
        width: width ?? double.infinity,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: borderRadius ?? AppRadius.radiusXl,
        ),
      ),
    );
  }
}

/// Shimmer loading list placeholder.
class ShimmerList extends StatelessWidget {
  const ShimmerList({super.key, this.itemCount = 5, this.itemHeight = 80});

  final int itemCount;
  final double itemHeight;

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      physics: const NeverScrollableScrollPhysics(),
      shrinkWrap: true,
      padding: AppSpacing.allMd,
      itemCount: itemCount,
      separatorBuilder: (_, __) => AppSpacing.verticalGapSm,
      itemBuilder: (context, index) => ShimmerCard(height: itemHeight),
    );
  }
}

/// Shimmer grid placeholder.
class ShimmerGrid extends StatelessWidget {
  const ShimmerGrid({super.key, this.itemCount = 4, this.itemHeight = 160});

  final int itemCount;
  final double itemHeight;

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;
    return Shimmer.fromColors(
      baseColor: isDark ? AppColors.shimmerBaseDark : AppColors.shimmerBase,
      highlightColor: isDark ? AppColors.shimmerHighlightDark : AppColors.shimmerHighlight,
      child: GridView.builder(
        physics: const NeverScrollableScrollPhysics(),
        shrinkWrap: true,
        padding: AppSpacing.allMd,
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          childAspectRatio: 0.85,
          crossAxisSpacing: AppSpacing.md,
          mainAxisSpacing: AppSpacing.md,
        ),
        itemCount: itemCount,
        itemBuilder: (context, index) => Container(
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: AppRadius.radiusXl,
          ),
        ),
      ),
    );
  }
}

/// Shimmer circular avatar placeholder.
class ShimmerAvatar extends StatelessWidget {
  const ShimmerAvatar({super.key, this.radius = 40});

  final double radius;

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;
    return Shimmer.fromColors(
      baseColor: isDark ? AppColors.shimmerBaseDark : AppColors.shimmerBase,
      highlightColor: isDark ? AppColors.shimmerHighlightDark : AppColors.shimmerHighlight,
      child: CircleAvatar(radius: radius, backgroundColor: Colors.white),
    );
  }
}
