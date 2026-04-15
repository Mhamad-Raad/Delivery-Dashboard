import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/status_badge.dart';
import '../../../core/widgets/error_state.dart';
import '../../../core/widgets/loading_indicator.dart';
import 'driver_orders_notifier.dart';
import 'delivery_details_page.dart';
import '../data/driver_order_model.dart';

class AvailableOrdersPage extends ConsumerWidget {
  const AvailableOrdersPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final ordersState = ref.watch(availableOrdersNotifierProvider);
    final appTheme = context.appTheme;

    return ordersState.when(
      data: (orders) {
        if (orders.isEmpty) {
          return Center(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(32, 0, 32, 100),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Container(
                    width: 80,
                    height: 80,
                    decoration: BoxDecoration(
                      color: Theme.of(context).colorScheme.primary.withAlpha(20),
                      borderRadius: AppRadius.radiusXl,
                    ),
                    child: Icon(
                      Icons.notifications_active_outlined,
                      size: 40,
                      color: Theme.of(context).colorScheme.primary,
                    ),
                  ),
                  const SizedBox(height: 20),
                  Text(
                    'Waiting for Orders',
                    style: GoogleFonts.inter(
                      fontSize: 20,
                      fontWeight: FontWeight.w700,
                      letterSpacing: -0.3,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Orders are automatically assigned by the system.\nYou\'ll receive a notification when an order is assigned to you.',
                    textAlign: TextAlign.center,
                    style: GoogleFonts.inter(
                      fontSize: 14,
                      color: appTheme.textSecondary,
                      height: 1.5,
                    ),
                  ),
                ].animate(interval: 100.ms).fadeIn(duration: 400.ms).slideY(begin: 0.1, end: 0),
              ),
            ),
          );
        }

        return RefreshIndicator(
          onRefresh: () async {
            await ref.read(availableOrdersNotifierProvider.notifier).refresh();
          },
          child: ListView.builder(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 120),
            itemCount: orders.length,
            itemBuilder: (context, index) {
              final order = orders[index];
              return _PremiumOrderCard(order: order)
                  .animate()
                  .fadeIn(duration: 400.ms, delay: Duration(milliseconds: index * 80))
                  .slideX(begin: 0.05, end: 0, duration: 400.ms, delay: Duration(milliseconds: index * 80));
            },
          ),
        );
      },
      loading: () => const LoadingIndicator(),
      error: (error, stackTrace) => ErrorState(
        title: 'Error loading orders',
        message: error.toString(),
        onRetry: () {
          ref.read(availableOrdersNotifierProvider.notifier).refresh();
        },
      ),
    );
  }
}

class _PremiumOrderCard extends ConsumerWidget {
  final DriverOrder order;

