import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/status_badge.dart';
import 'driver_orders_notifier.dart';
import '../data/driver_order_model.dart';

class DeliveryDetailsPage extends ConsumerWidget {
  final int orderId;

  const DeliveryDetailsPage({super.key, required this.orderId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final orderAsync = ref.watch(orderDetailsProvider(orderId));
    final liveOrderAsync = ref.watch(driverOrderSocketProvider(orderId));
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      appBar: AppBar(
        backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
        title: Text(
          'Delivery Details',
          style: GoogleFonts.inter(fontWeight: FontWeight.w700, fontSize: 18),
        ),
      ),
      body: orderAsync.when(
        data: (initialOrder) {
          final liveOrder = liveOrderAsync.when(
            data: (value) => value,
            loading: () => null,
            error: (_, __) => null,
          );

          final order = liveOrder ?? initialOrder;
          if (order == null) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Container(
                    width: 80,
                    height: 80,
                    decoration: BoxDecoration(
                      color: appTheme.textTertiary.withAlpha(20),
                      borderRadius: AppRadius.radiusXl,
                    ),
                    child: Icon(Icons.error_outline_rounded, size: 40, color: appTheme.textTertiary),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'Order not found',
                    style: GoogleFonts.inter(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                      color: appTheme.textSecondary,
                    ),
                  ),
                ],
              ),
            );
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.fromLTRB(20, 4, 20, 40),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Order Header Card
                _buildOrderHeader(context, order, isDark, appTheme)
                    .animate()
                    .fadeIn(duration: 500.ms)
                    .slideY(begin: 0.1, end: 0, duration: 500.ms, curve: Curves.easeOutCubic),
                const SizedBox(height: 16),

                // Status Timeline
                _buildStatusTimeline(context, order, isDark, appTheme)
                    .animate()
                    .fadeIn(duration: 500.ms, delay: 100.ms)
                    .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 100.ms, curve: Curves.easeOutCubic),
                const SizedBox(height: 16),

