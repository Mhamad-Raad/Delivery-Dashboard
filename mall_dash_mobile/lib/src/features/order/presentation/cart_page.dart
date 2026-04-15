import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import 'cart_notifier.dart';
import '../data/order_model.dart';
import '../data/order_repository.dart';
import 'orders_notifier.dart';

class CartPage extends ConsumerStatefulWidget {
  const CartPage({super.key});

  @override
  ConsumerState<CartPage> createState() => _CartPageState();
}

class _CartPageState extends ConsumerState<CartPage> {
  final _notesController = TextEditingController();
  final _deliveryFeeController = TextEditingController(text: '5.00');
  bool _isPlacingOrder = false;

  @override
  void dispose() {
    _notesController.dispose();
    _deliveryFeeController.dispose();
    super.dispose();
  }

  //  Place order 
  Future<void> _placeOrder() async {
    final cart = ref.read(cartProvider);

    if (cart.isEmpty) {
      _showSnackBar('Cart is empty', isError: true);
      return;
    }

    if (cart.vendorId == null) {
      _showSnackBar('Invalid cart state', isError: true);
      return;
    }

    final deliveryFee = double.tryParse(_deliveryFeeController.text) ?? 0.0;

    setState(() => _isPlacingOrder = true);

    try {
      final request = CreateOrderRequest(
        vendorId: cart.vendorId!,
        addressId: null,
        deliveryFee: deliveryFee,
        notes: _notesController.text.isEmpty ? null : _notesController.text,
        items: cart.items.values.map((item) {
          return CreateOrderItem(
            productId: item.product.id,
            quantity: item.quantity,
          );
        }).toList(),
      );

      final repository = ref.read(orderRepositoryProvider);
      final order = await repository.createOrder(request);

      ref.read(cartProvider.notifier).clear();
      ref.invalidate(ordersProvider);

      if (!mounted) return;

      _showSnackBar('Order placed successfully! Order #${order.orderNumber}');
      Navigator.pop(context);
    } catch (e) {
      if (!mounted) return;
      _showSnackBar(e.toString().replaceAll('Exception: ', ''), isError: true);
    } finally {
      if (mounted) setState(() => _isPlacingOrder = false);
    }
  }

