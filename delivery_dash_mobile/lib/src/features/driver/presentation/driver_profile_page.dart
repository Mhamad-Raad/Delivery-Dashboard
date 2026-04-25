import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/loading_indicator.dart';
import '../../profile/presentation/user_profile_notifier.dart';
import '../../profile/presentation/view_profile_page.dart';
import '../../auth/presentation/auth_notifier.dart';
import 'driver_shift_notifier.dart';

class DriverProfilePage extends ConsumerWidget {
  const DriverProfilePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    final shiftState = ref.watch(driverShiftNotifierProvider);
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 120),
      children: [
        // Premium Gradient Header Card
        _buildProfileHeader(context, profileAsync, isDark, appTheme)
            .animate()
            .fadeIn(duration: 500.ms)
            .slideY(begin: 0.1, end: 0, duration: 500.ms, curve: Curves.easeOutCubic),
        const SizedBox(height: 20),

        // Stats Grid
        _buildStatsGrid(context, isDark, appTheme)
            .animate()
            .fadeIn(duration: 500.ms, delay: 100.ms)
            .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 100.ms, curve: Curves.easeOutCubic),
        const SizedBox(height: 20),

        // Shift Status Card
        _buildShiftStatus(context, shiftState, isDark, appTheme)
            .animate()
            .fadeIn(duration: 500.ms, delay: 150.ms)
            .slideY(begin: 0.1, end: 0, duration: 500.ms, delay: 150.ms, curve: Curves.easeOutCubic),
        const SizedBox(height: 24),

        // Account Section
        Text(
          'Account',
          style: GoogleFonts.inter(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
          ),
        ).animate().fadeIn(duration: 500.ms, delay: 200.ms),
        const SizedBox(height: 12),
        _buildMenuSection(context, isDark, appTheme, [
          _MenuItem(Icons.person_outline_rounded, 'Profile Information', () {
            Navigator.push(context, MaterialPageRoute(builder: (_) => const ViewProfilePage()));
          }),
          _MenuItem(Icons.history_rounded, 'Delivery History', () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Delivery history coming soon', style: GoogleFonts.inter()),
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
              ),
            );
          }),
          _MenuItem(Icons.bar_chart_rounded, 'Performance Stats', () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Performance stats coming soon', style: GoogleFonts.inter()),
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
              ),
            );
          }),
        ]).animate()
            .fadeIn(duration: 500.ms, delay: 250.ms)
            .slideY(begin: 0.05, end: 0, duration: 500.ms, delay: 250.ms, curve: Curves.easeOutCubic),
        const SizedBox(height: 24),

        // Settings Section
        Text(
          'Settings',
          style: GoogleFonts.inter(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            letterSpacing: -0.3,
          ),
        ).animate().fadeIn(duration: 500.ms, delay: 300.ms),
        const SizedBox(height: 12),
        _buildMenuSection(context, isDark, appTheme, [
          _MenuItem(Icons.notifications_outlined, 'Notification Settings', () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Notification settings coming soon', style: GoogleFonts.inter()),
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
              ),
            );
          }),
          _MenuItem(Icons.settings_outlined, 'App Settings', () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('App settings coming soon', style: GoogleFonts.inter()),
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
              ),
            );
          }),
          _MenuItem(Icons.help_outline_rounded, 'Help & Support', () {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Help & support coming soon', style: GoogleFonts.inter()),
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
              ),
            );
          }),
        ]).animate()
            .fadeIn(duration: 500.ms, delay: 350.ms)
            .slideY(begin: 0.05, end: 0, duration: 500.ms, delay: 350.ms, curve: Curves.easeOutCubic),
        const SizedBox(height: 24),

        // Logout
        _buildLogoutButton(context, ref, isDark, appTheme)
            .animate()
            .fadeIn(duration: 500.ms, delay: 400.ms)
            .slideY(begin: 0.05, end: 0, duration: 500.ms, delay: 400.ms, curve: Curves.easeOutCubic),
        const SizedBox(height: 16),
      ],
    );
  }

  Widget _buildProfileHeader(BuildContext context, AsyncValue profileAsync, bool isDark, CustomThemeExtension appTheme) {
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
        child: profileAsync.when(
          data: (profile) => Column(
            children: [
              Container(
                padding: const EdgeInsets.all(4),
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(color: AppColors.accent, width: 3),
                  boxShadow: AppShadows.coloredShadow(AppColors.accent),
                ),
                child: CircleAvatar(
                  radius: 44,
                  backgroundColor: Colors.white.withAlpha(20),
                  backgroundImage: profile.profileImageUrl != null
                      ? NetworkImage(profile.profileImageUrl!)
                      : null,
                  child: profile.profileImageUrl == null
                      ? Icon(Icons.person_rounded, size: 44, color: Colors.white.withAlpha(200))
                      : null,
                ),
              ),
              const SizedBox(height: 16),
              Text(
                profile.fullName,
                style: GoogleFonts.inter(
                  fontSize: 24,
                  fontWeight: FontWeight.w800,
                  color: Colors.white,
                  letterSpacing: -0.5,
                ),
              ),
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
                decoration: BoxDecoration(
                  color: AppColors.accent.withAlpha(30),
                  borderRadius: AppRadius.radiusPill,
                  border: Border.all(color: AppColors.accent.withAlpha(80)),
                ),
                child: Text(
                  'DRIVER',
                  style: GoogleFonts.inter(
                    fontSize: 11,
                    fontWeight: FontWeight.w700,
                    color: AppColors.accentLight,
                    letterSpacing: 1.2,
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.email_outlined, size: 16, color: Colors.white.withAlpha(150)),
                  const SizedBox(width: 6),
                  Text(
                    profile.email,
                    style: GoogleFonts.inter(fontSize: 13, color: Colors.white.withAlpha(180)),
                  ),
                ],
              ),
              const SizedBox(height: 6),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.phone_outlined, size: 16, color: Colors.white.withAlpha(150)),
                  const SizedBox(width: 6),
                  Text(
                    profile.phoneNumber,
                    style: GoogleFonts.inter(fontSize: 13, color: Colors.white.withAlpha(180)),
                  ),
                ],
              ),
            ],
          ),
          loading: () => const Padding(
            padding: EdgeInsets.symmetric(vertical: 40),
            child: LoadingIndicator(),
          ),
          error: (error, _) => Padding(
            padding: const EdgeInsets.symmetric(vertical: 40),
            child: Text(
              'Error: $error',
              style: GoogleFonts.inter(color: AppColors.errorLight),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildStatsGrid(BuildContext context, bool isDark, CustomThemeExtension appTheme) {
    return Row(
      children: [
        Expanded(
          child: _ProfileStatCard(
            icon: Icons.local_shipping_rounded,
            label: 'Deliveries',
            value: '0',
            color: appTheme.infoColor,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
        const SizedBox(width: 10),
        Expanded(
          child: _ProfileStatCard(
            icon: Icons.star_rounded,
            label: 'Rating',
            value: '5.0',
            color: AppColors.accent,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
        const SizedBox(width: 10),
        Expanded(
          child: _ProfileStatCard(
            icon: Icons.attach_money_rounded,
            label: 'Earnings',
            value: '\$0',
            color: appTheme.successColor,
            isDark: isDark,
            appTheme: appTheme,
          ),
        ),
      ],
    );
  }

  Widget _buildShiftStatus(BuildContext context, AsyncValue shiftState, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: shiftState.when(
        data: (shift) => Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: (shift.isActive ? appTheme.successColor : appTheme.textTertiary).withAlpha(25),
                borderRadius: AppRadius.radiusMd,
              ),
              child: Icon(
                shift.isActive ? Icons.check_circle_rounded : Icons.cancel_rounded,
                color: shift.isActive ? appTheme.successColor : appTheme.textTertiary,
                size: 22,
              ),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Current Shift',
                    style: GoogleFonts.inter(fontSize: 15, fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    shift.isActive ? 'Active Shift' : 'No Active Shift',
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      color: shift.isActive ? appTheme.successColor : appTheme.textTertiary,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ],
              ),
            ),
            if (shift.isActive && shift.startTime != null)
              Text(
                'Since ${_formatTime(shift.startTime!)}',
                style: GoogleFonts.inter(fontSize: 12, color: appTheme.textSecondary),
              ),
          ],
        ),
        loading: () => const LoadingIndicator(),
        error: (_, __) => const SizedBox.shrink(),
      ),
    );
  }

  Widget _buildMenuSection(BuildContext context, bool isDark, CustomThemeExtension appTheme, List<_MenuItem> items) {
    return Container(
      decoration: BoxDecoration(
        color: isDark ? appTheme.cardBackground : Colors.white,
        borderRadius: AppRadius.radiusLg,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
      ),
      child: Column(
        children: items.asMap().entries.map((entry) {
          final index = entry.key;
          final item = entry.value;
          final isLast = index == items.length - 1;

          return Column(
            children: [
              Material(
                color: Colors.transparent,
                child: InkWell(
                  onTap: item.onTap,
                  borderRadius: index == 0 && isLast
                      ? AppRadius.radiusLg
                      : index == 0
                          ? AppRadius.radiusTopLg
                          : isLast
                              ? AppRadius.radiusBottomMd
                              : BorderRadius.zero,
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                    child: Row(
                      children: [
                        Icon(item.icon, size: 22, color: appTheme.textSecondary),
                        const SizedBox(width: 14),
                        Expanded(
                          child: Text(
                            item.title,
                            style: GoogleFonts.inter(fontSize: 15, fontWeight: FontWeight.w500),
                          ),
                        ),
                        Icon(Icons.chevron_right_rounded, size: 22, color: appTheme.textTertiary),
                      ],
                    ),
                  ),
                ),
              ),
              if (!isLast)
                Divider(height: 1, indent: 52, color: appTheme.dividerColor),
            ],
          );
        }).toList(),
      ),
    );
  }

  Widget _buildLogoutButton(BuildContext context, WidgetRef ref, bool isDark, CustomThemeExtension appTheme) {
    return Container(
      decoration: BoxDecoration(
        color: appTheme.errorColor.withAlpha(isDark ? 20 : 12),
        borderRadius: AppRadius.radiusLg,
        border: Border.all(color: appTheme.errorColor.withAlpha(30)),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () => _handleDriverLogout(context, ref),
          borderRadius: AppRadius.radiusLg,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
            child: Row(
              children: [
                Icon(Icons.logout_rounded, size: 22, color: appTheme.errorColor),
                const SizedBox(width: 14),
                Text(
                  'Logout',
                  style: GoogleFonts.inter(
                    fontSize: 15,
                    fontWeight: FontWeight.w600,
                    color: appTheme.errorColor,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _formatTime(DateTime time) {
    final hour = time.hour.toString().padLeft(2, '0');
    final minute = time.minute.toString().padLeft(2, '0');
    return '$hour:$minute';
  }

  Future<void> _handleDriverLogout(BuildContext context, WidgetRef ref) async {
    final shiftState = ref.read(driverShiftNotifierProvider);
    final hasActiveShift = shiftState.value?.isActive ?? false;

    if (hasActiveShift) {
      final action = await showDialog<String>(
        context: context,
        builder: (dialogContext) => AlertDialog(
          shape: AppRadius.dialogShape,
          title: Text('Active Shift Detected', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
          content: Text(
            'You have an active shift running. You should end your shift before logging out.\n\nDo you want to:',
            style: GoogleFonts.inter(),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(dialogContext, 'cancel'),
              child: Text('Cancel', style: GoogleFonts.inter()),
            ),
            TextButton(
              onPressed: () => Navigator.pop(dialogContext, 'end_and_logout'),
              child: Text('End Shift & Logout', style: GoogleFonts.inter()),
            ),
            ElevatedButton(
              onPressed: () => Navigator.pop(dialogContext, 'logout_anyway'),
              style: ElevatedButton.styleFrom(
                backgroundColor: Theme.of(dialogContext).colorScheme.error,
                foregroundColor: Colors.white,
                shape: AppRadius.buttonShape,
              ),
              child: Text('Logout Anyway', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
            ),
          ],
        ),
      );

      if (!context.mounted) return;

      if (action == 'end_and_logout') {
        showDialog(
          context: context,
          barrierDismissible: false,
          builder: (context) => AlertDialog(
            shape: AppRadius.dialogShape,
            content: Row(
              children: [
                const CircularProgressIndicator(),
                const SizedBox(width: 16),
                Text('Ending shift...', style: GoogleFonts.inter()),
              ],
            ),
          ),
        );

        try {
          await ref.read(driverShiftNotifierProvider.notifier).endShift();
          if (context.mounted) {
            Navigator.pop(context);
            await _performLogout(context, ref);
          }
        } catch (e) {
          if (context.mounted) {
            Navigator.pop(context);
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Failed to end shift: $e', style: GoogleFonts.inter()),
                backgroundColor: context.appTheme.errorColor,
                behavior: SnackBarBehavior.floating,
              ),
            );
          }
        }
      } else if (action == 'logout_anyway') {
        await _performLogout(context, ref);
      }
    } else {
      final confirmed = await showDialog<bool>(
        context: context,
        builder: (context) => AlertDialog(
          shape: AppRadius.dialogShape,
          title: Text('Logout', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
          content: Text('Are you sure you want to logout?', style: GoogleFonts.inter()),
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
              child: Text('Logout', style: GoogleFonts.inter(fontWeight: FontWeight.w600)),
            ),
          ],
        ),
      );

      if (confirmed == true && context.mounted) {
        await _performLogout(context, ref);
      }
    }
  }

  Future<void> _performLogout(BuildContext context, WidgetRef ref) async {
    try {
      if (context.mounted) {
        showDialog(
          context: context,
          barrierDismissible: false,
          builder: (context) => const Center(child: CircularProgressIndicator()),
        );
      }

      await ref.read(authNotifierProvider.notifier).logout();

      if (context.mounted) {
        Navigator.of(context).pop();
      }

      if (context.mounted) {
        Navigator.of(context).popUntil((route) => route.isFirst);
      }
    } catch (e) {
      if (context.mounted) {
        Navigator.of(context).pop();
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Logout failed: $e', style: GoogleFonts.inter()),
            backgroundColor: context.appTheme.errorColor,
            behavior: SnackBarBehavior.floating,
          ),
        );
      }
    }
  }
}

// ============================================================================
// PROFILE STAT CARD
// ============================================================================
class _ProfileStatCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color color;
  final bool isDark;
  final CustomThemeExtension appTheme;

  const _ProfileStatCard({
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
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: color.withAlpha(25),
              borderRadius: AppRadius.radiusMd,
            ),
            child: Icon(icon, color: color, size: 20),
          ),
          const SizedBox(height: 10),
          Text(
            value,
            style: GoogleFonts.inter(
              fontSize: 20,
              fontWeight: FontWeight.w800,
              letterSpacing: -0.5,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: GoogleFonts.inter(
              fontSize: 11,
              color: appTheme.textSecondary,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

class _MenuItem {
  final IconData icon;
  final String title;
  final VoidCallback onTap;
  const _MenuItem(this.icon, this.title, this.onTap);
}