                // Customer Information
                _buildCustomerCard(context, order, isDark, appTheme)
                    .animate()
                    .fadeIn(duration: 500.ms, delay: 150.ms)
                    .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 150.ms, curve: Curves.easeOutCubic),
                const SizedBox(height: 16),

                // Delivery Address
                _buildAddressCard(context, order, isDark, appTheme)
                    .animate()
                    .fadeIn(duration: 500.ms, delay: 200.ms)
                    .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 200.ms, curve: Curves.easeOutCubic),
                const SizedBox(height: 16),

                // Order Items
                if (order.items != null && order.items!.isNotEmpty) ...[
                  _buildItemsList(context, order, isDark, appTheme)
                      .animate()
                      .fadeIn(duration: 500.ms, delay: 250.ms)
                      .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 250.ms, curve: Curves.easeOutCubic),
                  const SizedBox(height: 16),
                ],

                // Notes
                if (order.notes != null && order.notes!.isNotEmpty) ...[
                  _buildNotesCard(context, order, isDark, appTheme)
                      .animate()
                      .fadeIn(duration: 500.ms, delay: 300.ms),
                  const SizedBox(height: 16),
                ],

                // Action Buttons
                _buildActionButtons(context, ref, order, isDark, appTheme)
                    .animate()
                    .fadeIn(duration: 500.ms, delay: 350.ms)
                    .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 350.ms, curve: Curves.easeOutCubic),
              ],
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stackTrace) => Center(
          child: Padding(
            padding: const EdgeInsets.all(32),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.error_outline_rounded, size: 64, color: appTheme.errorColor),
                const SizedBox(height: 16),
                Text(
                  'Error loading order',
                  style: GoogleFonts.inter(
                    fontSize: 18,
                    fontWeight: FontWeight.w600,
                    color: appTheme.textSecondary,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  error.toString(),
                  style: GoogleFonts.inter(fontSize: 14, color: appTheme.textTertiary),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  // ============================================================================
  // ORDER HEADER
  // ============================================================================
  Widget _buildOrderHeader(BuildContext context, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF1A1A2E), Color(0xFF16213E), Color(0xFF0F3460)],
        ),
        borderRadius: AppRadius.radiusXl,
        boxShadow: AppShadows.coloredShadow(const Color(0xFF1A1A2E), blur: 30),
      ),
      child: Padding(
        padding: const EdgeInsets.all(22),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Order #${order.orderNumber}',
                      style: GoogleFonts.inter(
                        fontSize: 22,
                        fontWeight: FontWeight.w800,
                        color: Colors.white,
                        letterSpacing: -0.5,
                      ),
                    ),
                    if (order.vendorName != null) ...[
                      const SizedBox(height: 4),
                      Text(
                        order.vendorName!,
                        style: GoogleFonts.inter(
                          fontSize: 14,
                          color: Colors.white.withAlpha(180),
                        ),
                      ),
                    ],
                  ],
                ),
                StatusBadgeSolid(
                  label: order.statusName,
                  statusCode: order.status,
                  size: StatusBadgeSize.medium,
                ),
              ],
            ),
            if (order.totalAmount != null) ...[
              const SizedBox(height: 18),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: Colors.white.withAlpha(15),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'Total Amount',
                      style: GoogleFonts.inter(
                        fontSize: 14,
                        color: Colors.white.withAlpha(180),
                      ),
                    ),
                    Text(
                      '\$${order.totalAmount!.toStringAsFixed(2)}',
                      style: GoogleFonts.inter(
                        fontSize: 22,
                        fontWeight: FontWeight.w800,
                        color: AppColors.accentLight,
                      ),
                    ),
                  ],
                ),
              ),
            ],
            if (order.createdAt != null) ...[
              const SizedBox(height: 10),
              Row(
                children: [
                  Icon(Icons.schedule_rounded, size: 14, color: Colors.white.withAlpha(140)),
                  const SizedBox(width: 6),
                  Text(
                    'Placed at ${_formatDateTime(order.createdAt!)}',
                    style: GoogleFonts.inter(
                      fontSize: 12,
                      color: Colors.white.withAlpha(140),
                    ),
                  ),
                ],
              ),
            ],
          ],
        ),
      ),
    );
  }

  // ============================================================================
  // STATUS TIMELINE
  // ============================================================================
  Widget _buildStatusTimeline(BuildContext context, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    final steps = [
      _TimelineStep('Ready', Icons.inventory_2_outlined, 4),
      _TimelineStep('Assigned', Icons.assignment_ind_outlined, 5),
      _TimelineStep('Picked Up', Icons.shopping_bag_outlined, 6),
      _TimelineStep('In Transit', Icons.local_shipping_outlined, 7),
      _TimelineStep('Delivered', Icons.check_circle_outline, 8),
    ];

    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Delivery Progress',
            style: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w700, letterSpacing: -0.3),
          ),
          const SizedBox(height: 16),
          Row(
            children: steps.asMap().entries.map((entry) {
              final index = entry.key;
              final step = entry.value;
              final isCompleted = order.status >= step.statusCode;
              final isCurrent = order.status == step.statusCode;
              final isLast = index == steps.length - 1;

              return Expanded(
                child: Row(
                  children: [
                    Expanded(
                      child: Column(
                        children: [
                          Container(
                            width: 36,
                            height: 36,
                            decoration: BoxDecoration(
                              color: isCompleted
                                  ? appTheme.successColor.withAlpha(isCurrent ? 255 : 40)
                                  : appTheme.textTertiary.withAlpha(20),
                              shape: BoxShape.circle,
                              border: isCurrent
                                  ? Border.all(color: appTheme.successColor, width: 2)
                                  : null,
                              boxShadow: isCurrent
                                  ? AppShadows.coloredShadow(appTheme.successColor)
                                  : [],
                            ),
                            child: Icon(
                              step.icon,
                              size: 16,
                              color: isCompleted
                                  ? (isCurrent ? Colors.white : appTheme.successColor)
                                  : appTheme.textTertiary,
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            step.label,
                            style: GoogleFonts.inter(
                              fontSize: 10,
                              fontWeight: isCurrent ? FontWeight.w700 : FontWeight.w500,
                              color: isCompleted ? null : appTheme.textTertiary,
                            ),
                            textAlign: TextAlign.center,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ],
                      ),
                    ),
                    if (!isLast)
                      Container(
                        width: 8,
                        height: 2,
                        color: isCompleted && order.status > step.statusCode
                            ? appTheme.successColor.withAlpha(80)
                            : appTheme.textTertiary.withAlpha(30),
                      ),
                  ],
                ),
              );
            }).toList(),
          ),
        ],
      ),
    );
  }

  // ============================================================================
  // CUSTOMER CARD
  // ============================================================================
  Widget _buildCustomerCard(BuildContext context, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.primary.withAlpha(20),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Icon(Icons.person_rounded, size: 20, color: Theme.of(context).colorScheme.primary),
              ),
              const SizedBox(width: 12),
              Text(
                'Customer',
                style: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w700, letterSpacing: -0.3),
              ),
            ],
          ),
          const SizedBox(height: 14),
          _DetailRow(
            icon: Icons.person_outline_rounded,
            label: 'Name',
            value: order.customerName ?? 'Customer',
            appTheme: appTheme,
          ),
          if (order.customerPhone != null) ...[
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: _DetailRow(
                    icon: Icons.phone_outlined,
                    label: 'Phone',
                    value: order.customerPhone!,
                    appTheme: appTheme,
                  ),
                ),
                Container(
                  decoration: BoxDecoration(
                    color: appTheme.successColor.withAlpha(20),
                    borderRadius: AppRadius.radiusMd,
                  ),
                  child: Material(
                    color: Colors.transparent,
                    child: InkWell(
                      onTap: () {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            content: Text('Call functionality coming soon', style: GoogleFonts.inter()),
                            behavior: SnackBarBehavior.floating,
                            shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
                          ),
                        );
                      },
                      borderRadius: AppRadius.radiusMd,
                      child: Padding(
                        padding: const EdgeInsets.all(10),
                        child: Icon(Icons.call_rounded, size: 20, color: appTheme.successColor),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ],
          if (order.customerEmail != null) ...[
            const SizedBox(height: 10),
            _DetailRow(
              icon: Icons.email_outlined,
              label: 'Email',
              value: order.customerEmail!,
              appTheme: appTheme,
            ),
          ],
        ],
      ),
    );
  }

  // ============================================================================
  // ADDRESS CARD
  // ============================================================================
  Widget _buildAddressCard(BuildContext context, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: appTheme.errorColor.withAlpha(20),
                      borderRadius: AppRadius.radiusMd,
                    ),
                    child: Icon(Icons.location_on_rounded, size: 20, color: appTheme.errorColor),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'Delivery Address',
                    style: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w700, letterSpacing: -0.3),
                  ),
                ],
              ),
              Container(
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.primary.withAlpha(20),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Material(
                  color: Colors.transparent,
                  child: InkWell(
                    onTap: () {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text('Map navigation coming soon', style: GoogleFonts.inter()),
                          behavior: SnackBarBehavior.floating,
                          shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
                        ),
                      );
                    },
                    borderRadius: AppRadius.radiusMd,
                    child: Padding(
                      padding: const EdgeInsets.all(10),
                      child: Icon(Icons.map_rounded, size: 20, color: Theme.of(context).colorScheme.primary),
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          if (order.buildingName != null)
            _DetailRow(icon: Icons.business_rounded, label: 'Building', value: order.buildingName!, appTheme: appTheme),
          if (order.floorNumber != null) ...[
            const SizedBox(height: 8),
            _DetailRow(icon: Icons.layers_rounded, label: 'Floor', value: order.floorNumber!, appTheme: appTheme),
          ],
          if (order.apartmentNumber != null) ...[
            const SizedBox(height: 8),
            _DetailRow(icon: Icons.door_front_door_rounded, label: 'Apartment', value: order.apartmentNumber!, appTheme: appTheme),
          ],
          if (order.deliveryAddress != null) ...[
            const SizedBox(height: 8),
            _DetailRow(icon: Icons.location_on_outlined, label: 'Address', value: order.deliveryAddress!, appTheme: appTheme),
          ],
        ],
      ),
    );
  }

  // ============================================================================
  // ITEMS LIST
  // ============================================================================
  Widget _buildItemsList(BuildContext context, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: appTheme.warningColor.withAlpha(20),
                  borderRadius: AppRadius.radiusMd,
                ),
                child: Icon(Icons.shopping_bag_rounded, size: 20, color: appTheme.warningColor),
              ),
              const SizedBox(width: 12),
              Text(
                'Order Items',
                style: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w700, letterSpacing: -0.3),
              ),
              const Spacer(),
              Text(
                '${order.items!.length} item${order.items!.length != 1 ? 's' : ''}',
                style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary),
              ),
            ],
          ),
          const SizedBox(height: 14),
          ...order.items!.asMap().entries.map((entry) {
            final index = entry.key;
            final item = entry.value;
            final isLast = index == order.items!.length - 1;

            return Column(
              children: [
                Row(
                  children: [
                    Container(
                      width: 48,
                      height: 48,
                      decoration: BoxDecoration(
                        color: appTheme.surfaceVariant,
                        borderRadius: AppRadius.radiusMd,
                      ),
                      child: item.imageUrl != null
                          ? ClipRRect(
                              borderRadius: AppRadius.radiusMd,
                              child: Image.network(
                                item.imageUrl!,
                                fit: BoxFit.cover,
                                errorBuilder: (_, __, ___) => Icon(Icons.image_rounded, color: appTheme.textTertiary),
                              ),
                            )
                          : Icon(Icons.shopping_bag_outlined, color: appTheme.textTertiary, size: 22),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            item.productName,
                            style: GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w600),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Qty: ${item.quantity}',
                            style: GoogleFonts.inter(fontSize: 12, color: appTheme.textSecondary),
                          ),
                        ],
                      ),
                    ),
                    Text(
                      '\$${(item.price * item.quantity).toStringAsFixed(2)}',
                      style: GoogleFonts.inter(fontSize: 15, fontWeight: FontWeight.w700),
                    ),
                  ],
                ),
                if (!isLast) ...[
                  const SizedBox(height: 10),
                  Divider(height: 1, color: appTheme.dividerColor),
                  const SizedBox(height: 10),
                ],
              ],
            );
          }),

          // Subtotals
          if (order.subtotal != null || order.deliveryFee != null) ...[
            const SizedBox(height: 12),
            Divider(color: appTheme.dividerColor),
            const SizedBox(height: 8),
            if (order.subtotal != null)
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Subtotal', style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary)),
                  Text('\$${order.subtotal!.toStringAsFixed(2)}', style: GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w600)),
                ],
              ),
            if (order.deliveryFee != null) ...[
              const SizedBox(height: 4),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Delivery Fee', style: GoogleFonts.inter(fontSize: 13, color: appTheme.textSecondary)),
                  Text('\$${order.deliveryFee!.toStringAsFixed(2)}', style: GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w600)),
                ],
              ),
            ],
            if (order.totalAmount != null) ...[
              const SizedBox(height: 8),
              Divider(color: appTheme.dividerColor),
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Total', style: GoogleFonts.inter(fontSize: 15, fontWeight: FontWeight.w700)),
                  Text(
                    '\$${order.totalAmount!.toStringAsFixed(2)}',
                    style: GoogleFonts.inter(fontSize: 17, fontWeight: FontWeight.w800, color: appTheme.successColor),
                  ),
                ],
              ),
            ],
          ],
        ],
      ),
    );
  }

  // ============================================================================
  // NOTES CARD
  // ============================================================================
  Widget _buildNotesCard(BuildContext context, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.note_alt_outlined, size: 20, color: appTheme.textSecondary),
              const SizedBox(width: 8),
              Text(
                'Notes',
                style: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w700, letterSpacing: -0.3),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Text(
            order.notes!,
            style: GoogleFonts.inter(fontSize: 14, color: appTheme.textSecondary, height: 1.5),
          ),
        ],
      ),
    );
  }

  // ============================================================================
  // ACTION BUTTONS
  // ============================================================================
  Widget _buildActionButtons(BuildContext context, WidgetRef ref, DriverOrder order, bool isDark, CustomThemeExtension appTheme) {
    if (order.status == 4 || order.status == 5) {
      return Row(
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
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.close_rounded, size: 20),
                  const SizedBox(width: 8),
                  Text('Reject', style: GoogleFonts.inter(fontWeight: FontWeight.w600, fontSize: 15)),
                ],
              ),
            ),
          ),
          const SizedBox(width: 12),
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
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.check_rounded, size: 20, color: Colors.white),
                        const SizedBox(width: 8),
                        Text(
                          'Accept Order',
                          style: GoogleFonts.inter(fontWeight: FontWeight.w700, fontSize: 15, color: Colors.white),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      );
    } else if (order.status == 4) {
      // Backend `OrderStatus.OutForDelivery = 4` — the only driver-actionable state.
      return _buildFullWidthGradientButton(
        icon: Icons.check_circle_rounded,
        label: 'Mark Delivered',
        gradient: AppColors.successGradient,
        shadowColor: AppColors.success,
        onTap: () => _completeDelivery(context, ref, order.id),
      );
    }
    return const SizedBox.shrink();
  }

  Widget _buildFullWidthGradientButton({
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
            padding: const EdgeInsets.symmetric(vertical: 16),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(icon, size: 22, color: Colors.white),
                const SizedBox(width: 10),
                Text(
                  label,
                  style: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w700, color: Colors.white),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  // ============================================================================
  // HELPERS
  // ============================================================================
  String _formatDateTime(DateTime dt) {
    final hour = dt.hour.toString().padLeft(2, '0');
    final minute = dt.minute.toString().padLeft(2, '0');
    final day = dt.day.toString().padLeft(2, '0');
    final month = dt.month.toString().padLeft(2, '0');
    return '$day/$month $hour:$minute';
  }

  Future<void> _acceptOrder(BuildContext context, WidgetRef ref, DriverOrder order) async {
    if (!context.mounted) return;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Accept Order', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text('Do you want to accept this order?', style: GoogleFonts.inter()),
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
          Navigator.pop(context);
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
            ),
          );
        }
      }
    }
  }

  Future<void> _rejectOrder(BuildContext context, WidgetRef ref, int assignmentId) async {
    if (!context.mounted) return;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Reject Order', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text('Are you sure you want to reject this order?', style: GoogleFonts.inter()),
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
          Navigator.pop(context);
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Order rejected', style: GoogleFonts.inter()),
              backgroundColor: context.appTheme.warningColor,
              behavior: SnackBarBehavior.floating,
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
            ),
          );
        }
      }
    }
  }

  Future<void> _updateStatus(BuildContext context, WidgetRef ref, int orderId, DeliveryStatus newStatus) async {
    if (!context.mounted) return;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Update Status', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text('Mark order as ${newStatus.displayName}?', style: GoogleFonts.inter()),
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

        ref.invalidate(orderDetailsProvider(orderId));

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
            ),
          );
        }
      }
    }
  }

  Future<void> _completeDelivery(BuildContext context, WidgetRef ref, int orderId) async {
    if (!context.mounted) return;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Complete Delivery', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text('Are you sure you want to mark this delivery as completed?', style: GoogleFonts.inter()),
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
          Navigator.pop(context);
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
            ),
          );
        }
      }
    }
  }
}

// ============================================================================
// HELPER WIDGETS
// ============================================================================
class _DetailRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final CustomThemeExtension appTheme;

  const _DetailRow({
    required this.icon,
    required this.label,
    required this.value,
    required this.appTheme,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 18, color: appTheme.textTertiary),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: GoogleFonts.inter(fontSize: 11, color: appTheme.textTertiary, fontWeight: FontWeight.w500),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w500),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _TimelineStep {
  final String label;
  final IconData icon;
  final int statusCode;
  const _TimelineStep(this.label, this.icon, this.statusCode);
}