  void _showSnackBar(String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message, style: GoogleFonts.inter(fontWeight: FontWeight.w500)),
        backgroundColor: isError ? AppColors.error : AppColors.success,
        behavior: SnackBarBehavior.floating,
        shape: AppRadius.pillButtonShape,
        margin: const EdgeInsets.all(16),
        duration: Duration(seconds: isError ? 4 : 3),
      ),
    );
  }

  void _confirmClearCart() {
    final isDark = context.isDarkMode;
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: AppRadius.dialogShape,
        backgroundColor: isDark ? AppColors.cardBackgroundDark : Colors.white,
        title: Text('Clear Cart',
            style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text('Remove all items from cart?',
            style: GoogleFonts.inter(
              fontSize: 14,
              color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
            )),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: Text('Cancel', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
          TextButton(
            onPressed: () {
              ref.read(cartProvider.notifier).clear();
              Navigator.pop(ctx);
            },
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: Text('Clear', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );
  }

  //  Build 
  @override
  Widget build(BuildContext context) {
    final cart = ref.watch(cartProvider);
    final isDark = context.isDarkMode;
    final deliveryFee = double.tryParse(_deliveryFeeController.text) ?? 0.0;
    final total = cart.subtotal + deliveryFee;

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
                  color: (isDark ? Colors.white : Colors.black).withAlpha(isDark ? 25 : 15),
                  borderRadius: AppRadius.radiusPill,
                  border: Border.all(
                    color: (isDark ? Colors.white : Colors.black).withAlpha(10),
                  ),
                ),
                child: Icon(Icons.arrow_back_rounded,
                    size: 20,
                    color: isDark ? Colors.white : Colors.black87),
              ),
            ),
          ),
        ),
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              'Cart',
              style: GoogleFonts.inter(
                fontSize: 17,
                fontWeight: FontWeight.w700,
                letterSpacing: -0.3,
              ),
            ),
            if (cart.isNotEmpty) ...[
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                  color: AppColors.accent.withAlpha(25),
                  borderRadius: AppRadius.radiusPill,
                ),
                child: Text(
                  '${cart.totalItems}',
                  style: GoogleFonts.inter(
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                    color: AppColors.accentDark,
                  ),
                ),
              ),
            ],
          ],
        ),
        centerTitle: true,
        actions: [
          if (cart.isNotEmpty)
            Padding(
              padding: const EdgeInsets.only(right: 12),
              child: GestureDetector(
                onTap: _confirmClearCart,
                child: Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: AppColors.error.withAlpha(15),
                    borderRadius: AppRadius.radiusPill,
                  ),
                  child: const Icon(Icons.delete_sweep_rounded,
                      size: 20, color: AppColors.error),
                ),
              ),
            ),
        ],
      ),
      body: cart.isEmpty
          ? _buildEmptyState(isDark)
          : Column(
              children: [
                Expanded(
                  child: _buildItemsList(cart, isDark),
                ),
                _buildCheckoutFooter(cart, deliveryFee, total, isDark),
              ],
            ),
    );
  }

  //  Empty state 
  Widget _buildEmptyState(bool isDark) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(40),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 120,
              height: 120,
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [
                    AppColors.accent.withAlpha(25),
                    AppColors.accent.withAlpha(10),
                  ],
                ),
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.shopping_cart_outlined,
                size: 56,
                color: AppColors.accent.withAlpha(150),
              ),
            )
                .animate()
                .fadeIn(duration: 600.ms)
                .scale(begin: const Offset(0.8, 0.8), end: const Offset(1, 1), duration: 600.ms),
            const SizedBox(height: 28),
            Text(
              'Your cart is empty',
              style: GoogleFonts.inter(
                fontSize: 22,
                fontWeight: FontWeight.w800,
                letterSpacing: -0.5,
                color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
              ),
            ).animate(delay: 200.ms).fadeIn(duration: 400.ms),
            const SizedBox(height: 8),
            Text(
              'Browse products and add items\nto get started',
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(
                fontSize: 14,
                height: 1.5,
                fontWeight: FontWeight.w400,
                color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
              ),
            ).animate(delay: 300.ms).fadeIn(duration: 400.ms),
          ],
        ),
      ),
    );
  }

  //  Items list 
  Widget _buildItemsList(Cart cart, bool isDark) {
    final cartNotifier = ref.read(cartProvider.notifier);
    final items = cart.items.values.toList();

    return CustomScrollView(
      physics: const BouncingScrollPhysics(),
      slivers: [
        SliverToBoxAdapter(child: SizedBox(height: kToolbarHeight + MediaQuery.of(context).padding.top + 12)),

        // Items count header
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 0, 20, 14),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                  decoration: BoxDecoration(
                    color: isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant,
                    borderRadius: AppRadius.radiusPill,
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.shopping_bag_rounded,
                          size: 14,
                          color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary),
                      const SizedBox(width: 6),
                      Text(
                        '${items.length} ${items.length == 1 ? 'item' : 'items'} \u00B7 ${cart.totalItems} ${cart.totalItems == 1 ? 'product' : 'products'}',
                        style: GoogleFonts.inter(
                          fontSize: 12,
                          fontWeight: FontWeight.w600,
                          color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ).animate().fadeIn(duration: 400.ms),
        ),

        // Cart items
        SliverPadding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          sliver: SliverList(
            delegate: SliverChildBuilderDelegate(
              (context, index) {
                final item = items[index];
                final product = item.product;

                return Dismissible(
                  key: ValueKey(product.id),
                  direction: DismissDirection.endToStart,
                  onDismissed: (_) => cartNotifier.removeItem(product.id),
                  background: Container(
                    margin: const EdgeInsets.only(bottom: 12),
                    decoration: BoxDecoration(
                      color: AppColors.error,
                      borderRadius: AppRadius.radiusLg,
                    ),
                    alignment: Alignment.centerRight,
                    padding: const EdgeInsets.only(right: 24),
                    child: const Icon(Icons.delete_rounded, color: Colors.white, size: 24),
                  ),
                  child: _CartItemCard(
                    item: item,
                    isDark: isDark,
                    onIncrement: () => cartNotifier.incrementQuantity(product.id),
                    onDecrement: () => cartNotifier.decrementQuantity(product.id),
                    onRemove: () => cartNotifier.removeItem(product.id),
                  ),
                )
                    .animate(delay: Duration(milliseconds: 100 + index * 70))
                    .fadeIn(duration: 400.ms)
                    .slideX(begin: 0.05, end: 0, duration: 400.ms);
              },
              childCount: items.length,
            ),
          ),
        ),

        // Notes field
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 12),
            child: Container(
              decoration: BoxDecoration(
                color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                borderRadius: AppRadius.radiusLg,
                boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                border: Border.all(
                  color: (isDark ? Colors.white : Colors.black).withAlpha(6),
                ),
              ),
              child: TextField(
                controller: _notesController,
                style: GoogleFonts.inter(fontSize: 14),
                decoration: InputDecoration(
                  hintText: 'Add a note (optional)',
                  hintStyle: GoogleFonts.inter(
                    fontSize: 14,
                    color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                  ),
                  border: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  prefixIcon: Padding(
                    padding: const EdgeInsets.only(left: 12, right: 4),
                    child: Icon(Icons.note_alt_outlined,
                        size: 20,
                        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary),
                  ),
                  prefixIconConstraints: const BoxConstraints(minWidth: 36),
                ),
                maxLines: 2,
                maxLength: 500,
                buildCounter: (_, {required currentLength, maxLength, required isFocused}) => null,
              ),
            ),
          ).animate(delay: 300.ms).fadeIn(duration: 400.ms),
        ),

        // Delivery fee field
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 0, 20, 12),
            child: Container(
              decoration: BoxDecoration(
                color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                borderRadius: AppRadius.radiusLg,
                boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                border: Border.all(
                  color: (isDark ? Colors.white : Colors.black).withAlpha(6),
                ),
              ),
              child: TextField(
                controller: _deliveryFeeController,
                style: GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w600),
                decoration: InputDecoration(
                  hintText: 'Delivery fee',
                  hintStyle: GoogleFonts.inter(
                    fontSize: 14,
                    color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                  ),
                  border: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  prefixIcon: Padding(
                    padding: const EdgeInsets.only(left: 12, right: 4),
                    child: Icon(Icons.delivery_dining_rounded,
                        size: 20,
                        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary),
                  ),
                  prefixIconConstraints: const BoxConstraints(minWidth: 36),
                  prefixText: '\$ ',
                  prefixStyle: GoogleFonts.inter(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                  ),
                ),
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                onChanged: (_) => setState(() {}),
              ),
            ),
          ).animate(delay: 350.ms).fadeIn(duration: 400.ms),
        ),

        // Bottom spacing for checkout footer
        const SliverToBoxAdapter(child: SizedBox(height: 20)),
      ],
    );
  }

  //  Glassmorphic checkout footer 
  Widget _buildCheckoutFooter(Cart cart, double deliveryFee, double total, bool isDark) {
    return ClipRRect(
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 25, sigmaY: 25),
        child: Container(
          padding: EdgeInsets.fromLTRB(20, 20, 20, MediaQuery.of(context).padding.bottom + 16),
          decoration: BoxDecoration(
            color: (isDark ? AppColors.cardBackgroundDark : Colors.white).withAlpha(isDark ? 230 : 240),
            border: Border(
              top: BorderSide(
                color: (isDark ? Colors.white : Colors.black).withAlpha(12),
              ),
            ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withAlpha(isDark ? 40 : 10),
                blurRadius: 20,
                offset: const Offset(0, -8),
              ),
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Subtotal
              _summaryRow(
                'Subtotal',
                '\$${cart.subtotal.toStringAsFixed(2)}',
                isDark,
              ),
              const SizedBox(height: 8),
              _summaryRow(
                'Delivery',
                '\$${deliveryFee.toStringAsFixed(2)}',
                isDark,
              ),
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 12),
                child: Container(
                  height: 1,
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      colors: [
                        Colors.transparent,
                        AppColors.accent.withAlpha(60),
                        AppColors.accent.withAlpha(60),
                        Colors.transparent,
                      ],
                    ),
                  ),
                ),
              ),
              // Total row
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Total',
                    style: GoogleFonts.inter(
                      fontSize: 16,
                      fontWeight: FontWeight.w800,
                      color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 5),
                    decoration: BoxDecoration(
                      gradient: AppColors.accentGradient,
                      borderRadius: AppRadius.radiusPill,
                      boxShadow: AppShadows.coloredShadow(AppColors.accent, blur: 12, spread: -4),
                    ),
                    child: Text(
                      '\$${total.toStringAsFixed(2)}',
                      style: GoogleFonts.inter(
                        fontSize: 19,
                        fontWeight: FontWeight.w800,
                        color: Colors.black87,
                        letterSpacing: -0.5,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              // Checkout button
              SizedBox(
                width: double.infinity,
                height: 54,
                child: ElevatedButton(
                  onPressed: _isPlacingOrder ? null : _placeOrder,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    disabledBackgroundColor: AppColors.primary.withAlpha(150),
                    elevation: 0,
                    shape: AppRadius.pillButtonShape,
                  ),
                  child: _isPlacingOrder
                      ? Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                color: Colors.white,
                                strokeWidth: 2.5,
                              ),
                            ),
                            const SizedBox(width: 12),
                            Text(
                              'Placing Order...',
                              style: GoogleFonts.inter(
                                fontSize: 16,
                                fontWeight: FontWeight.w700,
                                color: Colors.white,
                              ),
                            ),
                          ],
                        )
                      : Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.shopping_bag_rounded, size: 20),
                            const SizedBox(width: 10),
                            Text(
                              'Place Order',
                              style: GoogleFonts.inter(
                                fontSize: 16,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ],
                        ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _summaryRow(String label, String value, bool isDark) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: GoogleFonts.inter(
            fontSize: 14,
            fontWeight: FontWeight.w500,
            color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
          ),
        ),
        Text(
          value,
          style: GoogleFonts.inter(
            fontSize: 14,
            fontWeight: FontWeight.w600,
            color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
          ),
        ),
      ],
    );
  }
}

