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

class ActiveDeliveriesPage extends ConsumerWidget {
  const ActiveDeliveriesPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final deliveriesState = ref.watch(activeDeliveriesNotifierProvider);
    final appTheme = context.appTheme;

    return deliveriesState.when(
      data: (deliveries) {
        if (deliveries.isEmpty) {
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
                      color: appTheme.infoColor.withAlpha(20),
                      borderRadius: AppRadius.radiusXl,
                    ),
                    child: Icon(
                      Icons.local_shipping_outlined,
                      size: 40,
                      color: appTheme.infoColor,
                    ),
                  ),
                  const SizedBox(height: 20),
                  Text(
                    'No Active Deliveries',
                    style: GoogleFonts.inter(
                      fontSize: 20,
                      fontWeight: FontWeight.w700,
                      letterSpacing: -0.3,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Accept an order to start delivering',
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
            await ref.read(activeDeliveriesNotifierProvider.notifier).refresh();
          },
          child: ListView.builder(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 120),
            itemCount: deliveries.length,
            itemBuilder: (context, index) {
              final delivery = deliveries[index];
              return _PremiumDeliveryCard(delivery: delivery)
                  .animate()
                  .fadeIn(duration: 400.ms, delay: Duration(milliseconds: index * 80))
                  .slideX(begin: 0.05, end: 0, duration: 400.ms, delay: Duration(milliseconds: index * 80));
            },
          ),
        );
      },
      loading: () => const LoadingIndicator(),
      error: (error, stackTrace) => ErrorState(
        title: 'Error loading deliveries',
        message: error.toString(),
        onRetry: () {
          ref.read(activeDeliveriesNotifierProvider.notifier).refresh();
        },
      ),
    );
  }
}

class _PremiumDeliveryCard extends ConsumerWidget {
  final DriverOrder delivery;

  const _PremiumDeliveryCard({required this.delivery});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final appTheme = context.appTheme;
    final isDark = context.isDarkMode;

    // Progress based on status
    final progress = delivery.status == 6 ? 0.5 : delivery.status == 7 ? 0.75 : 0.25;
    final statusColor = delivery.status == 7 ? appTheme.infoColor : appTheme.warningColor;

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(
          color: statusColor.withAlpha(40),
          width: 1,
        ),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () {
            Navigator.push(
              context,
              MaterialPageRoute(
                builder: (context) => DeliveryDetailsPage(orderId: delivery.id),
              ),
            );
          },
          borderRadius: AppRadius.radiusXl,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Status progress bar
              ClipRRect(
                borderRadius: const BorderRadius.only(
                  topLeft: Radius.circular(20),
                  topRight: Radius.circular(20),
                ),
                child: LinearProgressIndicator(
                  value: progress,
                  backgroundColor: statusColor.withAlpha(20),
                  valueColor: AlwaysStoppedAnimation<Color>(statusColor),
                  minHeight: 4,
                ),
              ),

