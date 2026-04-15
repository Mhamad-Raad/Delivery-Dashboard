import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/loading_indicator.dart';
import 'driver_shift_notifier.dart';
import 'driver_orders_notifier.dart';
import 'available_orders_page.dart';
import 'active_deliveries_page.dart';
import 'driver_profile_page.dart';
import '../../notifications/presentation/notifications_page.dart';
import '../../settings/presentation/settings_page.dart';

class DriverHomePage extends ConsumerStatefulWidget {
  const DriverHomePage({super.key});

  @override
  ConsumerState<DriverHomePage> createState() => _DriverHomePageState();
}

class _DriverHomePageState extends ConsumerState<DriverHomePage> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    final isDark = context.isDarkMode;

    final List<Widget> pages = const [
      _DriverDashboardContent(),
      AvailableOrdersPage(),
      ActiveDeliveriesPage(),
      DriverProfilePage(),
    ];

    return Scaffold(
      extendBody: true,
      appBar: _buildAppBar(isDark),
      body: IndexedStack(index: _currentIndex, children: pages),
      bottomNavigationBar: _buildPremiumBottomNav(isDark),
    );
  }

  PreferredSizeWidget _buildAppBar(bool isDark) {
    return AppBar(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      title: Text(
        'Mall Dash',
        style: GoogleFonts.inter(
          fontSize: 22,
          fontWeight: FontWeight.w800,
          letterSpacing: -0.5,
        ),
      ),
      actions: [
        _buildIconAction(
          icon: Icons.notifications_outlined,
          onTap: () => Navigator.push(
            context,
            MaterialPageRoute(builder: (_) => const NotificationsPage()),
          ),
        ),
        _buildIconAction(
          icon: Icons.settings_outlined,
          onTap: () => Navigator.push(
            context,
            MaterialPageRoute(builder: (_) => const SettingsPage()),
          ),
        ),
        const SizedBox(width: 8),
      ],
    );
  }

  Widget _buildIconAction({required IconData icon, required VoidCallback onTap}) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 2),
      child: IconButton(
        onPressed: onTap,
        icon: Icon(icon, size: 22),
        style: IconButton.styleFrom(
          backgroundColor: Theme.of(context).colorScheme.surfaceContainerHighest.withAlpha(120),
          shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
          fixedSize: const Size(40, 40),
        ),
      ),
    );
  }

  Widget _buildPremiumBottomNav(bool isDark) {
    final items = [
      _NavItem(Icons.dashboard_outlined, Icons.dashboard_rounded, 'Home'),
      _NavItem(Icons.shopping_bag_outlined, Icons.shopping_bag_rounded, 'Orders'),
      _NavItem(Icons.local_shipping_outlined, Icons.local_shipping_rounded, 'Active'),
      _NavItem(Icons.person_outline_rounded, Icons.person_rounded, 'Profile'),
    ];

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 0, 16, 16),
      decoration: BoxDecoration(
        color: isDark
            ? AppColors.cardBackgroundDark.withAlpha(230)
            : Colors.white.withAlpha(240),
        borderRadius: AppRadius.radiusXl,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(isDark ? 40 : 15),
            blurRadius: 30,
            offset: const Offset(0, 10),
          ),
        ],
        border: Border.all(
          color: isDark ? Colors.white.withAlpha(10) : Colors.black.withAlpha(5),
        ),
      ),
      child: ClipRRect(
        borderRadius: AppRadius.radiusXl,
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 20, sigmaY: 20),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 8),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: items.asMap().entries.map((entry) {
                final index = entry.key;
                final item = entry.value;
                final isSelected = _currentIndex == index;

                return GestureDetector(
                  onTap: () => setState(() => _currentIndex = index),
                  behavior: HitTestBehavior.opaque,
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 250),
                    curve: Curves.easeOutCubic,
                    padding: EdgeInsets.symmetric(
                      horizontal: isSelected ? 16 : 12,
                      vertical: 10,
                    ),
                    decoration: BoxDecoration(
                      color: isSelected
                          ? Theme.of(context).colorScheme.primary.withAlpha(isDark ? 40 : 20)
                          : Colors.transparent,
                      borderRadius: AppRadius.radiusMd,
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(
                          isSelected ? item.activeIcon : item.icon,
                          color: isSelected
                              ? Theme.of(context).colorScheme.primary
                              : (isDark ? AppColors.textTertiaryDark : AppColors.textTertiary),
                          size: 22,
                        ),
                        if (isSelected) ...[
                          const SizedBox(width: 6),
                          Text(
                            item.label,
                            style: GoogleFonts.inter(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: Theme.of(context).colorScheme.primary,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                );
              }).toList(),
            ),
          ),
        ),
      ),
    );
  }
}

