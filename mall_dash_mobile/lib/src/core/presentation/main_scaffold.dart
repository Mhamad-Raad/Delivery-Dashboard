import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'dart:developer' as developer;
import '../design/design_system.dart';
import '../theme/custom_theme_extension.dart';
import '../widgets/loading_indicator.dart';
import '../widgets/error_state.dart';
import '../../features/home/presentation/home_page.dart';
import '../../features/orders/presentation/orders_page.dart';
import '../../features/profile/presentation/profile_page.dart';
import '../../features/profile/presentation/user_profile_notifier.dart';
import '../../features/notifications/presentation/notifications_page.dart';
import '../../features/settings/presentation/settings_page.dart';
import '../../features/driver/presentation/driver_home_page.dart';
import '../../features/auth/presentation/auth_notifier.dart';
import '../../features/order/data/customer_order_stream_service.dart';
import '../constants/user_role.dart';

class MainScaffold extends ConsumerStatefulWidget {
  const MainScaffold({super.key});

  @override
  ConsumerState<MainScaffold> createState() => _MainScaffoldState();
}

class _MainScaffoldState extends ConsumerState<MainScaffold> {
  int _currentIndex = 1;
  bool _customerStreamStarted = false;

  @override
  void dispose() {
    if (_customerStreamStarted) {
      ref.read(customerOrderStreamServiceProvider).stop();
    }
    super.dispose();
  }

  void _ensureCustomerStreamStarted() {
    if (_customerStreamStarted) return;
    _customerStreamStarted = true;
    ref.read(customerOrderStreamServiceProvider).start();
  }

  @override
  Widget build(BuildContext context) {
    final profileAsync = ref.watch(userProfileProvider);

    return profileAsync.when(
      data: (profile) {
        final isDriver = UserRole.isDriver(profile.role);
        developer.log(
          'MainScaffold: Role: ${profile.role}, IsDriver: $isDriver',
          name: 'MainScaffold',
        );
        if (isDriver) {
          return const DriverHomePage();
        } else {
          _ensureCustomerStreamStarted();
          return _buildCustomerScaffold(profile.firstName);
        }
      },
      loading: () => const Scaffold(body: LoadingIndicator()),
      error: (error, _) {
        // If the error is authentication-related, redirect to login
        final errorMsg = error.toString().toLowerCase();
        if (errorMsg.contains('authentication required') || errorMsg.contains('401')) {
          developer.log('Auth error in MainScaffold - triggering logout', name: 'MainScaffold');
          // Schedule the state change for next frame to avoid setState during build
          Future.microtask(() {
            ref.read(authNotifierProvider.notifier).logout();
          });
          return const Scaffold(body: LoadingIndicator(message: 'Redirecting to login...'));
        }
        return Scaffold(
          body: ErrorState(
            message: error.toString(),
            onRetry: () => ref.read(userProfileProvider.notifier).refresh(),
          ),
        );
      },
    );
  }

  Widget _buildCustomerScaffold(String firstName) {
    final List<Widget> pages = const [OrdersPage(), HomePage(), ProfilePage()];
    final isDark = context.isDarkMode;

    return Scaffold(
      extendBody: true,
      appBar: AppBar(
        backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Mall Dash',
              style: GoogleFonts.inter(
                fontSize: 22,
                fontWeight: FontWeight.w800,
                letterSpacing: -0.5,
              ),
            ),
          ],
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
      ),
      body: IndexedStack(index: _currentIndex, children: pages),
      bottomNavigationBar: _buildPremiumBottomNav(isDark),
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
      _NavItem(Icons.receipt_long_outlined, Icons.receipt_long_rounded, 'Orders'),
      _NavItem(Icons.home_outlined, Icons.home_rounded, 'Home'),
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
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
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
                      horizontal: isSelected ? 20 : 16,
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
                          const SizedBox(width: 8),
                          Text(
                            item.label,
                            style: GoogleFonts.inter(
                              fontSize: 13,
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