              Padding(
                padding: const EdgeInsets.all(18),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Header
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Row(
                          children: [
                            Container(
                              padding: const EdgeInsets.all(8),
                              decoration: BoxDecoration(
                                color: statusColor.withAlpha(20),
                                borderRadius: AppRadius.radiusMd,
                              ),
                              child: Icon(
                                delivery.status == 7
                                    ? Icons.local_shipping_rounded
                                    : Icons.inventory_2_rounded,
                                size: 20,
                                color: statusColor,
                              ),
                            ),
                            const SizedBox(width: 10),
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  '#${delivery.orderNumber}',
                                  style: GoogleFonts.inter(
                                    fontSize: 16,
                                    fontWeight: FontWeight.w700,
                                    letterSpacing: -0.3,
                                  ),
                                ),
                                if (delivery.vendorName != null)
                                  Text(
                                    delivery.vendorName!,
                                    style: GoogleFonts.inter(
                                      fontSize: 12,
                                      color: appTheme.textSecondary,
                                    ),
                                  ),
                              ],
                            ),
                          ],
                        ),
                        StatusBadgeSolid(
                          label: delivery.statusName,
                          statusCode: delivery.status,
                          size: StatusBadgeSize.small,
                        ),
                      ],
                    ),
                    const SizedBox(height: 14),

                    // Customer info
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: isDark
                            ? Colors.white.withAlpha(5)
                            : appTheme.surfaceVariant.withAlpha(120),
                        borderRadius: AppRadius.radiusMd,
                      ),
                      child: Column(
                        children: [
                          Row(
                            children: [
                              Icon(Icons.person_outline_rounded, size: 16, color: appTheme.textTertiary),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  delivery.customerName ?? 'Customer',
                                  style: GoogleFonts.inter(fontSize: 13, fontWeight: FontWeight.w500),
                                ),
                              ),
                              if (delivery.customerPhone != null)
                                Container(
                                  padding: const EdgeInsets.all(6),
                                  decoration: BoxDecoration(
                                    color: appTheme.successColor.withAlpha(20),
                                    borderRadius: AppRadius.radiusSm,
                                  ),
                                  child: Icon(Icons.phone_rounded, size: 16, color: appTheme.successColor),
                                ),
                            ],
                          ),
                          const SizedBox(height: 8),
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
                                  _formatAddress(delivery),
                                  style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),

                    // Total
                    if (delivery.totalAmount != null) ...[
                      const SizedBox(height: 12),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            'Total Amount',
                            style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
                          ),
                          Text(
                            '\$${delivery.totalAmount!.toStringAsFixed(2)}',
                            style: GoogleFonts.inter(
                              fontSize: 17,
                              fontWeight: FontWeight.w800,
                              color: appTheme.successColor,
                              letterSpacing: -0.3,
                            ),
                          ),
                        ],
                      ),
                    ],

                    // Action Button
                    const SizedBox(height: 14),
                    if (delivery.status == 6) ...[
                      _buildGradientButton(
                        icon: Icons.local_shipping_rounded,
                        label: 'Mark In Transit',
                        gradient: LinearGradient(
                          colors: [appTheme.infoColor, appTheme.infoColor.withAlpha(200)],
                        ),
                        shadowColor: appTheme.infoColor,
                        onTap: () => _updateStatus(
                          context, ref, delivery.id,
                          DeliveryStatus.inTransit, 'Mark In Transit',
                        ),
                      ),
                    ] else if (delivery.status == 7) ...[
                      _buildGradientButton(
                        icon: Icons.check_circle_rounded,
                        label: 'Mark Delivered',
                        gradient: AppColors.successGradient,
                        shadowColor: AppColors.success,
                        onTap: () => _completeDelivery(context, ref, delivery.id),
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildGradientButton({
    required IconData icon,
    required String label,
    required Gradient gradient,
    required Color shadowColor,
    required VoidCallback onTap,
  }) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        gradient: gradient,
        borderRadius: AppRadius.radiusMd,
        boxShadow: AppShadows.coloredShadow(shadowColor),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: AppRadius.radiusMd,
          child: Padding(
            padding: const EdgeInsets.symmetric(vertical: 14),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(icon, size: 20, color: Colors.white),
                const SizedBox(width: 8),
                Text(
                  label,
                  style: GoogleFonts.inter(
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _formatAddress(DriverOrder delivery) {
    final parts = <String>[];
    if (delivery.apartmentNumber != null) parts.add('Apt ${delivery.apartmentNumber}');
    if (delivery.floorNumber != null) parts.add('Floor ${delivery.floorNumber}');
    if (delivery.buildingName != null) parts.add(delivery.buildingName!);
    if (delivery.deliveryAddress != null) parts.add(delivery.deliveryAddress!);
    return parts.isEmpty ? 'No address provided' : parts.join(', ');
  }

  Future<void> _updateStatus(
    BuildContext context,
    WidgetRef ref,
    int orderId,
    DeliveryStatus newStatus,
    String actionName,
  ) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text(actionName, style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text(
          'Update order #${delivery.orderNumber} to ${newStatus.displayName}?',
          style: GoogleFonts.inter(),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text('Cancel', style: GoogleFonts.inter()),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(shape: AppRadius.buttonShape),
            child: Text('Confirm', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed == true && context.mounted) {
      try {
        await ref
            .read(activeDeliveriesNotifierProvider.notifier)
            .updateOrderStatus(orderId, newStatus);

        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Status updated to ${newStatus.displayName}', style: GoogleFonts.inter()),
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
              content: Text('Error updating status: $e', style: GoogleFonts.inter()),
              backgroundColor: context.appTheme.errorColor,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
            ),
          );
        }
      }
    }
  }

  Future<void> _completeDelivery(
    BuildContext context,
    WidgetRef ref,
    int orderId,
  ) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Complete Delivery', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text(
          'Are you sure you want to mark order #${delivery.orderNumber} as delivered?',
          style: GoogleFonts.inter(),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text('Cancel', style: GoogleFonts.inter()),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            style: ElevatedButton.styleFrom(shape: AppRadius.buttonShape),
            child: Text('Complete', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed == true && context.mounted) {
      try {
        await ref
            .read(activeDeliveriesNotifierProvider.notifier)
            .completeDelivery(orderId);

        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Delivery completed successfully!', style: GoogleFonts.inter()),
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
              content: Text('Error completing delivery: $e', style: GoogleFonts.inter()),
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