class _NavItem {
  final IconData icon;
  final IconData activeIcon;
  final String label;
  const _NavItem(this.icon, this.activeIcon, this.label);
}

// ============================================================================
// DRIVER DASHBOARD CONTENT (Tab 0 in IndexedStack)
// ============================================================================
class _DriverDashboardContent extends ConsumerWidget {
  const _DriverDashboardContent();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final shiftState = ref.watch(driverShiftNotifierProvider);
    final queuePositionAsync = ref.watch(queuePositionProvider);
    final availableOrdersState = ref.watch(availableOrdersNotifierProvider);
    final activeDeliveriesState = ref.watch(activeDeliveriesNotifierProvider);
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;

    return RefreshIndicator(
      onRefresh: () async {
        await ref.read(driverShiftNotifierProvider.notifier).refresh();
        await ref.read(availableOrdersNotifierProvider.notifier).refresh();
        await ref.read(activeDeliveriesNotifierProvider.notifier).refresh();
      },
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 120),
        children: [
          // Premium Shift Toggle Card
          _buildShiftCard(context, ref, shiftState, isDark, appTheme)
              .animate()
              .fadeIn(duration: 500.ms)
              .slideY(begin: 0.1, end: 0, duration: 500.ms, curve: Curves.easeOutCubic),
          const SizedBox(height: 16),

          // Queue Position
          _buildQueuePosition(context, queuePositionAsync, isDark, appTheme)
              .animate()
              .fadeIn(duration: 500.ms, delay: 100.ms)
              .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 100.ms, curve: Curves.easeOutCubic),
          const SizedBox(height: 20),