  const _PremiumOrderCard({required this.order});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final appTheme = context.appTheme;
    final isDark = context.isDarkMode;

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () {
            Navigator.push(
              context,
              MaterialPageRoute(
                builder: (context) => DeliveryDetailsPage(orderId: order.id),
              ),
            );
          },
          borderRadius: AppRadius.radiusXl,
          child: Padding(
            padding: const EdgeInsets.all(18),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header row
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(8),
                          decoration: BoxDecoration(
                            color: appTheme.warningColor.withAlpha(20),
                            borderRadius: AppRadius.radiusMd,
                          ),
                          child: Icon(Icons.receipt_long_rounded, size: 20, color: appTheme.warningColor),
                        ),
                        const SizedBox(width: 10),
                        Text(
                          '#${order.orderNumber}',
                          style: GoogleFonts.inter(
                            fontSize: 16,
                            fontWeight: FontWeight.w700,
                            letterSpacing: -0.3,
                          ),
                        ),
                      ],
                    ),
                    StatusBadgeSolid(
                      label: order.statusName,
                      statusCode: order.status,
                      size: StatusBadgeSize.small,
                    ),
                  ],
                ),
                const SizedBox(height: 14),

                // Vendor Name
                if (order.vendorName != null) ...[
                  Row(
                    children: [
                      Icon(Icons.store_rounded, size: 16, color: appTheme.textTertiary),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          order.vendorName!,
                          style: GoogleFonts.inter(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            color: Theme.of(context).colorScheme.primary,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                ],

                // Customer
                Row(
                  children: [
                    Icon(Icons.person_outline_rounded, size: 16, color: appTheme.textTertiary),
                    const SizedBox(width: 8),
                    Text(
                      order.customerName ?? 'Customer',
                      style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
                    ),
                  ],
                ),
                if (order.customerPhone != null) ...[
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(Icons.phone_outlined, size: 16, color: appTheme.textTertiary),
                      const SizedBox(width: 8),
                      Text(
                        order.customerPhone!,
                        style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
                      ),
                    ],
                  ),
                ],

                // Delivery Address
                const SizedBox(height: 4),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Padding(
                      padding: const EdgeInsets.only(top: 2),
                      child: Icon(Icons.location_on_outlined, size: 16, color: appTheme.textTertiary),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        _formatAddress(order),
                        style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
                      ),
                    ),
                  ],
                ),

                // Total
                if (order.totalAmount != null) ...[
                  const SizedBox(height: 12),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                    decoration: BoxDecoration(
                      color: appTheme.successColor.withAlpha(12),
                      borderRadius: AppRadius.radiusMd,
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Total Amount',
                          style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
                        ),
                        Text(
                          '\$${order.totalAmount!.toStringAsFixed(2)}',
                          style: GoogleFonts.inter(
                            fontSize: 17,
                            fontWeight: FontWeight.w800,
                            color: appTheme.successColor,
                            letterSpacing: -0.3,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],

                // Action Buttons
                const SizedBox(height: 14),
                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton(
                        onPressed: order.assignmentId != null
                            ? () => _rejectOrder(context, ref, order.assignmentId!)
                            : null,
                        style: OutlinedButton.styleFrom(
                          foregroundColor: appTheme.errorColor,
                          side: BorderSide(color: appTheme.errorColor.withAlpha(80)),
                          shape: AppRadius.buttonShape,
                          padding: const EdgeInsets.symmetric(vertical: 12),
                        ),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.close_rounded, size: 18),
                            const SizedBox(width: 6),
                            Text('Reject', style: GoogleFonts.inter(fontWeight: FontWeight.w600, fontSize: 14)),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      flex: 2,
                      child: Container(
                        decoration: BoxDecoration(
                          gradient: AppColors.successGradient,
                          borderRadius: AppRadius.radiusMd,
                          boxShadow: AppShadows.coloredShadow(AppColors.success),
                        ),
                        child: Material(
                          color: Colors.transparent,
                          child: InkWell(
                            onTap: order.assignmentId != null
                                ? () => _acceptOrder(context, ref, order)
                                : null,
                            borderRadius: AppRadius.radiusMd,
                            child: Padding(
                              padding: const EdgeInsets.symmetric(vertical: 12),
                              child: Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  const Icon(Icons.check_rounded, size: 18, color: Colors.white),
                                  const SizedBox(width: 6),
                                  Text(
                                    'Accept Order',
                                    style: GoogleFonts.inter(
                                      fontWeight: FontWeight.w700,
                                      fontSize: 14,
                                      color: Colors.white,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _formatAddress(DriverOrder order) {
    final parts = <String>[];
    if (order.apartmentNumber != null) parts.add('Apt ${order.apartmentNumber}');
    if (order.floorNumber != null) parts.add('Floor ${order.floorNumber}');
    if (order.buildingName != null) parts.add(order.buildingName!);
    if (order.deliveryAddress != null) parts.add(order.deliveryAddress!);
    return parts.isEmpty ? 'No address provided' : parts.join(', ');
  }

  Future<void> _acceptOrder(BuildContext context, WidgetRef ref, DriverOrder order) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Accept Order', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text('Do you want to accept order #${order.orderNumber}?', style: GoogleFonts.inter()),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text('Cancel', style: GoogleFonts.inter()),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(shape: AppRadius.buttonShape),
            child: Text('Accept', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed == true && context.mounted) {
      try {
        await ref
            .read(availableOrdersNotifierProvider.notifier)
            .acceptOrder(order.assignmentId!, order);

        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Order accepted successfully', style: GoogleFonts.inter()),
              backgroundColor: context.appTheme.successColor,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
            ),
          );
        }
      } catch (e) {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Error accepting order: $e', style: GoogleFonts.inter()),
              backgroundColor: context.appTheme.errorColor,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
            ),
          );
        }
      }
    }
  }

  Future<void> _rejectOrder(BuildContext context, WidgetRef ref, int assignmentId) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Reject Order', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text(
          'Are you sure you want to reject order #${order.orderNumber}?',
          style: GoogleFonts.inter(),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text('Cancel', style: GoogleFonts.inter()),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(
              backgroundColor: Theme.of(context).colorScheme.error,
              foregroundColor: Colors.white,
              shape: AppRadius.buttonShape,
            ),
            child: Text('Reject', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed == true && context.mounted) {
      try {
        await ref
            .read(availableOrdersNotifierProvider.notifier)
            .rejectOrder(assignmentId);

        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Order rejected', style: GoogleFonts.inter()),
              backgroundColor: context.appTheme.warningColor,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
            ),
          );
        }
      } catch (e) {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Error rejecting order: $e', style: GoogleFonts.inter()),
              backgroundColor: context.appTheme.errorColor,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
            ),
          );
        }
      }
    }
  }
}
