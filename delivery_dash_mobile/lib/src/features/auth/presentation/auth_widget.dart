import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'auth_notifier.dart';
import 'login_page.dart';
import 'splash_screen.dart';
import '../../../core/presentation/main_scaffold.dart';

class AuthWidget extends ConsumerStatefulWidget {
  const AuthWidget({super.key});

  @override
  ConsumerState<AuthWidget> createState() => _AuthWidgetState();
}

class _AuthWidgetState extends ConsumerState<AuthWidget> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(authNotifierProvider.notifier).checkAuthStatus());
  }

  @override
  Widget build(BuildContext context) {
    final authStatus = ref.watch(authNotifierProvider);

    return AnimatedSwitcher(
      duration: const Duration(milliseconds: 500),
      switchInCurve: Curves.easeOutCubic,
      switchOutCurve: Curves.easeInCubic,
      transitionBuilder: (child, animation) {
        return FadeTransition(
          opacity: animation,
          child: child,
        );
      },
      child: _buildForStatus(authStatus),
    );
  }

  Widget _buildForStatus(AuthStatus status) {
    switch (status) {
      case AuthStatus.initial:
        return const SplashScreen(key: ValueKey('splash'));
      case AuthStatus.authenticated:
        return const MainScaffold(key: ValueKey('main'));
      case AuthStatus.unauthenticated:
        return const LoginPage(key: ValueKey('login'));
    }
  }
}