          // Stats Row
          _buildStatsRow(context, availableOrdersState, activeDeliveriesState, isDark, appTheme)
              .animate()
              .fadeIn(duration: 500.ms, delay: 200.ms)
              .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 200.ms, curve: Curves.easeOutCubic),
          const SizedBox(height: 20),

          // Quick Actions
          Text(
            'Quick Actions',
            style: GoogleFonts.inter(
              fontSize: 18,
              fontWeight: FontWeight.w700,
              letterSpacing: -0.3,
            ),
          ).animate().fadeIn(duration: 500.ms, delay: 300.ms),
          const SizedBox(height: 12),
          _buildQuickActions(context, ref, availableOrdersState, activeDeliveriesState, isDark, appTheme)
              .animate()
              .fadeIn(duration: 500.ms, delay: 350.ms)
              .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 350.ms, curve: Curves.easeOutCubic),
        ],
      ),
    );
  }

  Widget _buildShiftCard(
    BuildContext context,
    WidgetRef ref,
    AsyncValue shiftState,
    bool isDark,
    CustomThemeExtension appTheme,
  ) {
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
        padding: const EdgeInsets.all(24),
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
                      'Shift Status',
                      style: GoogleFonts.inter(
                        fontSize: 14,
                        fontWeight: FontWeight.w500,
                        color: Colors.white.withAlpha(180),
                      ),
                    ),
                    const SizedBox(height: 4),
                    shiftState.when(
                      data: (shift) => Text(
                        shift.isActive ? 'On Duty' : 'Off Duty',
                        style: GoogleFonts.inter(
                          fontSize: 28,
                          fontWeight: FontWeight.w800,
                          color: Colors.white,
                          letterSpacing: -0.5,
                        ),
                      ),
                      loading: () => Text(
                        'Loading...',
                        style: GoogleFonts.inter(
                          fontSize: 28,
                          fontWeight: FontWeight.w800,
                          color: Colors.white,
                        ),
                      ),
                      error: (_, __) => Text(
                        'Error',
                        style: GoogleFonts.inter(
                          fontSize: 28,
                          fontWeight: FontWeight.w800,
                          color: AppColors.errorLight,
                        ),
                      ),
                    ),
                  ],
                ),
                // Premium animated toggle
                shiftState.when(
                  data: (shift) => _PremiumShiftToggle(
                    isActive: shift.isActive,
                    isLoading: false,
                    onToggle: () async {
                      if (shift.isActive) {
                        await _endShift(context, ref);
                      } else {
                        await _startShift(context, ref);
                      }
                    },
                  ),
                  loading: () => _PremiumShiftToggle(
                    isActive: false,
                    isLoading: true,
                    onToggle: () {},
                  ),
                  error: (_, __) => _PremiumShiftToggle(
                    isActive: false,
                    isLoading: false,
                    onToggle: () => ref.read(driverShiftNotifierProvider.notifier).refresh(),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            shiftState.when(
              data: (shift) {
                if (shift.isActive && shift.startTime != null) {
                  return Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                    decoration: BoxDecoration(
                      color: Colors.white.withAlpha(20),
                      borderRadius: AppRadius.radiusMd,
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.access_time_rounded, size: 16, color: AppColors.accentLight),
                        const SizedBox(width: 8),
                        Text(
                          'Started at ${_formatTime(shift.startTime!)}',
                          style: GoogleFonts.inter(
                            fontSize: 13,
                            fontWeight: FontWeight.w500,
                            color: Colors.white.withAlpha(200),
                          ),
                        ),
                      ],
                    ),
                  );
                }
                return const SizedBox.shrink();
              },
              loading: () => const SizedBox.shrink(),
              error: (_, __) => const SizedBox.shrink(),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildQueuePosition(
    BuildContext context,
    AsyncValue queuePositionAsync,
    bool isDark,
    CustomThemeExtension appTheme,
  ) {
    return queuePositionAsync.when(
      data: (position) {
        if (position == null) return const SizedBox.shrink();
        return Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: isDark ? appTheme.cardBackground : Colors.white,
            borderRadius: AppRadius.radiusXl,
            boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
          ),
          child: Row(
            children: [
              Container(
                width: 64,
                height: 64,
                decoration: BoxDecoration(
                  gradient: AppColors.accentGradient,
                  borderRadius: AppRadius.radiusLg,
                  boxShadow: AppShadows.coloredShadow(AppColors.accent),
                ),
                child: Center(
                  child: Text(
                    '#${position.position}',
                    style: GoogleFonts.inter(
                      fontSize: 24,
                      fontWeight: FontWeight.w900,
                      color: Colors.white,
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Queue Position',
                      style: GoogleFonts.inter(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                        letterSpacing: -0.3,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      position.totalDrivers > 0
                          ? '${position.totalDrivers} drivers in queue'
                          : 'Waiting for assignment',
                      style: GoogleFonts.inter(
                        fontSize: 13,
                        color: appTheme.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: AppColors.accent.withAlpha(25),
                  borderRadius: AppRadius.radiusPill,
                ),
                child: Text(
                  'Active',
                  style: GoogleFonts.inter(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: AppColors.accent,
                  ),
                ),
              ),
            ],
          ),
        );
      },
      loading: () => Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: isDark ? appTheme.cardBackground : Colors.white,
          borderRadius: AppRadius.radiusXl,
          boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        ),
        child: const LoadingIndicator(),
      ),
      error: (_, __) => const SizedBox.shrink(),
    );
  }

  Widget _buildStatsRow(
    BuildContext context,
    AsyncValue availableOrdersState,
    AsyncValue activeDeliveriesState,
    bool isDark,
    CustomThemeExtension appTheme,
  ) {
    final availableCount = availableOrdersState.when(
      data: (orders) => (orders as List).length,
      loading: () => 0,
      error: (_, __) => 0,
    );
    final activeCount = activeDeliveriesState.when(
      data: (deliveries) => (deliveries as List).length,
      loading: () => 0,
      error: (_, __) => 0,
    );

    return Row(
      children: [
        Expanded(
          child: _StatCard(
            icon: Icons.shopping_bag_outlined,
            label: 'Available',
            value: '$availableCount',
            color: appTheme.warningColor,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: _StatCard(
            icon: Icons.local_shipping_outlined,
            label: 'Active',
            value: '$activeCount',
            color: appTheme.infoColor,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: _StatCard(
            icon: Icons.check_circle_outline,
            label: 'Today',
            value: '0',
            color: appTheme.successColor,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
      ],
    );
  }

  Widget _buildQuickActions(
    BuildContext context,
    WidgetRef ref,
    AsyncValue availableOrdersState,
    AsyncValue activeDeliveriesState,
    bool isDark,
    CustomThemeExtension appTheme,
  ) {
    final availableCount = availableOrdersState.when(
      data: (orders) => (orders as List).length,
      loading: () => 0,
      error: (_, __) => 0,
    );
    final activeCount = activeDeliveriesState.when(
      data: (deliveries) => (deliveries as List).length,
      loading: () => 0,
      error: (_, __) => 0,
    );

    return Column(
      children: [
        _QuickActionTile(
          icon: Icons.shopping_bag_outlined,
          title: 'Available Orders',
          subtitle: '$availableCount order${availableCount != 1 ? 's' : ''} waiting',
          color: appTheme.warningColor,
          isDark: isDark,
          appTheme: appTheme,
          onTap: () {},
          badge: availableCount > 0 ? '$availableCount' : null,
        ),
        const SizedBox(height: 10),
        _QuickActionTile(
          icon: Icons.local_shipping_outlined,
          title: 'Active Deliveries',
          subtitle: '$activeCount active deliver${activeCount != 1 ? 'ies' : 'y'}',
          color: appTheme.infoColor,
          isDark: isDark,
          appTheme: appTheme,
          onTap: () {},
          badge: activeCount > 0 ? '$activeCount' : null,
        ),
      ],
    );
  }

  String _formatTime(DateTime time) {
    final hour = time.hour.toString().padLeft(2, '0');
    final minute = time.minute.toString().padLeft(2, '0');
    return '$hour:$minute';
  }

  Future<void> _startShift(BuildContext context, WidgetRef ref) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('Start Shift', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text(
          'Are you sure you want to start your shift? You will start receiving delivery notifications.',
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
              backgroundColor: context.appTheme.successColor,
              foregroundColor: Colors.white,
              shape: AppRadius.buttonShape,
            ),
            child: Text('Start', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed == true && context.mounted) {
      await ref.read(driverShiftNotifierProvider.notifier).startShift();

      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Shift started successfully', style: GoogleFonts.inter()),
            backgroundColor: context.appTheme.successColor,
            behavior: SnackBarBehavior.floating,
            shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
          ),
        );
      }
    }
  }

  Future<void> _endShift(BuildContext context, WidgetRef ref) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: AppRadius.dialogShape,
        title: Text('End Shift', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: Text(
          'Are you sure you want to end your shift? You will stop receiving delivery notifications.',
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
              backgroundColor: context.appTheme.errorColor,
              foregroundColor: Colors.white,
              shape: AppRadius.buttonShape,
            ),
            child: Text('End', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed == true && context.mounted) {
      await ref.read(driverShiftNotifierProvider.notifier).endShift();

      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Shift ended successfully', style: GoogleFonts.inter()),
            backgroundColor: context.appTheme.warningColor,
            behavior: SnackBarBehavior.floating,
            shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
          ),
        );
      }
    }
  }
}

