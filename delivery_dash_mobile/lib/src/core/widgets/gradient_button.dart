import 'package:flutter/material.dart';
import '../design/design_system.dart';

/// Premium gradient button with loading state and animation.
class GradientButton extends StatefulWidget {
  const GradientButton({
    super.key,
    required this.onPressed,
    required this.child,
    this.gradient,
    this.isLoading = false,
    this.height = 56,
    this.width,
    this.borderRadius,
    this.padding,
  });

  final VoidCallback? onPressed;
  final Widget child;
  final LinearGradient? gradient;
  final bool isLoading;
  final double height;
  final double? width;
  final BorderRadius? borderRadius;
  final EdgeInsetsGeometry? padding;

  @override
  State<GradientButton> createState() => _GradientButtonState();
}

class _GradientButtonState extends State<GradientButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 150),
      vsync: this,
    );
    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.96).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final gradient = widget.gradient ?? AppColors.buttonGradient;
    final radius = widget.borderRadius ?? AppRadius.radiusMd;

    return AnimatedBuilder(
      animation: _scaleAnimation,
      builder: (context, child) => Transform.scale(
        scale: _scaleAnimation.value,
        child: child,
      ),
      child: GestureDetector(
        onTapDown: (_) => _controller.forward(),
        onTapUp: (_) {
          _controller.reverse();
          if (!widget.isLoading) widget.onPressed?.call();
        },
        onTapCancel: () => _controller.reverse(),
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          height: widget.height,
          width: widget.width ?? double.infinity,
          padding: widget.padding ??
              const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
          decoration: BoxDecoration(
            gradient: widget.onPressed != null ? gradient : null,
            color: widget.onPressed == null ? AppColors.textDisabled : null,
            borderRadius: radius,
            boxShadow: widget.onPressed != null
                ? AppShadows.coloredShadow(
                    gradient.colors.first,
                    blur: 20,
                    spread: -4,
                  )
                : null,
          ),
          child: Center(
            child: widget.isLoading
                ? const SizedBox(
                    width: 24,
                    height: 24,
                    child: CircularProgressIndicator(
                      strokeWidth: 2.5,
                      valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                    ),
                  )
                : DefaultTextStyle(
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: AppTypography.sizeBodyLg,
                      fontWeight: AppTypography.semiBold,
                      letterSpacing: 0.5,
                    ),
                    child: IconTheme(
                      data: const IconThemeData(color: Colors.white),
                      child: widget.child,
                    ),
                  ),
          ),
        ),
      ),
    );
  }
}

/// Gold accent gradient button variant.
class GoldButton extends StatelessWidget {
  const GoldButton({
    super.key,
    required this.onPressed,
    required this.child,
    this.isLoading = false,
    this.height = 56,
  });

  final VoidCallback? onPressed;
  final Widget child;
  final bool isLoading;
  final double height;

  @override
  Widget build(BuildContext context) {
    return GradientButton(
      onPressed: onPressed,
      gradient: AppColors.accentGradient,
      isLoading: isLoading,
      height: height,
      child: DefaultTextStyle(
        style: TextStyle(
          color: AppColors.primary,
          fontSize: AppTypography.sizeBodyLg,
          fontWeight: AppTypography.bold,
          letterSpacing: 0.5,
        ),
        child: child,
      ),
    );
  }
}
