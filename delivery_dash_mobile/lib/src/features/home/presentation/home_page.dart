import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/shimmer_widgets.dart';
import '../../vendor/presentation/vendors_notifier.dart';
import '../../vendor/presentation/vendor_details_page.dart';

class HomePage extends ConsumerStatefulWidget {
  const HomePage({super.key});

  @override
  ConsumerState<HomePage> createState() => _HomePageState();
}

class _HomePageState extends ConsumerState<HomePage> {
  final TextEditingController _searchController = TextEditingController();
  Timer? _debounce;
  String _currentQuery = '';

  @override
  void dispose() {
    _debounce?.cancel();
    _searchController.dispose();
    super.dispose();
  }

  void _onSearchChanged(String value) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 350), () {
      if (!mounted) return;
      setState(() => _currentQuery = value.trim());
      ref.read(vendorsProvider.notifier).setSearch(value);
    });
  }

  void _clearSearch() {
    _searchController.clear();
    _debounce?.cancel();
    setState(() => _currentQuery = '');
    ref.read(vendorsProvider.notifier).setSearch(null);
  }

  @override
  Widget build(BuildContext context) {
    final vendorsState = ref.watch(vendorsProvider);
    final isDark = context.isDarkMode;

    return RefreshIndicator(
      color: AppColors.accent,
      onRefresh: () => ref.read(vendorsProvider.notifier).refresh(),
      child: CustomScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        slivers: [
          // Search bar (always visible)
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
              child: _SearchBar(
                controller: _searchController,
                isDark: isDark,
                onChanged: _onSearchChanged,
                onClear: _searchController.text.isEmpty ? null : _clearSearch,
              ),
            ).animate().fadeIn(duration: 400.ms).slideY(begin: -0.1, end: 0, duration: 400.ms),
          ),

          // Body
          ...vendorsState.when(
            data: (vendors) => _buildDataSlivers(vendors, isDark),
            loading: () => const [
              SliverToBoxAdapter(
                child: Padding(
                  padding: EdgeInsets.only(top: 16),
                  child: ShimmerGrid(itemCount: 6),
                ),
              ),
            ],
            error: (error, stack) => [
              SliverFillRemaining(
                hasScrollBody: false,
                child: ErrorState(
                  message: error.toString(),
                  onRetry: () => ref.read(vendorsProvider.notifier).refresh(),
                ),
              ),
            ],
          ),

          const SliverToBoxAdapter(child: SizedBox(height: 100)),
        ],
      ),
    );
  }

  List<Widget> _buildDataSlivers(List vendors, bool isDark) {
    if (vendors.isEmpty) {
      final searching = _currentQuery.isNotEmpty;
      return [
        SliverFillRemaining(
          hasScrollBody: false,
          child: Center(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 32),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    searching ? Icons.search_off_rounded : Icons.store_mall_directory_outlined,
                    size: 56,
                    color: AppColors.textTertiary,
                  ),
                  AppSpacing.verticalGapMd,
                  Text(
                    searching ? 'No results for "$_currentQuery"' : 'No vendors available',
                    textAlign: TextAlign.center,
                    style: GoogleFonts.inter(
                      fontSize: 16,
                      fontWeight: FontWeight.w700,
                      color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                    ),
                  ),
                  AppSpacing.verticalGapXs,
                  Text(
                    searching
                        ? 'Try a different vendor or food name'
                        : 'Pull down to refresh',
                    textAlign: TextAlign.center,
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      color: AppColors.textTertiary,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ];
    }

    return [
      // Section header
      SliverToBoxAdapter(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                _currentQuery.isEmpty ? 'Popular Vendors' : 'Results',
                style: GoogleFonts.inter(
                  fontSize: 18,
                  fontWeight: FontWeight.w700,
                  letterSpacing: -0.3,
                ),
              ),
              Text(
                '${vendors.length} ${vendors.length == 1 ? 'vendor' : 'vendors'}',
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
    ];
  }
}

class _SearchBar extends StatelessWidget {
  final TextEditingController controller;
  final bool isDark;
  final ValueChanged<String> onChanged;
  final VoidCallback? onClear;

  const _SearchBar({
    required this.controller,
    required this.isDark,
    required this.onChanged,
    required this.onClear,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
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
          Expanded(
            child: TextField(
              controller: controller,
              onChanged: onChanged,
              textInputAction: TextInputAction.search,
              style: GoogleFonts.inter(
                fontSize: 14,
                color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
              ),
              cursorColor: AppColors.primary,
              decoration: InputDecoration(
                isCollapsed: true,
                contentPadding: const EdgeInsets.symmetric(vertical: 14),
                border: InputBorder.none,
                hintText: 'Search food or vendors...',
                hintStyle: GoogleFonts.inter(
                  color: AppColors.textTertiary,
                  fontSize: 14,
                ),
              ),
            ),
          ),
          if (onClear != null)
            GestureDetector(
              onTap: onClear,
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 12),
                child: Icon(Icons.close_rounded,
                    color: AppColors.textTertiary, size: 18),
              ),
            )
          else
            const SizedBox(width: 16),
        ],
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
