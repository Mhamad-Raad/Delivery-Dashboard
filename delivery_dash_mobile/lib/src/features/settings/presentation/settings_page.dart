import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../core/design/design_system.dart';
import '../../../core/theme/custom_theme_extension.dart';
import '../../../core/providers/localization_provider.dart';
import '../../../core/providers/theme_provider.dart';

class SettingsPage extends ConsumerWidget {
  const SettingsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final themeMode = ref.watch(themeModeProvider);
    final locale = ref.watch(localeProvider);
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
          l10n?.appTitle ?? 'Settings',
          style: GoogleFonts.inter(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
          ),
        ),
        centerTitle: true,
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 40),
        children: [
          // Appearance Section
          _buildSectionHeader(
            title: 'APPEARANCE',
            isDark: isDark,
          )
              .animate()
              .fadeIn(duration: 400.ms),

          const SizedBox(height: 12),

          // Theme Toggle Card
          Container(
            decoration: BoxDecoration(
              color: isDark ? AppColors.cardBackgroundDark : Colors.white,
              borderRadius: AppRadius.radiusXl,
              boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
              border: Border.all(
                color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
              ),
            ),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.accent.withAlpha(isDark ? 30 : 20),
                      borderRadius: AppRadius.radiusMd,
                    ),
                    child: Icon(
                      themeMode == ThemeMode.dark
                          ? Icons.nightlight_round_outlined
                          : Icons.wb_sunny_outlined,
                      color: AppColors.accent,
                      size: 22,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          l10n?.theme ?? 'Theme',
                          style: GoogleFonts.inter(
                            fontSize: 15,
                            fontWeight: FontWeight.w600,
                            color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                          ),
                        ),
                        const SizedBox(height: 3),
                        Text(
                          themeMode == ThemeMode.dark
                              ? (l10n?.darkTheme ?? 'Dark')
                              : (l10n?.lightTheme ?? 'Light'),
                          style: GoogleFonts.inter(
                            fontSize: 12,
                            color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                  _buildPremiumSwitch(
                    value: themeMode == ThemeMode.dark,
                    onChanged: (bool darkEnabled) {
                      ref.read(themeModeProvider.notifier).setTheme(
                        darkEnabled ? ThemeMode.dark : ThemeMode.light,
                      );
                    },
                  ),
                ],
              ),
            ),
          )
              .animate(delay: 100.ms)
              .fadeIn(duration: 400.ms)
              .slideY(begin: 0.05, end: 0, duration: 400.ms),

          const SizedBox(height: 28),

          // Language Section
          _buildSectionHeader(
            title: 'LANGUAGE',
            isDark: isDark,
          )
              .animate(delay: 200.ms)
              .fadeIn(duration: 400.ms),

          const SizedBox(height: 12),

          // Language Selector Card
          GestureDetector(
            onTap: () => _showLanguageBottomSheet(context, ref, locale, isDark),
            child: Container(
              decoration: BoxDecoration(
                color: isDark ? AppColors.cardBackgroundDark : Colors.white,
                borderRadius: AppRadius.radiusXl,
                boxShadow: isDark ? AppShadows.cardShadowDark : AppShadows.cardShadow,
                border: Border.all(
                  color: isDark ? Colors.white.withAlpha(8) : Colors.black.withAlpha(5),
                ),
              ),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Row(
                  children: [
                    Container(
                      width: 44,
                      height: 44,
                      decoration: BoxDecoration(
                        color: AppColors.info.withAlpha(isDark ? 30 : 20),
                        borderRadius: AppRadius.radiusMd,
                      ),
                      child: const Icon(
                        Icons.language_rounded,
                        color: AppColors.info,
                        size: 22,
                      ),
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            l10n?.language ?? 'Language',
                            style: GoogleFonts.inter(
                              fontSize: 15,
                              fontWeight: FontWeight.w600,
                              color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 3),
                          Text(
                            _getLanguageName(locale),
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
                      size: 22,
                    ),
                  ],
                ),
              ),
            ),
          )
              .animate(delay: 300.ms)
              .fadeIn(duration: 400.ms)
              .slideY(begin: 0.05, end: 0, duration: 400.ms),

          const SizedBox(height: 28),

          // About Section
          _buildSectionHeader(
            title: 'ABOUT',
            isDark: isDark,
          )
              .animate(delay: 400.ms)
              .fadeIn(duration: 400.ms),

          const SizedBox(height: 12),

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
                _buildSimpleRow(
                  icon: Icons.info_outline_rounded,
                  iconColor: AppColors.primaryMedium,
                  title: 'Version',
                  trailing: Text(
                    '1.0.0',
                    style: GoogleFonts.inter(
                      fontSize: 14,
                      fontWeight: FontWeight.w500,
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                  ),
                  isDark: isDark,
                  isFirst: true,
                ),
                Divider(
                  height: 1,
                  indent: 70,
                  color: isDark ? AppColors.dividerDark : AppColors.divider,
                ),
                _buildSimpleRow(
                  icon: Icons.description_outlined,
                  iconColor: AppColors.secondary,
                  title: 'Terms of Service',
                  trailing: Icon(
                    Icons.chevron_right_rounded,
                    color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                    size: 20,
                  ),
                  isDark: isDark,
                ),
                Divider(
                  height: 1,
                  indent: 70,
                  color: isDark ? AppColors.dividerDark : AppColors.divider,
                ),
                _buildSimpleRow(
                  icon: Icons.shield_outlined,
                  iconColor: AppColors.success,
                  title: 'Privacy Policy',
                  trailing: Icon(
                    Icons.chevron_right_rounded,
                    color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                    size: 20,
                  ),
                  isDark: isDark,
                  isLast: true,
                ),
              ],
            ),
          )
              .animate(delay: 500.ms)
              .fadeIn(duration: 400.ms)
              .slideY(begin: 0.05, end: 0, duration: 400.ms),
        ],
      ),
    );
  }

  Widget _buildSectionHeader({required String title, required bool isDark}) {
    return Padding(
      padding: const EdgeInsets.only(left: 4),
      child: Text(
        title,
        style: GoogleFonts.inter(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
          letterSpacing: 1.5,
        ),
      ),
    );
  }

  Widget _buildPremiumSwitch({
    required bool value,
    required ValueChanged<bool> onChanged,
  }) {
    return GestureDetector(
      onTap: () => onChanged(!value),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 300),
        width: 52,
        height: 30,
        padding: const EdgeInsets.all(3),
        decoration: BoxDecoration(
          borderRadius: AppRadius.radiusPill,
          gradient: value ? AppColors.primaryGradient : null,
          color: value ? null : AppColors.textDisabled,
        ),
        child: AnimatedAlign(
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOutBack,
          alignment: value ? Alignment.centerRight : Alignment.centerLeft,
          child: Container(
            width: 24,
            height: 24,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withAlpha(25),
                  blurRadius: 4,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: Center(
              child: Icon(
                value ? Icons.nightlight_round : Icons.wb_sunny_rounded,
                size: 14,
                color: value ? AppColors.primary : AppColors.accent,
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSimpleRow({
    required IconData icon,
    required Color iconColor,
    required String title,
    required Widget trailing,
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
            child: Text(
              title,
              style: GoogleFonts.inter(
                fontSize: 15,
                fontWeight: FontWeight.w600,
                color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
              ),
            ),
          ),
          trailing,
        ],
      ),
    );
  }

  String _getLanguageName(Locale? locale) {
    switch (locale?.languageCode) {
      case 'ar':
        return 'العربية';
      case 'fa':
        return 'Kurdî (Sorani)';
      case 'en':
      default:
        return 'English';
    }
  }

  void _showLanguageBottomSheet(
    BuildContext context,
    WidgetRef ref,
    Locale? currentLocale,
    bool isDark,
  ) {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      isScrollControlled: true,
      builder: (context) => Container(
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardBackgroundDark : Colors.white,
          borderRadius: AppRadius.radiusTopXxl,
        ),
        child: SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Handle Bar
              Container(
                margin: const EdgeInsets.only(top: 12),
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: isDark ? Colors.white.withAlpha(30) : Colors.black.withAlpha(20),
                  borderRadius: AppRadius.radiusPill,
                ),
              ),

              const SizedBox(height: 20),

              // Title
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24),
                child: Row(
                  children: [
                    Container(
                      width: 44,
                      height: 44,
                      decoration: BoxDecoration(
                        color: AppColors.info.withAlpha(isDark ? 30 : 20),
                        borderRadius: AppRadius.radiusMd,
                      ),
                      child: const Icon(Icons.language_rounded, color: AppColors.info, size: 22),
                    ),
                    const SizedBox(width: 14),
                    Text(
                      'Select Language',
                      style: GoogleFonts.inter(
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                        color: isDark ? AppColors.textPrimaryDark : AppColors.textPrimary,
                      ),
                    ),
                  ],
                ),
              ),

              const SizedBox(height: 20),

              // Language Options
              _buildLanguageOption(
                context: context,
                ref: ref,
                locale: const Locale('en'),
                currentLocale: currentLocale,
                name: 'English',
                nativeName: 'English',
                flag: '',
                isDark: isDark,
              ),
              _buildLanguageOption(
                context: context,
                ref: ref,
                locale: const Locale('ar'),
                currentLocale: currentLocale,
                name: 'Arabic',
                nativeName: 'العربية',
                flag: '',
                isDark: isDark,
              ),
              _buildLanguageOption(
                context: context,
                ref: ref,
                locale: const Locale('fa'),
                currentLocale: currentLocale,
                name: 'Kurdish (Sorani)',
                nativeName: 'Kurdî (Sorani)',
                flag: '',
                isDark: isDark,
              ),

              const SizedBox(height: 16),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildLanguageOption({
    required BuildContext context,
    required WidgetRef ref,
    required Locale locale,
    required Locale? currentLocale,
    required String name,
    required String nativeName,
    required String flag,
    required bool isDark,
  }) {
    final isSelected = currentLocale?.languageCode == locale.languageCode;

    return InkWell(
      onTap: () {
        ref.read(localeProvider.notifier).setLocale(locale);
        Navigator.pop(context);
      },
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 250),
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
        decoration: BoxDecoration(
          color: isSelected
              ? (isDark ? AppColors.primaryMedium.withAlpha(25) : AppColors.primaryContainer)
              : Colors.transparent,
          borderRadius: AppRadius.radiusLg,
          border: isSelected
              ? Border.all(color: AppColors.primaryMedium.withAlpha(60))
              : null,
        ),
        child: Row(
          children: [
            Text(flag, style: const TextStyle(fontSize: 28)),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    name,
                    style: GoogleFonts.inter(
                      fontSize: 15,
                      fontWeight: isSelected ? FontWeight.w700 : FontWeight.w600,
                      color: isSelected
                          ? AppColors.primaryMedium
                          : (isDark ? AppColors.textPrimaryDark : AppColors.textPrimary),
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    nativeName,
                    style: GoogleFonts.inter(
                      fontSize: 13,
                      color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
            if (isSelected)
              Container(
                width: 28,
                height: 28,
                decoration: BoxDecoration(
                  gradient: AppColors.primaryGradient,
                  shape: BoxShape.circle,
                  boxShadow: AppShadows.coloredShadow(AppColors.primary, blur: 8, spread: -2),
                ),
                child: const Icon(Icons.check_rounded, color: Colors.white, size: 16),
              )
            else
              Container(
                width: 28,
                height: 28,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: isDark ? AppColors.outlineDark : AppColors.outline,
                    width: 2,
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
}
