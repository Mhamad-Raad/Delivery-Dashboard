import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import 'user_profile_notifier.dart';
import 'edit_profile_page.dart';

class ViewProfilePage extends ConsumerWidget {
  const ViewProfilePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileState = ref.watch(userProfileProvider);
    final isDark = context.isDarkMode;

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        leading: IconButton(
          icon: Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: isDark ? Colors.white.withAlpha(10) : Colors.black.withAlpha(8),
              borderRadius: AppRadius.radiusMd,
            ),
            child: Icon(
              Icons.arrow_back_ios_new_rounded,
              size: 18,
              color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
            ),
          ),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text(
          'My Profile',
          style: GoogleFonts.inter(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
          ),
        ),
        centerTitle: true,
        actions: [
          profileState.when(
            data: (profile) => Padding(
              padding: const EdgeInsets.only(right: 8),
              child: IconButton(
                icon: Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    gradient: AppColors.primaryGradient,
                    borderRadius: AppRadius.radiusMd,
                    boxShadow: AppShadows.coloredShadow(AppColors.primary, blur: 12, spread: -2),
                  ),
                  child: const Icon(Icons.edit_rounded, size: 18, color: Colors.white),
                ),
                onPressed: () async {
                  final updated = await Navigator.push<bool>(
                    context,
                    MaterialPageRoute(
                      builder: (context) => EditProfilePage(profile: profile),
                    ),
                  );

                  if (updated == true) {
                    ref.read(userProfileProvider.notifier).refresh();
                  }
                },
              ),
            ),
            loading: () => const SizedBox.shrink(),
            error: (_, __) => const SizedBox.shrink(),
          ),
        ],
      ),
      body: profileState.when(
        data: (profile) {
          return RefreshIndicator(
            color: AppColors.accent,
            onRefresh: () => ref.read(userProfileProvider.notifier).refresh(),
            child: SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 40),
              child: Column(
                children: [
                  // Premium Avatar with Gradient Ring
                  Container(
                    padding: const EdgeInsets.all(4),
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      gradient: const LinearGradient(
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                        colors: [AppColors.accent, AppColors.primaryMedium, AppColors.accent],
                      ),
                      boxShadow: AppShadows.coloredShadow(AppColors.accent, blur: 30, spread: -4),
                    ),
                    child: Container(
                      padding: const EdgeInsets.all(3),
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: isDark ? AppColors.backgroundDark : AppColors.background,
                      ),
                      child: CircleAvatar(
                        radius: 56,
                        backgroundColor: isDark
                            ? AppColors.primaryMedium.withAlpha(40)
                            : AppColors.primaryContainer,
                        backgroundImage: profile.profileImageUrl != null
                            ? NetworkImage(profile.profileImageUrl!)
                            : null,
                        child: profile.profileImageUrl == null
                            ? const Icon(Icons.person_rounded, size: 56, color: AppColors.primaryMedium)
                            : null,
                      ),
                    ),
                  )
                      .animate()
                      .fadeIn(duration: 600.ms)
                      .scale(begin: const Offset(0.8, 0.8), end: const Offset(1, 1), duration: 600.ms, curve: Curves.easeOutBack),

                  const SizedBox(height: 20),

                  // Full Name
                  Text(
                    profile.fullName,
                    style: GoogleFonts.inter(
                      fontSize: 26,
                      fontWeight: FontWeight.w800,
                      color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                      letterSpacing: -0.5,
                    ),
                  )
                      .animate(delay: 200.ms)
                      .fadeIn(duration: 400.ms)
                      .slideY(begin: 0.2, end: 0, duration: 400.ms),

                  const SizedBox(height: 6),

                  // Email subtitle
                  Text(
                    profile.email,
                    style: GoogleFonts.inter(
                      fontSize: 14,
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                  )
                      .animate(delay: 300.ms)
                      .fadeIn(duration: 400.ms),

                  const SizedBox(height: 14),

                  // Role Badge
                  if (profile.role != null)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                      decoration: BoxDecoration(
                        gradient: AppColors.accentGradient,
                        borderRadius: AppRadius.radiusPill,
                        boxShadow: AppShadows.coloredShadow(AppColors.accent, blur: 16, spread: -4),
                      ),
                      child: Text(
                        profile.role!,
                        style: GoogleFonts.inter(
                          fontSize: 12,
                          fontWeight: FontWeight.w700,
                          color: AppColors.primary,
                          letterSpacing: 1.2,
                        ),
                      ),
                    )
                        .animate(delay: 400.ms)
                        .fadeIn(duration: 400.ms)
                        .scale(begin: const Offset(0.8, 0.8), end: const Offset(1, 1), duration: 400.ms),

                  const SizedBox(height: 32),

                  // Section Header
                  Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      'PERSONAL INFORMATION',
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                        letterSpacing: 1.5,
                      ),
                    ),
                  )
                      .animate(delay: 500.ms)
                      .fadeIn(duration: 400.ms),

                  const SizedBox(height: 16),

                  // Information Cards Container
                  Container(
                    decoration: BoxDecoration(
                      color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                      borderRadius: AppRadius.radiusXl,
                      boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                      border: Border.all(
                        color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
                      ),
                    ),
                    child: Column(
                      children: [
                        _buildInfoRow(
                          icon: Icons.person_outline_rounded,
                          iconColor: AppColors.primaryMedium,
                          label: 'First Name',
                          value: profile.firstName.isNotEmpty ? profile.firstName : 'Not set',
                          isDark: isDark,
                          isFirst: true,
                        ),
                        _divider(isDark),
                        _buildInfoRow(
                          icon: Icons.person_outline_rounded,
                          iconColor: AppColors.primaryMedium,
                          label: 'Last Name',
                          value: profile.lastName.isNotEmpty ? profile.lastName : 'Not set',
                          isDark: isDark,
                        ),
                        _divider(isDark),
                        _buildInfoRow(
                          icon: Icons.email_outlined,
                          iconColor: AppColors.info,
                          label: 'Email',
                          value: profile.email.isNotEmpty ? profile.email : 'Not set',
                          isDark: isDark,
                        ),
                        _divider(isDark),
                        _buildInfoRow(
                          icon: Icons.phone_outlined,
                          iconColor: AppColors.success,
                          label: 'Phone',
                          value: profile.phoneNumber.isNotEmpty ? profile.phoneNumber : 'Not set',
                          isDark: isDark,
                          isLast: profile.buildingName == null && profile.apartmentNumber == null,
                        ),
                        if (profile.buildingName != null) ...[
                          _divider(isDark),
                          _buildInfoRow(
                            icon: Icons.apartment_rounded,
                            iconColor: AppColors.warning,
                            label: 'Building',
                            value: profile.buildingName!,
                            isDark: isDark,
                            isLast: profile.apartmentNumber == null,
                          ),
                        ],
                        if (profile.apartmentNumber != null) ...[
                          _divider(isDark),
                          _buildInfoRow(
                            icon: Icons.home_outlined,
                            iconColor: AppColors.accent,
                            label: 'Apartment',
                            value: profile.apartmentNumber!,
                            isDark: isDark,
                            isLast: true,
                          ),
                        ],
                      ],
                    ),
                  )
                      .animate(delay: 600.ms)
                      .fadeIn(duration: 500.ms)
                      .slideY(begin: 0.05, end: 0, duration: 500.ms),

                  const SizedBox(height: 24),

                  // User ID Card
                  Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      'ACCOUNT',
                      style: GoogleFonts.inter(
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                        letterSpacing: 1.5,
                      ),
                    ),
                  )
                      .animate(delay: 700.ms)
                      .fadeIn(duration: 400.ms),

                  const SizedBox(height: 16),

                  Container(
                    decoration: BoxDecoration(
                      color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                      borderRadius: AppRadius.radiusXl,
                      boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                      border: Border.all(
                        color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
                      ),
                    ),
                    child: _buildInfoRow(
                      icon: Icons.badge_outlined,
                      iconColor: AppColors.secondary,
                      label: 'User ID',
                      value: profile.id.length > 8
                          ? '${profile.id.substring(0, 8)}...'
                          : profile.id,
                      isDark: isDark,
                      isFirst: true,
                      isLast: true,
                    ),
                  )
                      .animate(delay: 800.ms)
                      .fadeIn(duration: 500.ms)
                      .slideY(begin: 0.05, end: 0, duration: 500.ms),
                ],
              ),
            ),
          );
        },
        loading: () => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              SizedBox(
                width: 48,
                height: 48,
                child: CircularProgressIndicator(
                  strokeWidth: 3,
                  color: AppColors.accent,
                  backgroundColor: AppColors.accent.withAlpha(30),
                ),
              ),
              const SizedBox(height: 16),
              Text(
                'Loading profile...',
                style: GoogleFonts.inter(
                  fontSize: 14,
                  color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                ),
              ),
            ],
          ),
        ),
        error: (error, stack) {
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
                      color: AppColors.error.withAlpha(isDark ? 25 : 15),
                      borderRadius: AppRadius.radiusXxl,
                    ),
                    child: const Icon(Icons.error_outline_rounded, size: 40, color: AppColors.error),
                  ),
                  const SizedBox(height: 20),
                  Text(
                    'Error loading profile',
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
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 24),
                  GestureDetector(
                    onTap: () => ref.read(userProfileProvider.notifier).refresh(),
                    child: Container(
                      padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 14),
                      decoration: BoxDecoration(
                        gradient: AppColors.primaryGradient,
                        borderRadius: AppRadius.radiusPill,
                        boxShadow: AppShadows.coloredShadow(AppColors.primary),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Icon(Icons.refresh_rounded, color: Colors.white, size: 20),
                          const SizedBox(width: 8),
                          Text(
                            'Retry',
                            style: GoogleFonts.inter(
                              fontSize: 15,
                              fontWeight: FontWeight.w600,
                              color: Colors.white,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildInfoRow({
    required IconData icon,
    required Color iconColor,
    required String label,
    required String value,
    required bool isDark,
    bool isFirst = false,
    bool isLast = false,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: iconColor.withAlpha(isDark ? 30 : 20),
              borderRadius: AppRadius.radiusMd,
            ),
            child: Icon(icon, color: iconColor, size: 20),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: GoogleFonts.inter(
                    fontSize: 12,
                    fontWeight: FontWeight.w500,
                    color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                  ),
                ),
                const SizedBox(height: 3),
                Text(
                  value,
                  style: GoogleFonts.inter(
                    fontSize: 15,
                    fontWeight: FontWeight.w600,
                    color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _divider(bool isDark) {
    return Divider(
      height: 1,
      indent: 70,
      color: isDark ? AppColors.dividerDark : AppColors.divider,
    );
  }
}