// ============================================================================
// PREMIUM SHIFT TOGGLE
// ============================================================================
class _PremiumShiftToggle extends StatelessWidget {
  final bool isActive;
  final bool isLoading;
  final VoidCallback onToggle;

  const _PremiumShiftToggle({
    required this.isActive,
    required this.isLoading,
    required this.onToggle,
  });

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return const SizedBox(
        width: 72,
        height: 40,
        child: Center(
          child: SizedBox(
            width: 24,
            height: 24,
            child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
          ),
        ),
      );
    }

    return GestureDetector(
      onTap: onToggle,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 350),
        curve: Curves.easeOutCubic,
        width: 72,
        height: 40,
        decoration: BoxDecoration(
          borderRadius: AppRadius.radiusPill,
          color: isActive
              ? AppColors.success.withAlpha(200)
              : Colors.white.withAlpha(30),
          border: Border.all(
            color: isActive
                ? AppColors.success
                : Colors.white.withAlpha(60),
            width: 2,
          ),
          boxShadow: isActive
              ? [BoxShadow(color: AppColors.success.withAlpha(80), blurRadius: 12, spreadRadius: -2)]
              : [],
        ),
        child: AnimatedAlign(
          duration: const Duration(milliseconds: 350),
          curve: Curves.easeOutCubic,
          alignment: isActive ? Alignment.centerRight : Alignment.centerLeft,
          child: Container(
            margin: const EdgeInsets.all(4),
            width: 30,
            height: 30,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withAlpha(40),
                  blurRadius: 6,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Center(
              child: Icon(
                isActive ? Icons.flash_on_rounded : Icons.flash_off_rounded,
                size: 16,
                color: isActive ? AppColors.success : AppColors.textTertiary,
              ),
            ),
          ),
        ),
      ),
    );
  }
}

