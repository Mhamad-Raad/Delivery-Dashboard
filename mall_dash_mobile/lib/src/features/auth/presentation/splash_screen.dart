import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/design/design_system.dart';

/// Premium animated splash screen with brand reveal.
class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with TickerProviderStateMixin {
  late AnimationController _pulseController;

  @override
  void initState() {
    super.initState();
    _pulseController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat(reverse: true);
  }

  @override
  void dispose() {
    _pulseController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(
          gradient: AppColors.splashGradient,
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Spacer(flex: 3),

            // Logo / Brand Icon
            Container(
              width: 100,
              height: 100,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                gradient: AppColors.accentGradient,
                boxShadow: [
                  BoxShadow(
                    color: AppColors.accent.withAlpha(60),
                    blurRadius: 40,
                    spreadRadius: 5,
                  ),
                ],
              ),
              child: const Icon(
                Icons.shopping_bag_rounded,
                size: 48,
                color: AppColors.primary,
              ),
            )
                .animate()
                .scale(
                  begin: const Offset(0.5, 0.5),
                  end: const Offset(1.0, 1.0),
                  duration: 800.ms,
                  curve: Curves.elasticOut,
                )
                .fadeIn(duration: 600.ms),

            const SizedBox(height: 32),

            // Brand Name
            Text(
              'Delivery Dash',
              style: TextStyle(
                fontSize: 36,
                fontWeight: FontWeight.w800,
                color: Colors.white,
                letterSpacing: -1,
                height: 1,
              ),
            )
                .animate(delay: 300.ms)
                .fadeIn(duration: 600.ms)
                .slideY(begin: 0.3, end: 0, duration: 600.ms, curve: Curves.easeOutCubic),

            const SizedBox(height: 8),

            // Tagline
            Text(
              'Premium Delivery Experience',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w400,
                color: AppColors.accentLight,
                letterSpacing: 2,
              ),
            )
                .animate(delay: 600.ms)
                .fadeIn(duration: 600.ms)
                .slideY(begin: 0.3, end: 0, duration: 600.ms, curve: Curves.easeOutCubic),

            const Spacer(flex: 3),

            // Loading indicator
            SizedBox(
              width: 40,
              child: AnimatedBuilder(
                animation: _pulseController,
                builder: (context, child) {
                  return Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: List.generate(3, (index) {
                      final delay = index * 0.2;
                      final value = (_pulseController.value + delay) % 1.0;
                      final opacity = (value < 0.5 ? value * 2 : (1 - value) * 2).clamp(0.3, 1.0);
                      return Container(
                        width: 8,
                        height: 8,
                        margin: const EdgeInsets.symmetric(horizontal: 2),
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: AppColors.accent.withAlpha((opacity * 255).round()),
                        ),
                      );
                    }),
                  );
                },
              ),
            )
                .animate(delay: 900.ms)
                .fadeIn(duration: 400.ms),

            const SizedBox(height: 60),
          ],
        ),
      ),
    );
  }
}
