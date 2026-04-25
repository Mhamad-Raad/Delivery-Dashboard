import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/loading_indicator.dart';
import '../../vendor/data/vendor_model.dart';
import '../../product/data/product_repository.dart';
import '../../product/data/product_model.dart';
import '../../category/data/category_repository.dart';
import '../../category/data/category_model.dart';
import '../../order/presentation/cart_notifier.dart';
import '../../order/presentation/cart_page.dart';

class VendorDetailsPage extends ConsumerStatefulWidget {
  final Vendor vendor;

  const VendorDetailsPage({super.key, required this.vendor});

  @override
  ConsumerState<VendorDetailsPage> createState() => _VendorDetailsPageState();
}

class _VendorDetailsPageState extends ConsumerState<VendorDetailsPage> {
  int? _selectedCategoryId;
  late Future<List<Product>> _productsFuture;
  late Future<List<Category>> _categoriesFuture;

  @override
  void initState() {
    super.initState();
    _categoriesFuture =
        ref.read(categoryRepositoryProvider).getByVendor(widget.vendor.id);
    _productsFuture = _loadProducts();
  }

  Future<List<Product>> _loadProducts() {
    return ref.read(productRepositoryProvider).getProducts(
          vendorId: widget.vendor.id,
          categoryId: _selectedCategoryId,
        );
  }

  void _onCategorySelected(int? categoryId) {
    if (_selectedCategoryId == categoryId) return;
    setState(() {
      _selectedCategoryId = categoryId;
      _productsFuture = _loadProducts();
    });
  }

  void _refresh() {
    setState(() {
      _categoriesFuture =
          ref.read(categoryRepositoryProvider).getByVendor(widget.vendor.id);
      _productsFuture = _loadProducts();
    });
  }

  @override
  Widget build(BuildContext context) {
    final appTheme = context.appTheme;
    final isDark = context.isDarkMode;
    final cart = ref.watch(cartProvider);

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        leading: Padding(
          padding: const EdgeInsets.only(left: 12),
          child: Center(
            child: GestureDetector(
              onTap: () => Navigator.pop(context),
              child: Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: Colors.black.withAlpha(30),
                  borderRadius: AppRadius.radiusPill,
                  border: Border.all(color: Colors.white.withAlpha(30)),
                ),
                child: ClipRRect(
                  borderRadius: AppRadius.radiusPill,
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                    child: const Icon(Icons.arrow_back_rounded,
                        size: 20, color: Colors.white),
                  ),
                ),
              ),
            ),
          ),
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 12),
            child: Center(
              child: GestureDetector(
                onTap: _refresh,
                child: Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: Colors.black.withAlpha(30),
                    borderRadius: AppRadius.radiusPill,
                    border: Border.all(color: Colors.white.withAlpha(30)),
                  ),
                  child: ClipRRect(
                    borderRadius: AppRadius.radiusPill,
                    child: BackdropFilter(
                      filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                      child: const Icon(Icons.refresh_rounded,
                          size: 20, color: Colors.white),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
      body: FutureBuilder<List<Product>>(
        future: _productsFuture,
        builder: (context, productSnapshot) {
          if (productSnapshot.connectionState == ConnectionState.waiting &&
              !productSnapshot.hasData) {
            return const LoadingIndicator();
          }
          if (productSnapshot.hasError) {
            return ErrorState(
              title: 'Error loading products',
              message: productSnapshot.error.toString(),
              onRetry: _refresh,
            );
          }
          final products = productSnapshot.data ?? const <Product>[];
          return _VendorContent(
            vendor: widget.vendor,
            products: products,
            isDark: isDark,
            appTheme: appTheme,
            categoriesFuture: _categoriesFuture,
            selectedCategoryId: _selectedCategoryId,
            onCategorySelected: _onCategorySelected,
          );
        },
      ),
      floatingActionButton: cart.isNotEmpty
          ? _CartFab(cart: cart, isDark: isDark)
              .animate()
              .scale(
                begin: const Offset(0, 0),
                end: const Offset(1, 1),
                duration: 400.ms,
                curve: Curves.elasticOut,
              )
          : null,
    );
  }
}

//  Cart FAB

