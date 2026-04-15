import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/widgets/loading_indicator.dart';
import '../../auth/presentation/auth_notifier.dart';
import '../../support/presentation/support_tickets_page.dart';
import '../../settings/presentation/settings_page.dart';
import 'user_profile_notifier.dart';
import 'view_profile_page.dart';

class ProfilePage extends ConsumerWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    final isDark = context.isDarkMode;
    final appTheme = context.appTheme;

    return ListView(
      padding: const EdgeInsets.fromLTRB(16, 8, 16, 100),
      children: [
        // Profile Header Card
        Container(
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            gradient: AppColors.primaryGradient,
            borderRadius: AppRadius.radiusXl,
            boxShadow: AppShadows.coloredShadow(AppColors.primary),
          ),
          child: profileAsync.when(
            data: (profile) => Column(
              children: [
                CircleAvatar(
                  radius: 40,
                  backgroundColor: Colors.white.withAlpha(30),
                  backgroundImage: profile.profileImageUrl != null
                      ? NetworkImage(profile.profileImageUrl!)
                      : null,
                  child: profile.profileImageUrl == null
                      ? const Icon(Icons.person_rounded, size: 40, color: Colors.white70)
                      : null,
                ),
                const SizedBox(height: 16),
                Text(
                  profile.fullName,
                  style: GoogleFonts.inter(
                    fontSize: 22,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                    letterSpacing: -0.3,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  profile.email,
                  style: GoogleFonts.inter(
                    fontSize: 13,
                    color: Colors.white70,
                  ),
                ),
                if (profile.role != null) ...[
                  const SizedBox(height: 12),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
                    decoration: BoxDecoration(
                      color: AppColors.accent.withAlpha(40),
                      borderRadius: AppRadius.radiusPill,
                      border: Border.all(color: AppColors.accent.withAlpha(80)),
                    ),
                    child: Text(
                      profile.role!,
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        color: AppColors.accentLight,
                        letterSpacing: 1,
                      ),
                    ),
                  ),
                ],
              ],
            ),
            loading: () => const SizedBox(height: 120, child: Center(child: CircularProgressIndicator(color: Colors.white))),
            error: (_, __) => const SizedBox(height: 120, child: Center(child: Icon(Icons.error, color: Colors.white54, size: 40))),
          ),
        )
            .animate()
            .fadeIn(duration: 500.ms)
            .slideY(begin: 0.1, end: 0, duration: 500.ms),

        const SizedBox(height: 24),

        // Menu Section
        Text(
          'ACCOUNT',
          style: GoogleFonts.inter(
            fontSize: 12,
            fontWeight: FontWeight.w600,
            color: AppColors.textTertiary,
            letterSpacing: 1.5,
          ),
        )
            .animate(delay: 200.ms)
            .fadeIn(duration: 400.ms),

        const SizedBox(height: 12),

        _buildMenuCard(
          context: context,
          isDark: isDark,
          items: [
            _MenuItem(
              icon: Icons.person_outline_rounded,
              iconColor: AppColors.primaryMedium,
              title: 'Profile Information',
              subtitle: 'View and edit your details',
              onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ViewProfilePage())),
            ),
            _MenuItem(
              icon: Icons.support_agent_rounded,
              iconColor: AppColors.success,
              title: 'Support Tickets',
              subtitle: 'Get help and track requests',
              onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SupportTicketsPage())),
            ),
            _MenuItem(
              icon: Icons.settings_outlined,
              iconColor: AppColors.secondary,
              title: 'Settings',
              subtitle: 'App preferences',
              onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SettingsPage())),
            ),
          ],
        )
            .animate(delay: 300.ms)
            .fadeIn(duration: 400.ms)
            .slideY(begin: 0.05, end: 0, duration: 400.ms),

        const SizedBox(height: 24),

        // Logout
        GestureDetector(
          onTap: () => _handleLogout(context, ref, appTheme),
          child: Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.error.withAlpha(isDark ? 25 : 15),
              borderRadius: AppRadius.radiusXl,
              border: Border.all(color: AppColors.error.withAlpha(40)),
            ),
            child: Row(
              children: [
                Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: AppColors.error.withAlpha(30),
                    borderRadius: AppRadius.radiusMd,
                  ),
                  child: const Icon(Icons.logout_rounded, color: AppColors.error, size: 20),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Text(
                    'Logout',
                    style: GoogleFonts.inter(
                      fontSize: 15,
                      fontWeight: FontWeight.w600,
                      color: AppColors.error,
                    ),
                  ),
                ),
                Icon(Icons.chevron_right_rounded, color: AppColors.error.withAlpha(120), size: 20),
              ],
            ),
          ),
        )
            .animate(delay: 400.ms)
            .fadeIn(duration: 400.ms),
      ],
    );
  }

  Widget _buildMenuCard({
    required BuildContext context,
    required bool isDark,
    required List<_MenuItem> items,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardBackgroundDark : Colors.white,
        borderRadius: AppRadius.radiusXl,
        boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
        border: Border.all(color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5)),
      ),
      child: Column(
        children: items.asMap().entries.map((entry) {
          final index = entry.key;
          final item = entry.value;
          final isLast = index == items.length - 1;

          return Column(
            children: [
              InkWell(
                onTap: item.onTap,
                borderRadius: BorderRadius.vertical(
                  top: index == 0 ? const Radius.circular(AppRadius.xl) : Radius.zero,
                  bottom: isLast ? const Radius.circular(AppRadius.xl) : Radius.zero,
                ),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      Container(
                        width: 40,
                        height: 40,
                        decoration: BoxDecoration(
                          color: item.iconColor.withAlpha(isDark ? 30 : 20),
                          borderRadius: AppRadius.radiusMd,
                        ),
                        child: Icon(item.icon, color: item.iconColor, size: 20),
                      ),
                      const SizedBox(width: 14),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              item.title,
                              style: GoogleFonts.inter(fontSize: 15, fontWeight: FontWeight.w600),
                            ),
                            const SizedBox(height: 2),
                            Text(
                              item.subtitle,
                              style: GoogleFonts.inter(
                                fontSize: 12,
                                color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ),
                      Icon(
                        Icons.chevron_right_rounded,
                        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                        size: 20,
                      ),
                    ],
                  ),
                ),
              ),
              if (!isLast) Divider(
                height: 1,
                indent: 70,
                color: isDark ? AppColors.dividerDark : AppColors.divider,
              ),
            ],
          );
        }).toList(),
      ),
    );
  }

  Future<void> _handleLogout(BuildContext context, WidgetRef ref, CustomThemeExtension appTheme) async {
    final shouldLogout = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Logout', style: GoogleFonts.inter(fontWeight: FontWeight.w700)),
        content: const Text('Are you sure you want to logout?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: const Text('Logout'),
          ),
        ],
      ),
    );

    if (shouldLogout == true && context.mounted) {
      // Show loading dialog and capture its Navigator context
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (_) => const LoadingIndicator(),
      );

      try {
        // Logout will set auth state to unauthenticated, which triggers
        // AuthWidget to swap MainScaffold with LoginPage. We just need
        // to dismiss the loading dialog before that happens.
        
        // Pop the loading dialog first, before the auth state changes
        if (context.mounted) {
          Navigator.of(context, rootNavigator: true).pop();
        }
        
        // Now trigger the logout — this invalidates providers and
        // changes auth state, which will automatically navigate to LoginPage
        // via AuthWidget's AnimatedSwitcher.
        await ref.read(authNotifierProvider.notifier).logout();
      } catch (e) {
        if (context.mounted) {
          // If dialog is still showing, pop it
          Navigator.of(context, rootNavigator: true).pop();
        }
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Logout error: $e'), backgroundColor: AppColors.error),
          );
        }
      }
    }
  }
}

class _MenuItem {
  final IconData icon;
  final Color iconColor;
  final String title;
  final String subtitle;
  final VoidCallback onTap;
  const _MenuItem({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });
}