// ============================================================================
// STAT CARD
// ============================================================================
class _StatCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color color;
  final bool isDark;
  final CustomThemeExtension appTheme;

  const _StatCard({
    required this.icon,
    required this.label,
    required this.value,
    required this.color,
    required this.isDark,
    required this.appTheme,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        children: [
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: color.withAlpha(25),
              borderRadius: AppRadius.radiusMd,
            ),
            child: Icon(icon, color: color, size: 22),
          ),
          const SizedBox(height: 12),
          Text(
            value,
            style: GoogleFonts.inter(
              fontSize: 24,
              fontWeight: FontWeight.w800,
              letterSpacing: -0.5,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: GoogleFonts.inter(
              fontSize: 12,
              color: appTheme.textSecondary,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

// ============================================================================
// QUICK ACTION TILE
// ============================================================================
class _QuickActionTile extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;
  final Color color;
  final bool isDark;
  final CustomThemeExtension appTheme;
  final VoidCallback onTap;
  final String? badge;

  const _QuickActionTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.color,
    required this.isDark,
    required this.appTheme,
    required this.onTap,
    this.badge,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: AppRadius.radiusLg,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                Container(
                  width: 48,
                  height: 48,
                  decoration: BoxDecoration(
                    color: color.withAlpha(25),
                    borderRadius: AppRadius.radiusMd,
                  ),
                  child: Icon(icon, color: color, size: 24),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        title,
                        style: GoogleFonts.inter(
                          fontSize: 15,
                          fontWeight: FontWeight.w600,
                          letterSpacing: -0.2,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        subtitle,
                        style: GoogleFonts.inter(
                          fontSize: 13,
                          color: appTheme.textSecondary,
                        ),
                      ),
                    ],
                  ),
                ),
                if (badge != null)
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: color.withAlpha(25),
                      borderRadius: AppRadius.radiusPill,
                    ),
                    child: Text(
                      badge!,
                      style: GoogleFonts.inter(
                        fontSize: 13,
                        fontWeight: FontWeight.w700,
                        color: color,
                      ),
                    ),
                  ),
                const SizedBox(width: 4),
                Icon(Icons.chevron_right_rounded, color: appTheme.textTertiary, size: 22),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