class _CartFab extends StatelessWidget {
  final Cart cart;
  final bool isDark;
  const _CartFab({required this.cart, required this.isDark});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => const CartPage()),
      ),
      child: Container(
        height: 60,
        padding: const EdgeInsets.symmetric(horizontal: 20),
        decoration: BoxDecoration(
          gradient: AppColors.primaryGradient,
          borderRadius: AppRadius.radiusPill,
          boxShadow: [
            BoxShadow(
              color: AppColors.primary.withAlpha(80),
              blurRadius: 20,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 32,
              height: 32,
              decoration: BoxDecoration(
                color: Colors.white.withAlpha(30),
                borderRadius: AppRadius.radiusPill,
              ),
              child: Center(
                child: Text(
                  '${cart.totalItems}',
                  style: GoogleFonts.inter(
                    fontSize: 14,
                    fontWeight: FontWeight.w800,
                    color: Colors.white,
                  ),
                ),
              ),
            ),
            const SizedBox(width: 10),
            const Icon(Icons.shopping_bag_rounded, color: Colors.white, size: 22),
            const SizedBox(width: 6),
            Text(
              'View Cart',
              style: GoogleFonts.inter(
                fontSize: 15,
                fontWeight: FontWeight.w700,
                color: Colors.white,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

//  Vendor Content (scrollable body)

class _VendorContent extends StatelessWidget {
  final Vendor vendor;
  final List<Product> products;
  final bool isDark;
  final CustomThemeExtension appTheme;
  final Future<List<Category>> categoriesFuture;
  final int? selectedCategoryId;
  final ValueChanged<int?> onCategorySelected;

  const _VendorContent({
    required this.vendor,
    required this.products,
    required this.isDark,
    required this.appTheme,
    required this.categoriesFuture,
    required this.selectedCategoryId,
    required this.onCategorySelected,
  });

  @override
  Widget build(BuildContext context) {
    return CustomScrollView(
      physics: const BouncingScrollPhysics(),
      slivers: [
        //  Hero Header
        SliverToBoxAdapter(child: _buildHeroHeader(context)),
        //  Vendor Info Bar
        SliverToBoxAdapter(
          child: _buildInfoBar(context)
              .animate()
              .fadeIn(duration: 500.ms, delay: 200.ms)
              .slideY(begin: 0.1, end: 0),
        ),
        //  Category Filter Chips
        SliverToBoxAdapter(
          child: _CategoryChipStrip(
            categoriesFuture: categoriesFuture,
            selectedCategoryId: selectedCategoryId,
            onSelected: onCategorySelected,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
        //  Section Title
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 12, 20, 4),
            child: Text(
              'Products',
              style: GoogleFonts.inter(
                fontSize: 20,
                fontWeight: FontWeight.w800,
                letterSpacing: -0.5,
                color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
              ),
            ),
          ).animate().fadeIn(duration: 500.ms, delay: 300.ms),
        ),
        if (products.isEmpty)
          SliverFillRemaining(
            child: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    width: 80,
                    height: 80,
                    decoration: BoxDecoration(
                      color: appTheme.surfaceVariant,
                      borderRadius: AppRadius.radiusXxl,
                    ),
                    child: Icon(Icons.inventory_2_outlined,
                        size: 40, color: appTheme.textTertiary),
                  ),
                  AppSpacing.verticalGapMd,
                  Text(
                    selectedCategoryId == null
                        ? 'No products available'
                        : 'No products in this category',
                    style: GoogleFonts.inter(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: appTheme.textSecondary,
                    ),
                  ),
                  AppSpacing.verticalGapXs,
                  Text(
                    selectedCategoryId == null
                        ? 'Check back later for new items'
                        : 'Try a different category',
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      color: appTheme.textTertiary,
                    ),
                  ),
                ],
              ),
            ),
          )
        else
          //  Product Grid
          SliverPadding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 120),
            sliver: SliverGrid(
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                childAspectRatio: 0.62,
                crossAxisSpacing: 12,
                mainAxisSpacing: 14,
              ),
              delegate: SliverChildBuilderDelegate(
                (context, index) {
                  final product = products[index];
                  return _ProductCard(
                    product: product,
                    isDark: isDark,
                    appTheme: appTheme,
                  )
                      .animate()
                      .fadeIn(
                        duration: 500.ms,
                        delay: Duration(milliseconds: 350 + (index * 60)),
                      )
                      .slideY(begin: 0.15, end: 0);
                },
                childCount: products.length,
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildHeroHeader(BuildContext context) {
    return SizedBox(
      height: 260,
      child: Stack(
        fit: StackFit.expand,
        children: [
          // Background image
          if (vendor.profileImageUrl != null)
            CachedNetworkImage(
              imageUrl: vendor.profileImageUrl!,
              fit: BoxFit.cover,
              placeholder: (_, __) => Container(
                color: isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant,
              ),
              errorWidget: (_, __, ___) => Container(
                decoration: BoxDecoration(
                  gradient: AppColors.primaryGradient,
                ),
              ),
            )
          else
            Container(
              decoration: const BoxDecoration(gradient: AppColors.primaryGradient),
            ),
          // Gradient overlay
          Container(
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
                colors: [
                  Colors.black.withAlpha(60),
                  Colors.black.withAlpha(30),
                  Colors.black.withAlpha(160),
                ],
                stops: const [0.0, 0.4, 1.0],
              ),
            ),
          ),
          // Vendor name overlay at bottom
          Positioned(
            bottom: 0,
            left: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 18),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (vendor.type != null)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      margin: const EdgeInsets.only(bottom: 8),
                      decoration: BoxDecoration(
                        color: AppColors.accent.withAlpha(200),
                        borderRadius: AppRadius.radiusPill,
                      ),
                      child: Text(
                        vendor.type!,
                        style: GoogleFonts.inter(
                          fontSize: 11,
                          fontWeight: FontWeight.w700,
                          color: AppColors.primary,
                          letterSpacing: 0.5,
                        ),
                      ),
                    ),
                  Text(
                    vendor.name,
                    style: GoogleFonts.inter(
                      fontSize: 26,
                      fontWeight: FontWeight.w800,
                      color: Colors.white,
                      letterSpacing: -0.5,
                      height: 1.15,
                    ),
                  ),
                ],
              ),
            ).animate().fadeIn(duration: 600.ms).slideY(begin: 0.15, end: 0),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoBar(BuildContext context) {
    return Container(
      margin: const EdgeInsets.fromLTRB(16, 16, 16, 0),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : AppColors.cardBackground,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
        ),
      ),
      child: Row(
        children: [
          if (vendor.description != null)
            Expanded(
              child: Text(
                vendor.description!,
                style: GoogleFonts.inter(
                  fontSize: 13,
                  color: appTheme.textSecondary,
                  height: 1.4,
                ),
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            )
          else
            const Spacer(),
          if (vendor.openingTime != null && vendor.closeTime != null) ...[
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: AppColors.success.withAlpha(20),
                borderRadius: AppRadius.radiusPill,
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.schedule_rounded,
                      size: 14, color: AppColors.success),
                  const SizedBox(width: 5),
                  Text(
                    '${vendor.openingTime} - ${vendor.closeTime}',
                    style: GoogleFonts.inter(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                      color: AppColors.success,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}

//  Category Chip Strip

class _CategoryChipStrip extends StatelessWidget {
  final Future<List<Category>> categoriesFuture;
  final int? selectedCategoryId;
  final ValueChanged<int?> onSelected;
  final bool isDark;
  final CustomThemeExtension appTheme;

  const _CategoryChipStrip({
    required this.categoriesFuture,
    required this.selectedCategoryId,
    required this.onSelected,
    required this.isDark,
    required this.appTheme,
  });

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<Category>>(
      future: categoriesFuture,
      builder: (context, snapshot) {
        if (!snapshot.hasData || snapshot.data!.isEmpty) {
          return const SizedBox.shrink();
        }
        final categories = snapshot.data!;
        return Padding(
          padding: const EdgeInsets.only(top: 16),
          child: SizedBox(
            height: 40,
            child: ListView.separated(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16),
              itemCount: categories.length + 1,
              separatorBuilder: (_, __) => const SizedBox(width: 8),
              itemBuilder: (context, index) {
                if (index == 0) {
                  return _Chip(
                    label: 'All',
                    selected: selectedCategoryId == null,
                    onTap: () => onSelected(null),
                    isDark: isDark,
                    appTheme: appTheme,
                  );
                }
                final category = categories[index - 1];
                return _Chip(
                  label: category.name,
                  selected: selectedCategoryId == category.id,
                  onTap: () => onSelected(category.id),
                  isDark: isDark,
                  appTheme: appTheme,
                );
              },
            ),
          ),
        ).animate().fadeIn(duration: 500.ms, delay: 250.ms);
      },
    );
  }
}

class _Chip extends StatelessWidget {
  final String label;
  final bool selected;
  final VoidCallback onTap;
  final bool isDark;
  final CustomThemeExtension appTheme;

  const _Chip({
    required this.label,
    required this.selected,
    required this.onTap,
    required this.isDark,
    required this.appTheme,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: selected
              ? AppColors.primary
              : (isDark
                  ? AppColors.surfaceVariantDark.withAlpha(120)
                  : AppColors.surfaceVariant.withAlpha(180)),
          borderRadius: AppRadius.radiusPill,
          border: Border.all(
            color: selected
                ? AppColors.primary
                : (isDark
                    ? Colors.white.withAlpha(10)
                    : Colors.black.withAlpha(8)),
          ),
        ),
        child: Center(
          child: Text(
            label,
            style: GoogleFonts.inter(
              fontSize: 13,
              fontWeight: FontWeight.w700,
              color: selected
                  ? Colors.white
                  : (isDark
                      ? AppColors.textPrimaryDark
                      : AppColors.textPrimary),
              letterSpacing: -0.2,
            ),
          ),
        ),
      ),
    );
  }
}

//  Product Card

class _ProductCard extends ConsumerWidget {
  final Product product;
  final bool isDark;
  final CustomThemeExtension appTheme;

  const _ProductCard({
    required this.product,
    required this.isDark,
    required this.appTheme,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final cartNotifier = ref.read(cartProvider.notifier);
    final quantity =
        ref.watch(cartProvider).items[product.id]?.quantity ?? 0;

    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : AppColors.cardBackground,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          //  Image
          Expanded(
            flex: 5,
            child: Stack(
              children: [
                ClipRRect(
                  borderRadius: const BorderRadius.vertical(
                      top: Radius.circular(AppRadius.xl)),
                  child: product.imageUrl != null
                      ? CachedNetworkImage(
                          imageUrl: product.imageUrl!,
                          fit: BoxFit.cover,
                          width: double.infinity,
                          height: double.infinity,
                          placeholder: (_, __) => Container(
                            color: isDark
                                ? AppColors.surfaceVariantDark
                                : AppColors.surfaceVariant,
                            child: Center(
                              child: SizedBox(
                                width: 24,
                                height: 24,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: appTheme.textTertiary,
                                ),
                              ),
                            ),
                          ),
                          errorWidget: (_, __, ___) => Container(
                            color: isDark
                                ? AppColors.surfaceVariantDark
                                : AppColors.surfaceVariant,
                            child: Icon(Icons.shopping_bag_rounded,
                                size: 36, color: appTheme.textTertiary),
                          ),
                        )
                      : Container(
                          color: isDark
                              ? AppColors.surfaceVariantDark
                              : AppColors.surfaceVariant,
                          child: Icon(Icons.shopping_bag_rounded,
                              size: 36, color: appTheme.textTertiary),
                        ),
                ),
                // Discount badge
                if (product.hasDiscount)
                  Positioned(
                    top: 8,
                    left: 8,
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: AppColors.error,
                        borderRadius: AppRadius.radiusPill,
                      ),
                      child: Text(
                        'SALE',
                        style: GoogleFonts.inter(
                          fontSize: 10,
                          fontWeight: FontWeight.w800,
                          color: Colors.white,
                          letterSpacing: 0.5,
                        ),
                      ),
                    ),
                  ),
                // Out of stock overlay
                if (!product.isAvailable)
                  ClipRRect(
                    borderRadius: const BorderRadius.vertical(
                        top: Radius.circular(AppRadius.xl)),
                    child: Container(
                      color: Colors.black.withAlpha(120),
                      child: Center(
                        child: Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 12, vertical: 6),
                          decoration: BoxDecoration(
                            color: Colors.black.withAlpha(160),
                            borderRadius: AppRadius.radiusPill,
                          ),
                          child: Text(
                            'Out of Stock',
                            style: GoogleFonts.inter(
                              fontSize: 11,
                              fontWeight: FontWeight.w700,
                              color: Colors.white,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
              ],
            ),
          ),
          //  Details
          Expanded(
            flex: 4,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    product.name,
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      fontWeight: FontWeight.w700,
                      color: isDark
                          ? AppColors.textPrimaryDark
                          : AppColors.textPrimary,
                      height: 1.2,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const Spacer(),
                  // Price row
                  if (product.hasDiscount) ...[
                    Text(
                      '\$${product.price.toStringAsFixed(2)}',
                      style: GoogleFonts.inter(
                        fontSize: 11,
                        color: appTheme.textTertiary,
                        decoration: TextDecoration.lineThrough,
                        decorationColor: appTheme.textTertiary,
                      ),
                    ),
                    const SizedBox(height: 1),
                    Text(
                      '\$${product.effectivePrice.toStringAsFixed(2)}',
                      style: GoogleFonts.inter(
                        fontSize: 17,
                        fontWeight: FontWeight.w800,
                        color: AppColors.error,
                        letterSpacing: -0.3,
                      ),
                    ),
                  ] else
                    Text(
                      '\$${product.effectivePrice.toStringAsFixed(2)}',
                      style: GoogleFonts.inter(
                        fontSize: 17,
                        fontWeight: FontWeight.w800,
                        color: AppColors.accent,
                        letterSpacing: -0.3,
                      ),
                    ),
                  const SizedBox(height: 6),
                  // Add to cart / quantity row
                  _buildCartButton(context, cartNotifier, quantity),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCartButton(
    BuildContext context,
    CartNotifier cartNotifier,
    int quantity,
  ) {
    if (quantity == 0) {
      return SizedBox(
        height: 32,
        width: double.infinity,
        child: ElevatedButton(
          onPressed: product.isAvailable
              ? () {
                  try {
                    cartNotifier.addItem(product);
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text('${product.name} added to cart',
                            style: GoogleFonts.inter(fontWeight: FontWeight.w500)),
                        duration: const Duration(seconds: 1),
                        behavior: SnackBarBehavior.floating,
                        shape: AppRadius.pillButtonShape,
                        margin: const EdgeInsets.all(16),
                        backgroundColor: AppColors.success,
                      ),
                    );
                  } catch (e) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(
                            e.toString().replaceAll('Exception: ', ''),
                            style: GoogleFonts.inter(fontWeight: FontWeight.w500)),
                        backgroundColor: AppColors.error,
                        behavior: SnackBarBehavior.floating,
                        shape: AppRadius.pillButtonShape,
                        margin: const EdgeInsets.all(16),
                      ),
                    );
                  }
                }
              : null,
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            padding: EdgeInsets.zero,
            shape: AppRadius.pillButtonShape,
            elevation: 0,
            disabledBackgroundColor: isDark
                ? AppColors.surfaceVariantDark
                : AppColors.surfaceVariant,
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.add_rounded, size: 16),
              const SizedBox(width: 4),
              Text('Add',
                  style: GoogleFonts.inter(
                      fontSize: 12, fontWeight: FontWeight.w700)),
            ],
          ),
        ),
      );
    }

    return Container(
      height: 32,
      padding: const EdgeInsets.symmetric(horizontal: 4),
      decoration: BoxDecoration(
        color: AppColors.primary.withAlpha(12),
        borderRadius: AppRadius.radiusPill,
        border: Border.all(color: AppColors.primary.withAlpha(30)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          SizedBox(
            width: 28,
            height: 28,
            child: IconButton(
              padding: EdgeInsets.zero,
              onPressed: () => cartNotifier.decrementQuantity(product.id),
              icon: Icon(Icons.remove_rounded,
                  size: 16, color: AppColors.primary),
            ),
          ),
          Text(
            '$quantity',
            style: GoogleFonts.inter(
              fontSize: 14,
              fontWeight: FontWeight.w800,
              color: AppColors.primary,
            ),
          ),
          SizedBox(
            width: 28,
            height: 28,
            child: IconButton(
              padding: EdgeInsets.zero,
              onPressed: () => cartNotifier.incrementQuantity(product.id),
              icon: Icon(Icons.add_rounded,
                  size: 16, color: AppColors.primary),
            ),
          ),
        ],
      ),
    );
  }
}
