import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../data/order_model.dart';
import '../data/order_repository.dart';
import 'orders_notifier.dart';
import '../../tracking/presentation/order_tracking_map.dart';

final orderDetailsProvider =
    FutureProvider.family<Order, int>((ref, orderId) async {
  final repository = ref.watch(orderRepositoryProvider);
  return await repository.getOrderById(orderId);
});

class OrderDetailsPage extends ConsumerStatefulWidget {
  final int orderId;

  const OrderDetailsPage({super.key, required this.orderId});

  @override
  ConsumerState<OrderDetailsPage> createState() => _OrderDetailsPageState();
}

class _OrderDetailsPageState extends ConsumerState<OrderDetailsPage> {
  bool _isCancelling = false;

  //  Status helpers 
  Color _statusColor(int status) {
    switch (status) {
      case 1:
        return const Color(0xFFF59E0B); // amber
      case 2:
        return const Color(0xFF3B82F6); // blue
      case 3:
        return const Color(0xFF8B5CF6); // violet
      case 4:
        return const Color(0xFF6366F1); // indigo
      case 5:
        return const Color(0xFF10B981); // emerald
      case 6:
        return const Color(0xFFF43F5E); // rose
      default:
        return const Color(0xFF9CA3AF);
    }
  }

  IconData _statusIcon(int status) {
    switch (status) {
      case 1:
        return Icons.schedule_rounded;
      case 2:
        return Icons.check_circle_outline_rounded;
      case 3:
        return Icons.restaurant_rounded;
      case 4:
        return Icons.delivery_dining_rounded;
      case 5:
        return Icons.verified_rounded;
      case 6:
        return Icons.cancel_rounded;
      default:
        return Icons.help_outline_rounded;
    }
  }

  bool _hasAnyTimestamp(Order order) {
    return order.confirmedAt != null ||
        order.preparingAt != null ||
        order.outForDeliveryAt != null ||
        order.deliveredAt != null ||
        order.cancelledAt != null;
  }

