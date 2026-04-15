import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:developer' as developer;
import '../../../core/storage/token_storage_service.dart';
import '../../../core/network/dio_provider.dart';
import '../data/auth_repository.dart';
import 'login_controller.dart';
import '../../profile/presentation/user_profile_notifier.dart';
import '../../order/presentation/orders_notifier.dart';
import '../../order/presentation/cart_notifier.dart';
import '../../support/presentation/support_tickets_notifier.dart';
import '../../driver/presentation/driver_shift_notifier.dart';
import '../../driver/presentation/driver_orders_notifier.dart';

// Simple enum to represent auth state
enum AuthStatus { initial, authenticated, unauthenticated }

final authNotifierProvider = NotifierProvider<AuthNotifier, AuthStatus>(AuthNotifier.new);

class AuthNotifier extends Notifier<AuthStatus> {
  @override
  AuthStatus build() {
    // Listen to auth errors from Dio interceptor and trigger logout
    ref.listen(authErrorStreamProvider, (previous, next) {
      next.whenData((_) {
        developer.log('Auth error received - logging out', name: 'AuthNotifier');
        state = AuthStatus.unauthenticated;
      });
    });
    
    return AuthStatus.initial;
  }

  Future<void> checkAuthStatus() async {
    developer.log('Checking authentication status...', name: 'AuthNotifier');
    
    final tokenService = ref.read(tokenStorageServiceProvider);
    final accessToken = await tokenService.getAccessToken();
    final refreshToken = await tokenService.getRefreshToken();

    developer.log(
      'Token status',
      name: 'AuthNotifier',
      error: 'Access: ${accessToken != null ? "present" : "missing"}, Refresh: ${refreshToken != null ? "present" : "missing"}',
    );

    if (refreshToken != null && refreshToken.isNotEmpty) {
      // Validate refresh token with backend
      try {
        final repository = ref.read(authRepositoryProvider);
        final isValid = await repository.validateToken();
        
        if (isValid) {
          // Refresh token is valid - if access token exists, assume authenticated.
          // The Dio interceptor will handle refreshing the access token if needed.
          if (accessToken != null && accessToken.isNotEmpty) {
            developer.log('Token validated successfully', name: 'AuthNotifier');
            state = AuthStatus.authenticated;
          } else {
            // Refresh token valid but no access token - try refreshing now
            developer.log('Refresh token valid but no access token - attempting refresh', name: 'AuthNotifier');
            final refreshed = await repository.tryRefreshTokens();
            if (refreshed) {
              developer.log('Token refresh successful', name: 'AuthNotifier');
              state = AuthStatus.authenticated;
            } else {
              developer.log('Token refresh failed - clearing all user data', name: 'AuthNotifier');
              await tokenService.clearTokens();
              ref.invalidate(userProfileProvider);
              state = AuthStatus.unauthenticated;
            }
          }
        } else {
          developer.log('Token validation failed - clearing all user data', name: 'AuthNotifier');
          await tokenService.clearTokens();
          ref.invalidate(userProfileProvider);
          state = AuthStatus.unauthenticated;
        }
      } catch (e) {
        developer.log('Token validation error - clearing all user data', name: 'AuthNotifier', error: e.toString());
        await tokenService.clearTokens();
        ref.invalidate(userProfileProvider);
        state = AuthStatus.unauthenticated;
      }
    } else {
      developer.log('No tokens found - user not authenticated', name: 'AuthNotifier');
      state = AuthStatus.unauthenticated;
    }
  }

  Future<void> logout() async {
    developer.log('Logout initiated', name: 'AuthNotifier');
    
    try {
      final repository = ref.read(authRepositoryProvider);
      await repository.logout();
      developer.log('Backend logout successful', name: 'AuthNotifier');
    } catch (e) {
      developer.log('Backend logout failed', name: 'AuthNotifier', error: e.toString());
    } finally {
      // CRITICAL: Invalidate ALL user-specific providers to prevent data leakage between users
      developer.log('Invalidating all user-specific providers', name: 'AuthNotifier');
      
      // Auth/login state
      ref.invalidate(loginControllerProvider);
      
      // Profile data
      ref.invalidate(userProfileProvider);
      
      // Tenant/Customer data
      ref.invalidate(ordersProvider);
      ref.invalidate(ordersNotifierProvider);
      ref.invalidate(cartProvider);
      ref.invalidate(supportTicketsProvider);
      
      // Driver data
      ref.invalidate(driverShiftNotifierProvider);
      ref.invalidate(queuePositionProvider);
      ref.invalidate(availableOrdersNotifierProvider);
      ref.invalidate(activeDeliveriesNotifierProvider);
      
      // Ensure state is updated even if logout fails
      state = AuthStatus.unauthenticated;
      developer.log('Auth state set to unauthenticated - all user data cleared', name: 'AuthNotifier');
    }
  }

  // Call this when login is successful
  void setAuthenticated() {
    developer.log('User authenticated', name: 'AuthNotifier');
    
    // CRITICAL: Invalidate user profile to force fresh fetch with new user's data
    developer.log('Invalidating user profile to fetch fresh data', name: 'AuthNotifier');
    ref.invalidate(userProfileProvider);
    
    state = AuthStatus.authenticated;
  }
}