// 
// CART ITEM CARD
// 
class _CartItemCard extends StatelessWidget {
  final CartItem item;
  final bool isDark;
  final VoidCallback onIncrement;
  final VoidCallback onDecrement;
  final VoidCallback onRemove;

  const _CartItemCard({
    required this.item,
    required this.isDark,
    required this.onIncrement,
    required this.onDecrement,
    required this.onRemove,
  });

  @override
  Widget build(BuildContext context) {
    final product = item.product;

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: (isDark ? Colors.white : Colors.black).withAlpha(6),
        ),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Product image
          ClipRRect(
            borderRadius: AppRadius.radiusMd,
            child: product.imageUrl != null
                ? Image.network(
                    product.imageUrl!,
                    width: 76,
                    height: 76,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => _imagePlaceholder(),
                  )
                : _imagePlaceholder(),
          ),
          const SizedBox(width: 14),
          // Product details
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  product.name,
                  style: GoogleFonts.inter(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                    height: 1.3,
                  ),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 6),
                // Price
                if (product.hasDiscount)
                  Row(
                    children: [
                      Text(
                        '\$${product.price.toStringAsFixed(2)}',
                        style: GoogleFonts.inter(
                          fontSize: 12,
                          color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                          decoration: TextDecoration.lineThrough,
                          decorationColor: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                        ),
                      ),
                      const SizedBox(width: 6),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                        decoration: BoxDecoration(
                          color: AppColors.error.withAlpha(15),
                          borderRadius: AppRadius.radiusPill,
                        ),
                        child: Text(
                          '\$${product.effectivePrice.toStringAsFixed(2)}',
                          style: GoogleFonts.inter(
                            fontSize: 13,
                            fontWeight: FontWeight.w700,
                            color: AppColors.error,
                          ),
                        ),
                      ),
                    ],
                  )
                else
                  Text(
                    '\$${product.effectivePrice.toStringAsFixed(2)}',
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                  ),
                const SizedBox(height: 10),
                // Quantity stepper + total
                Row(
                  children: [
                    // Premium quantity stepper
                    Container(
                      decoration: BoxDecoration(
                        color: isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant,
                        borderRadius: AppRadius.radiusPill,
                        border: Border.all(
                          color: (isDark ? Colors.white : Colors.black).withAlpha(8),
                        ),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          _stepperButton(
                            icon: item.quantity == 1
                                ? Icons.delete_outline_rounded
                                : Icons.remove_rounded,
                            onTap: onDecrement,
                            color: item.quantity == 1 ? AppColors.error : null,
                          ),
                          Container(
                            constraints: const BoxConstraints(minWidth: 32),
                            alignment: Alignment.center,
                            child: Text(
                              '${item.quantity}',
                              style: GoogleFonts.inter(
                                fontSize: 14,
                                fontWeight: FontWeight.w700,
                                color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                              ),
                            ),
                          ),
                          _stepperButton(
                            icon: Icons.add_rounded,
                            onTap: onIncrement,
                          ),
                        ],
                      ),
                    ),
                    const Spacer(),
                    // Item total
                    Text(
                      '\$${item.totalPrice.toStringAsFixed(2)}',
                      style: GoogleFonts.inter(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                        color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _stepperButton({
    required IconData icon,
    required VoidCallback onTap,
    Color? color,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 32,
        height: 32,
        alignment: Alignment.center,
        child: Icon(
          icon,
          size: 17,
          color: color ?? (isDark ? AppColors.textSecondaryDark : AppColors.textSecondary),
        ),
      ),
    );
  }

  Widget _imagePlaceholder() {
    return Container(
      width: 76,
      height: 76,
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            AppColors.accent.withAlpha(30),
            AppColors.accent.withAlpha(15),
          ],
        ),
        borderRadius: AppRadius.radiusMd,
      ),
      child: Icon(Icons.shopping_bag_rounded,
          color: AppColors.accent.withAlpha(120), size: 30),
    );
  }
}