  //  Cancel order
  Future<void> _cancelOrder(Order order) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text(
          'Cancel Order',
          style: GoogleFonts.inter(fontWeight: FontWeight.w700),
        ),
        content: Text(
          'Are you sure you want to cancel this order?',
          style: GoogleFonts.inter(fontSize: 14, color: AppColors.textSecondary),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text('No', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: TextButton.styleFrom(foregroundColor: const Color(0xFFF43F5E)),
            child: Text('Yes, Cancel',
                style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed != true || !mounted) return;

    setState(() => _isCancelling = true);

    try {
      final repository = ref.read(orderRepositoryProvider);
      await repository.cancelOrder(order.id);

      ref.invalidate(orderDetailsProvider(widget.orderId));
      ref.invalidate(ordersProvider);

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Order cancelled successfully',
              style: GoogleFonts.inter(fontWeight: FontWeight.w500)),
          backgroundColor: AppColors.success,
          behavior: SnackBarBehavior.floating,
          shape: AppRadius.pillButtonShape,
          margin: const EdgeInsets.all(16),
        ),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(e.toString().replaceAll('Exception: ', ''),
              style: GoogleFonts.inter(fontWeight: FontWeight.w500)),
          backgroundColor: AppColors.error,
          behavior: SnackBarBehavior.floating,
          shape: AppRadius.pillButtonShape,
          margin: const EdgeInsets.all(16),
        ),
      );
    } finally {
      if (mounted) setState(() => _isCancelling = false);
    }
  }

  //  Build 
  @override
  Widget build(BuildContext context) {
    final orderState = ref.watch(orderDetailsProvider(widget.orderId));
    final isDark = context.isDarkMode;

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
                child: BackdropFilter(
                  filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                  child: Icon(Icons.arrow_back_rounded,
                      size: 20,
                      color: isDark ? Colors.white : Colors.black87),
                ),
              ),
            ),
          ),
        ),
        title: Text(
          'Order Details',
          style: GoogleFonts.inter(
            fontSize: 17,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
          ),
        ),
        centerTitle: true,
      ),
      body: orderState.when(
        data: (order) => _buildContent(order, isDark),
        loading: () => const Center(
          child: CircularProgressIndicator(color: AppColors.accent),
        ),
        error: (error, _) => _buildError(error),
      ),
    );
  }

  Widget _buildContent(Order order, bool isDark) {
    final statusColor = _statusColor(order.status);
    final canCancel = order.status <= 2;

    return SingleChildScrollView(
      physics: const BouncingScrollPhysics(),
      child: Column(
        children: [
          const SizedBox(height: kToolbarHeight + 50),

          //  Gradient status header 
          _GradientStatusHeader(
            order: order,
            statusColor: statusColor,
            statusIcon: _statusIcon(order.status),
            isDark: isDark,
          ).animate().fadeIn(duration: 500.ms).slideY(begin: 0.05),

          const SizedBox(height: 20),

          //  Order timeline stepper
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: _OrderTimeline(
              currentStatus: order.status,
              statusColor: statusColor,
              isDark: isDark,
            ),
          ).animate(delay: 150.ms).fadeIn(duration: 500.ms).slideY(begin: 0.05),

          //  Live driver tracking map (only while out for delivery)
          if (order.status == 4) ...[
            const SizedBox(height: 24),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: _SectionHeader(
                  title: 'Live Tracking',
                  icon: Icons.delivery_dining_rounded,
                  isDark: isDark),
            ).animate(delay: 200.ms).fadeIn(duration: 400.ms),
            const SizedBox(height: 12),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: OrderTrackingMap(
                orderId: order.id,
                destinationLat: order.deliveryLatitude,
                destinationLng: order.deliveryLongitude,
              ),
            ).animate(delay: 220.ms).fadeIn(duration: 400.ms).slideY(begin: 0.05),
          ],

          const SizedBox(height: 24),

          //  Order items
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: _SectionHeader(title: 'Order Items', icon: Icons.shopping_bag_rounded, isDark: isDark),
          ).animate(delay: 250.ms).fadeIn(duration: 400.ms),

          const SizedBox(height: 12),

          ...order.items.asMap().entries.map((entry) {
            final idx = entry.key;
            final item = entry.value;
            return Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: _OrderItemCard(item: item, isDark: isDark),
            ).animate(delay: Duration(milliseconds: 300 + idx * 80)).fadeIn(duration: 400.ms).slideX(begin: 0.04);
          }),

          //  Notes 
          if (order.notes != null && order.notes!.isNotEmpty) ...[
            const SizedBox(height: 20),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: _NotesCard(notes: order.notes!, isDark: isDark),
            ).animate(delay: 450.ms).fadeIn(duration: 400.ms).slideY(begin: 0.03),
          ],

          const SizedBox(height: 20),

          //  Pricing summary
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: _PricingSummary(order: order, isDark: isDark),
          ).animate(delay: 550.ms).fadeIn(duration: 500.ms).slideY(begin: 0.04),

          //  Activity (per-status timestamps)
          if (_hasAnyTimestamp(order)) ...[
            const SizedBox(height: 24),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: _SectionHeader(
                title: 'Activity',
                icon: Icons.timeline_rounded,
                isDark: isDark,
              ),
            ).animate(delay: 600.ms).fadeIn(duration: 400.ms),
            const SizedBox(height: 12),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: _ActivityFeed(order: order, isDark: isDark),
            ).animate(delay: 650.ms).fadeIn(duration: 400.ms).slideY(begin: 0.04),
          ],

          //  Cancel button 
          if (canCancel) ...[
            const SizedBox(height: 24),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: SizedBox(
                width: double.infinity,
                height: 52,
                child: OutlinedButton.icon(
                  onPressed: _isCancelling ? null : () => _cancelOrder(order),
                  icon: _isCancelling
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Color(0xFFF43F5E)),
                        )
                      : const Icon(Icons.cancel_outlined, size: 20),
                  label: Text(
                    _isCancelling ? 'Cancelling...' : 'Cancel Order',
                    style: GoogleFonts.inter(fontSize: 15, fontWeight: FontWeight.w600),
                  ),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: const Color(0xFFF43F5E),
                    side: const BorderSide(color: Color(0xFFF43F5E), width: 1.5),
                    shape: AppRadius.pillButtonShape,
                  ),
                ),
              ),
            ).animate(delay: 650.ms).fadeIn(duration: 400.ms).slideY(begin: 0.04),
          ],

          const SizedBox(height: 40),
        ],
      ),
    );
  }

  Widget _buildError(Object error) {
    final isDark = context.isDarkMode;
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                color: AppColors.error.withAlpha(20),
                shape: BoxShape.circle,
              ),
              child: const Icon(Icons.error_outline_rounded, size: 40, color: AppColors.error),
            ),
            const SizedBox(height: 20),
            Text(
              'Error loading order',
              style: GoogleFonts.inter(
                fontSize: 18,
                fontWeight: FontWeight.w700,
                color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              error.toString().replaceAll('Exception: ', ''),
              style: GoogleFonts.inter(
                fontSize: 14,
                color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: () => ref.invalidate(orderDetailsProvider(widget.orderId)),
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.accent,
                foregroundColor: Colors.black87,
                shape: AppRadius.pillButtonShape,
                padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 14),
              ),
              child: Text('Retry', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
            ),
          ],
        ),
      ),
    );
  }
}

