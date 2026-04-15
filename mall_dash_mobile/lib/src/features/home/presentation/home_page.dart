import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/empty_state.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/shimmer_widgets.dart';
import '../../vendor/presentation/vendors_notifier.dart';
import '../../vendor/presentation/vendor_details_page.dart';

class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final vendorsState = ref.watch(vendorsProvider);
    final isDark = context.isDarkMode;

    return vendorsState.when(
      data: (vendors) {
        if (vendors.isEmpty) {
          return const EmptyState(
            icon: Icons.store_mall_directory_outlined,
            title: 'No vendors available',
            subtitle: 'Pull down to refresh',
          );
        }

        return RefreshIndicator(
          color: AppColors.accent,
          onRefresh: () => ref.read(vendorsProvider.notifier).refresh(),
          child: CustomScrollView(
            slivers: [
              // Search bar
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
                  child: Container(
                    height: 48,
                    decoration: BoxDecoration(
                      color: isDark
                          ? AppColors.surfaceVariantDark.withAlpha(120)
                          : AppColors.surfaceVariant.withAlpha(180),
                      borderRadius: AppRadius.radiusMd,
                      border: Border.all(
                        color: isDark ? Colors.white.withAlpha(10) : Colors.black.withAlpha(8),
                      ),
                    ),
                    child: Row(
                      children: [
                        const SizedBox(width: 16),
                        Icon(Icons.search_rounded, color: AppColors.textTertiary, size: 20),
                        const SizedBox(width: 12),
                        Text(
                          'Search vendors...',
                          style: GoogleFonts.inter(
                            color: AppColors.textTertiary,
                            fontSize: 14,
                          ),
                        ),
                      ],
                    ),
                  ),
                ).animate().fadeIn(duration: 400.ms).slideY(begin: -0.1, end: 0, duration: 400.ms),
              ),

              // Section header
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        'Popular Vendors',
                        style: GoogleFonts.inter(
                          fontSize: 18,
                          fontWeight: FontWeight.w700,
                          letterSpacing: -0.3,
                        ),
                      ),
                      Text(
                        '${vendors.length} available',
                        style: GoogleFonts.inter(
                          fontSize: 13,
                          color: AppColors.textTertiary,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ],
                  ),
                ).animate(delay: 100.ms).fadeIn(duration: 400.ms),
              ),

              // Vendor grid
              SliverPadding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                sliver: SliverGrid(
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    childAspectRatio: 0.78,
                    crossAxisSpacing: 12,
                    mainAxisSpacing: 12,
                  ),
                  delegate: SliverChildBuilderDelegate(
                    (context, index) {
                      final vendor = vendors[index];
                      return _VendorCard(vendor: vendor)
                          .animate(delay: Duration(milliseconds: 100 + index * 60))
                          .fadeIn(duration: 400.ms)
                          .slideY(begin: 0.1, end: 0, duration: 400.ms);
                    },
                    childCount: vendors.length,
                  ),
                ),
              ),

              // Bottom spacing for floating nav
              const SliverToBoxAdapter(child: SizedBox(height: 100)),
            ],
          ),
        );
      },
      loading: () => const Padding(
        padding: EdgeInsets.only(top: 16),
        child: ShimmerGrid(itemCount: 6),
      ),
      error: (error, stack) => ErrorState(
        message: error.toString(),
        onRetry: () => ref.read(vendorsProvider.notifier).refresh(),
      ),
    );
  }
}

class _VendorCard extends StatelessWidget {
  final dynamic vendor;
  const _VendorCard({required this.vendor});

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;

    return GestureDetector(
      onTap: () {
        Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => VendorDetailsPage(vendor: vendor)),
        );
      },
      child: Container(
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardBackgroundDark : Colors.white,
          borderRadius: AppRadius.radiusXl,
          boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
          border: Border.all(
            color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
          ),
        ),
        clipBehavior: Clip.antiAlias,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Image
            Expanded(
              flex: 3,
              child: Container(
                decoration: BoxDecoration(
                  color: isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant,
                ),
                child: vendor.profileImageUrl != null
                    ? CachedNetworkImage(
                        imageUrl: vendor.profileImageUrl!,
                        fit: BoxFit.cover,
                        placeholder: (_, __) => const ShimmerCard(),
                        errorWidget: (_, __, ___) => _buildPlaceholderIcon(isDark),
                      )
                    : _buildPlaceholderIcon(isDark),
              ),
            ),
            // Info
            Expanded(
              flex: 2,
              child: Padding(
                padding: const EdgeInsets.all(12),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      vendor.name,
                      style: GoogleFonts.inter(
                        fontSize: 14,
                        fontWeight: FontWeight.w700,
                        letterSpacing: -0.2,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    if (vendor.description != null) ...[
                      const SizedBox(height: 4),
                      Text(
                        vendor.description!,
                        style: GoogleFonts.inter(
                          fontSize: 12,
                          color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                          fontWeight: FontWeight.w400,
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPlaceholderIcon(bool isDark) {
    return Center(
      child: Icon(
        Icons.store_rounded,
        size: 36,
        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
      ),
    );
  }
}
