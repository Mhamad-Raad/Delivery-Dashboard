import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../core/design/design_system.dart';
import '../../../core/widgets/gradient_button.dart';
import '../../../core/widgets/theme_toggle_button.dart';
import '../../../core/widgets/language_selector.dart';
import 'auth_notifier.dart';
import 'login_controller.dart';

class LoginPage extends ConsumerStatefulWidget {
  const LoginPage({super.key});
  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage> {
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final loginState = ref.watch(loginControllerProvider);
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    ref.listen(loginControllerProvider, (previous, next) {
      if (next is AsyncError) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.error.toString()),
            backgroundColor: AppColors.error,
            behavior: SnackBarBehavior.floating,
            shape: RoundedRectangleBorder(borderRadius: AppRadius.radiusMd),
          ),
        );
      } else if (next is AsyncData && !next.isLoading) {
        ref.read(authNotifierProvider.notifier).setAuthenticated();
      }
    });

    return Scaffold(
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: BoxDecoration(
          gradient: isDark ? AppColors.splashGradient : null,
          color: isDark ? null : AppColors.background,
        ),
        child: SafeArea(
          child: Column(
            children: [
              // Top bar with language and theme
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    const LanguageSelector(),
                    const ThemeToggleButton(),
                  ],
                ),
              ),

              Expanded(
                child: Center(
                  child: SingleChildScrollView(
                    padding: AppSpacing.allLg,
                    child: ConstrainedBox(
                      constraints: const BoxConstraints(maxWidth: 400),
                      child: Form(
                        key: _formKey,
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            // Brand logo
                            Center(
                              child: Container(
                                width: 80,
                                height: 80,
                                decoration: BoxDecoration(
                                  shape: BoxShape.circle,
                                  gradient: AppColors.accentGradient,
                                  boxShadow: [
                                    BoxShadow(
                                      color: AppColors.accent.withAlpha(40),
                                      blurRadius: 30,
                                      spreadRadius: 2,
                                    ),
                                  ],
                                ),
                                child: Icon(
                                  Icons.shopping_bag_rounded,
                                  size: 36,
                                  color: AppColors.primary,
                                ),
                              ),
                            )
                                .animate()
                                .scale(
                                  begin: const Offset(0.8, 0.8),
                                  end: const Offset(1.0, 1.0),
                                  duration: 600.ms,
                                  curve: Curves.easeOutBack,
                                )
                                .fadeIn(duration: 500.ms),

                            const SizedBox(height: 32),

                            // Welcome text
                            Text(
                              l10n?.welcomeMessage ?? 'Welcome back',
                              style: GoogleFonts.inter(
                                fontSize: 28,
                                fontWeight: FontWeight.w800,
                                color: isDark ? Colors.white : AppColors.textPrimary,
                                letterSpacing: -0.5,
                              ),
                              textAlign: TextAlign.center,
                            )
                                .animate(delay: 200.ms)
                                .fadeIn(duration: 500.ms)
                                .slideY(begin: 0.2, end: 0, duration: 500.ms),

                            const SizedBox(height: 8),

                            Text(
                              'Sign in to continue',
                              style: GoogleFonts.inter(
                                fontSize: 15,
                                fontWeight: FontWeight.w400,
                                color: isDark ? AppColors.textSecondaryDark : AppColors.textSecondary,
                              ),
                              textAlign: TextAlign.center,
                            )
                                .animate(delay: 300.ms)
                                .fadeIn(duration: 500.ms),

                            const SizedBox(height: 40),

                            // Username field
                            _buildField(
                              controller: _usernameController,
                              label: l10n?.username ?? 'Username',
                              icon: Icons.person_outline_rounded,
                              enabled: !loginState.isLoading,
                              validator: (value) => (value == null || value.isEmpty)
                                  ? 'Please enter your username'
                                  : null,
                              isDark: isDark,
                            )
                                .animate(delay: 400.ms)
                                .fadeIn(duration: 500.ms)
                                .slideX(begin: -0.05, end: 0, duration: 500.ms),

                            const SizedBox(height: 16),

                            // Password field
                            _buildField(
                              controller: _passwordController,
                              label: l10n?.password ?? 'Password',
                              icon: Icons.lock_outline_rounded,
                              obscureText: _obscurePassword,
                              enabled: !loginState.isLoading,
                              validator: (value) => (value == null || value.isEmpty)
                                  ? 'Please enter your password'
                                  : null,
                              isDark: isDark,
                              suffixIcon: IconButton(
                                icon: Icon(
                                  _obscurePassword ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                                  color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
                                  size: 20,
                                ),
                                onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                              ),
                            )
                                .animate(delay: 500.ms)
                                .fadeIn(duration: 500.ms)
                                .slideX(begin: -0.05, end: 0, duration: 500.ms),

                            const SizedBox(height: 32),

                            // Sign in button
                            GradientButton(
                              onPressed: loginState.isLoading
                                  ? null
                                  : () {
                                      if (_formKey.currentState!.validate()) {
                                        ref.read(loginControllerProvider.notifier).login(
                                          email: _usernameController.text,
                                          password: _passwordController.text,
                                        );
                                      }
                                    },
                              isLoading: loginState.isLoading,
                              child: Text(l10n?.loginButton ?? 'Sign In'),
                            )
                                .animate(delay: 600.ms)
                                .fadeIn(duration: 500.ms)
                                .slideY(begin: 0.2, end: 0, duration: 500.ms),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildField({
    required TextEditingController controller,
    required String label,
    required IconData icon,
    bool obscureText = false,
    bool enabled = true,
    String? Function(String?)? validator,
    bool isDark = false,
    Widget? suffixIcon,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: obscureText,
      enabled: enabled,
      validator: validator,
      style: GoogleFonts.inter(
        fontSize: 15,
        color: isDark ? Colors.white : AppColors.textPrimary,
      ),
      decoration: InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon, size: 20),
        suffixIcon: suffixIcon,
        filled: true,
        fillColor: isDark
            ? Colors.white.withAlpha(15)
            : AppColors.surfaceVariant.withAlpha(180),
        border: OutlineInputBorder(
          borderRadius: AppRadius.radiusMd,
          borderSide: BorderSide.none,
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: AppRadius.radiusMd,
          borderSide: BorderSide(
            color: isDark ? Colors.white.withAlpha(20) : AppColors.outline.withAlpha(60),
          ),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: AppRadius.radiusMd,
          borderSide: BorderSide(
            color: isDark ? AppColors.accentLight : AppColors.primaryMedium,
            width: 2,
          ),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
        labelStyle: GoogleFonts.inter(
          color: isDark ? AppColors.textTertiaryDark : AppColors.textTertiary,
        ),
      ),
    );
  }
}