// 
// GRADIENT STATUS HEADER
// 
class _GradientStatusHeader extends StatelessWidget {
  final Order order;
  final Color statusColor;
  final IconData statusIcon;
  final bool isDark;

  const _GradientStatusHeader({
    required this.order,
    required this.statusColor,
    required this.statusIcon,
    required this.isDark,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 20),
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            statusColor,
            statusColor.withAlpha(180),
          ],
        ),
        borderRadius: AppRadius.radiusXl,
        boxShadow: AppShadows.coloredShadow(statusColor, blur: 20),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 48,
                height: 48,
                decoration: BoxDecoration(
                  color: Colors.white.withAlpha(40),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Icon(statusIcon, color: Colors.white, size: 26),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      order.statusName,
                      style: GoogleFonts.inter(
                        fontSize: 20,
                        fontWeight: FontWeight.w800,
                        color: Colors.white,
                        letterSpacing: -0.3,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Order #${order.orderNumber}',
                      style: GoogleFonts.inter(
                        fontSize: 14,
                        fontWeight: FontWeight.w500,
                        color: Colors.white.withAlpha(200),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Container(
            height: 1,
            color: Colors.white.withAlpha(50),
          ),
          const SizedBox(height: 14),
          Row(
            children: [
              if (order.vendorName != null) ...[
                Icon(Icons.store_rounded, size: 15, color: Colors.white.withAlpha(200)),
                const SizedBox(width: 6),
                Expanded(
                  child: Text(
                    order.vendorName!,
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      fontWeight: FontWeight.w500,
                      color: Colors.white.withAlpha(220),
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                const SizedBox(width: 12),
              ],
              Icon(Icons.access_time_rounded, size: 15, color: Colors.white.withAlpha(200)),
              const SizedBox(width: 6),
              Text(
                _formatDateTime(order.createdAt),
                style: GoogleFonts.inter(
                  fontSize: 12,
                  fontWeight: FontWeight.w500,
                  color: Colors.white.withAlpha(200),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour}:${dateTime.minute.toString().padLeft(2, '0')}';
  }
}

// 
// ORDER TIMELINE
// 
class _OrderTimeline extends StatelessWidget {
  final int currentStatus;
  final Color statusColor;
  final bool isDark;

  const _OrderTimeline({
    required this.currentStatus,
    required this.statusColor,
    required this.isDark,
  });

  static const _steps = [
    (status: 1, label: 'Pending', icon: Icons.schedule_rounded),
    (status: 2, label: 'Confirmed', icon: Icons.check_circle_outline_rounded),
    (status: 3, label: 'Preparing', icon: Icons.restaurant_rounded),
    (status: 4, label: 'On the way', icon: Icons.delivery_dining_rounded),
    (status: 5, label: 'Delivered', icon: Icons.verified_rounded),
  ];

  @override
  Widget build(BuildContext context) {
    final isCancelled = currentStatus == 6;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: (isDark ? Colors.white : Colors.black).withAlpha(8),
        ),
      ),
      child: isCancelled
          ? Row(
              children: [
                Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: const Color(0xFFF43F5E).withAlpha(20),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.cancel_rounded, color: Color(0xFFF43F5E), size: 22),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Order Cancelled',
                        style: GoogleFonts.inter(
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                          color: const Color(0xFFF43F5E),
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        'This order has been cancelled',
                        style: GoogleFonts.inter(
                          fontSize: 12,
                          color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            )
          : Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: _steps.asMap().entries.map((entry) {
                final idx = entry.key;
                final step = entry.value;
                final isCompleted = currentStatus >= step.status;
                final isCurrent = currentStatus == step.status;
                final isLast = idx == _steps.length - 1;

                return Expanded(
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          children: [
                            Container(
                              width: isCurrent ? 36 : 30,
                              height: isCurrent ? 36 : 30,
                              decoration: BoxDecoration(
                                color: isCompleted
                                    ? statusColor
                                    : (isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant),
                                shape: BoxShape.circle,
                                boxShadow: isCurrent
                                    ? AppShadows.coloredShadow(statusColor, blur: 12, spread: -2)
                                    : null,
                              ),
                              child: Icon(
                                step.icon,
                                size: isCurrent ? 18 : 15,
                                color: isCompleted
                                    ? Colors.white
                                    : (isDark ? AppColors.textTertiaryDark : AppColors.textTertiary),
                              ),
                            ),
                            const SizedBox(height: 6),
                            Text(
                              step.label,
                              style: GoogleFonts.inter(
                                fontSize: 9,
                                fontWeight: isCurrent ? FontWeight.w700 : FontWeight.w500,
                                color: isCompleted
                                    ? (isDark ? AppColors.textPrimaryDark : AppColors.textPrimary)
                                    : (isDark ? AppColors.textTertiaryDark : AppColors.textTertiary),
                              ),
                              textAlign: TextAlign.center,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                      if (!isLast)
                        Expanded(
                          child: Container(
                            height: 2,
                            margin: const EdgeInsets.only(bottom: 18),
                            decoration: BoxDecoration(
                              color: currentStatus > step.status
                                  ? statusColor
                                  : (isDark ? AppColors.surfaceVariantDark : AppColors.outline),
                              borderRadius: AppRadius.radiusPill,
                            ),
                          ),
                        ),
                    ],
                  ),
                );
              }).toList(),
            ),
    );
  }
}

// 
// SECTION HEADER
// 
class _SectionHeader extends StatelessWidget {
  final String title;
  final IconData icon;
  final bool isDark;

  const _SectionHeader({required this.title, required this.icon, required this.isDark});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          width: 32,
          height: 32,
          decoration: BoxDecoration(
            color: AppColors.accent.withAlpha(20),
            borderRadius: AppRadius.radiusSm,
          ),
          child: Icon(icon, size: 17, color: AppColors.accentDark),
        ),
        const SizedBox(width: 10),
        Text(
          title,
          style: GoogleFonts.inter(
            fontSize: 16,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
            color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
          ),
        ),
      ],
    );
  }
}

// 
// ORDER ITEM CARD
// 
class _OrderItemCard extends StatelessWidget {
  final OrderItem item;
  final bool isDark;

  const _OrderItemCard({required this.item, required this.isDark});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
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
        children: [
          // Product image
          ClipRRect(
            borderRadius: AppRadius.radiusMd,
            child: item.productImageUrl != null
                ? Image.network(
                    item.productImageUrl!,
                    width: 64,
                    height: 64,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => _imagePlaceholder(),
                  )
                : _imagePlaceholder(),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.productName,
                  style: GoogleFonts.inter(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                  ),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 6),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant,
                    borderRadius: AppRadius.radiusPill,
                  ),
                  child: Text(
                    '\$${item.price.toStringAsFixed(2)} x ${item.quantity}',
                    style: GoogleFonts.inter(
                      fontSize: 12,
                      fontWeight: FontWeight.w500,
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 8),
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
    );
  }

  Widget _imagePlaceholder() {
    return Container(
      width: 64,
      height: 64,
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
          color: AppColors.accent.withAlpha(120), size: 28),
    );
  }
}

// 
// NOTES CARD
// 
class _NotesCard extends StatelessWidget {
  final String notes;
  final bool isDark;

  const _NotesCard({required this.notes, required this.isDark});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: (isDark ? Colors.white : Colors.black).withAlpha(6),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.note_alt_rounded, size: 16, color: AppColors.accent),
              const SizedBox(width: 8),
              Text(
                'Notes',
                style: GoogleFonts.inter(
                  fontSize: 14,
                  fontWeight: FontWeight.w700,
                  color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: isDark ? AppColors.surfaceVariantDark : AppColors.surfaceVariant,
              borderRadius: AppRadius.radiusMd,
            ),
            child: Text(
              notes,
              style: GoogleFonts.inter(
                fontSize: 13,
                height: 1.5,
                color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

//
// ACTIVITY FEED — per-status timestamps from backend Path 2
//
class _ActivityFeed extends StatelessWidget {
  final Order order;
  final bool isDark;

  const _ActivityFeed({required this.order, required this.isDark});

  @override
  Widget build(BuildContext context) {
    final entries = <_ActivityEntry>[
      _ActivityEntry(
        label: 'Order placed',
        timestamp: order.createdAt,
        icon: Icons.receipt_long_rounded,
        color: const Color(0xFFF59E0B),
      ),
      if (order.confirmedAt != null)
        _ActivityEntry(
          label: 'Confirmed by vendor',
          timestamp: order.confirmedAt!,
          icon: Icons.check_circle_outline_rounded,
          color: const Color(0xFF3B82F6),
        ),
      if (order.preparingAt != null)
        _ActivityEntry(
          label: 'Being prepared',
          timestamp: order.preparingAt!,
          icon: Icons.restaurant_rounded,
          color: const Color(0xFF8B5CF6),
        ),
      if (order.outForDeliveryAt != null)
        _ActivityEntry(
          label: 'Out for delivery',
          timestamp: order.outForDeliveryAt!,
          icon: Icons.delivery_dining_rounded,
          color: const Color(0xFF6366F1),
        ),
      if (order.deliveredAt != null)
        _ActivityEntry(
          label: 'Delivered',
          timestamp: order.deliveredAt!,
          icon: Icons.verified_rounded,
          color: const Color(0xFF10B981),
        ),
      if (order.cancelledAt != null)
        _ActivityEntry(
          label: 'Cancelled',
          timestamp: order.cancelledAt!,
          icon: Icons.cancel_rounded,
          color: const Color(0xFFF43F5E),
        ),
    ];

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: (isDark ? Colors.white : Colors.black).withAlpha(6),
        ),
      ),
      child: Column(
        children: [
          for (var i = 0; i < entries.length; i++)
            _buildRow(entries[i], isLast: i == entries.length - 1),
        ],
      ),
    );
  }

  Widget _buildRow(_ActivityEntry entry, {required bool isLast}) {
    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Column(
            children: [
              Container(
                width: 34,
                height: 34,
                decoration: BoxDecoration(
                  color: entry.color.withAlpha(25),
                  shape: BoxShape.circle,
                ),
                child: Icon(entry.icon, size: 17, color: entry.color),
              ),
              if (!isLast)
                Expanded(
                  child: Container(
                    width: 2,
                    color: (isDark ? Colors.white : Colors.black).withAlpha(20),
                  ),
                ),
            ],
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Padding(
              padding: EdgeInsets.only(bottom: isLast ? 0 : 14, top: 4),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    entry.label,
                    style: GoogleFonts.inter(
                      fontSize: 14,
                      fontWeight: FontWeight.w700,
                      color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    _formatTimestamp(entry.timestamp),
                    style: GoogleFonts.inter(
                      fontSize: 12,
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  String _formatTimestamp(DateTime dt) {
    final local = dt.toLocal();
    final day = local.day.toString().padLeft(2, '0');
    final month = local.month.toString().padLeft(2, '0');
    final year = local.year;
    final hour = local.hour.toString().padLeft(2, '0');
    final minute = local.minute.toString().padLeft(2, '0');
    return '$day/$month/$year · $hour:$minute';
  }
}

class _ActivityEntry {
  final String label;
  final DateTime timestamp;
  final IconData icon;
  final Color color;
  const _ActivityEntry({
    required this.label,
    required this.timestamp,
    required this.icon,
    required this.color,
  });
}

//
// PRICING SUMMARY
//
class _PricingSummary extends StatelessWidget {
  final Order order;
  final bool isDark;

  const _PricingSummary({required this.order, required this.isDark});

  @override
  Widget build(BuildContext context) {
    final subtotal = order.totalAmount - order.deliveryFee;

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: (isDark ? Colors.white : Colors.black).withAlpha(6),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 32,
                height: 32,
                decoration: BoxDecoration(
                  color: AppColors.accent.withAlpha(20),
                  borderRadius: AppRadius.radiusSm,
                ),
                child: const Icon(Icons.receipt_long_rounded, size: 17, color: AppColors.accentDark),
              ),
              const SizedBox(width: 10),
              Text(
                'Price Summary',
                style: GoogleFonts.inter(
                  fontSize: 16,
                  fontWeight: FontWeight.w700,
                  letterSpacing: -0.3,
                  color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          _priceRow('Subtotal', subtotal),
          const SizedBox(height: 10),
          _priceRow('Delivery Fee', order.deliveryFee),
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 14),
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
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Total',
                style: GoogleFonts.inter(
                  fontSize: 17,
                  fontWeight: FontWeight.w800,
                  color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
                decoration: BoxDecoration(
                  gradient: AppColors.accentGradient,
                  borderRadius: AppRadius.radiusPill,
                  boxShadow: AppShadows.coloredShadow(AppColors.accent, blur: 12, spread: -4),
                ),
                child: Text(
                  '\$${order.totalAmount.toStringAsFixed(2)}',
                  style: GoogleFonts.inter(
                    fontSize: 20,
                    fontWeight: FontWeight.w800,
                    color: Colors.black87,
                    letterSpacing: -0.5,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _priceRow(String label, double amount) {
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
          '\$${amount.toStringAsFixed(2)}',
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
