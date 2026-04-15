import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:developer' as developer;
import '../../../core/network/dio_provider.dart';
import '../../../core/storage/token_storage_service.dart';

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  final dio = ref.watch(dioProvider);
  final tokenStorage = ref.watch(tokenStorageServiceProvider);
  return AuthRepository(dio: dio, tokenStorage: tokenStorage);
});

class AuthRepository {
  final Dio _dio;
  final TokenStorageService _tokenStorage;

  AuthRepository({required Dio dio, required TokenStorageService tokenStorage})
    : _dio = dio,
      _tokenStorage = tokenStorage;

  Future<Map<String, dynamic>> login({
    required String email,
    required String password,
    required String applicationContext,
  }) async {
    try {
      developer.log('Attempting login for: $email', name: 'AuthRepository');
      
      final response = await _dio.post(
        '/Account/login/mobile',
        data: {
          'email': email,
          'password': password,
          'applicationContext': applicationContext,
        },
      );

      developer.log(
        'Login response received',
        name: 'AuthRepository',
        error: 'Status: ${response.statusCode}, Data type: ${response.data.runtimeType}',
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = response.data as Map<String, dynamic>;
        
        developer.log(
          'Login response data',
          name: 'AuthRepository',
          error: 'Keys: ${data.keys.join(", ")}',
        );

        // Handle different possible response formats:
        // Format 1: {accessToken, refreshToken, ...}
        // Format 2: {access_token, refresh_token, ...}
        // Format 3: {token: {accessToken, refreshToken}}
        // Format 4: {data: {accessToken, refreshToken}}
        var accessToken = data['accessToken'] ?? data['access_token'];
        var refreshToken = data['refreshToken'] ?? data['refresh_token'];

        // Check nested formats if not found at top level
        if (accessToken == null || refreshToken == null) {
          final nested = data['token'] ?? data['data'];
          if (nested is Map<String, dynamic>) {
            accessToken ??= nested['accessToken'] ?? nested['access_token'];
            refreshToken ??= nested['refreshToken'] ?? nested['refresh_token'];
          }
        }

        if (accessToken != null && refreshToken != null) {
          developer.log('Saving authentication tokens', name: 'AuthRepository');
          await _tokenStorage.saveTokens(
            accessToken: accessToken.toString(),
            refreshToken: refreshToken.toString(),
          );
        } else {
          developer.log(
            'CRITICAL: Login response missing tokens!',
            name: 'AuthRepository',
            error: 'Response keys: ${data.keys.join(", ")}, Full data: $data',
          );
          throw Exception('Login succeeded but server did not return authentication tokens');
        }

        return data;
      } else {
        throw Exception('Login failed: ${response.statusCode} - ${response.statusMessage}');
      }
    } on DioException catch (e) {
      developer.log('Login failed with DioException', name: 'AuthRepository', error: e.toString());
      
      String errorMessage = 'Login failed';
      if (e.response?.data != null) {
        final errorData = e.response!.data;
        if (errorData is Map && errorData.containsKey('message')) {
          errorMessage = errorData['message'];
        } else if (errorData is String) {
          errorMessage = errorData;
        }
      }
      
      throw Exception(errorMessage);
    } catch (e) {
      developer.log('Login failed with exception', name: 'AuthRepository', error: e.toString());
      rethrow;
    }
  }

  Future<bool> validateToken() async {
    try {
      developer.log('Validating token with backend', name: 'AuthRepository');
      
      final refreshToken = await _tokenStorage.getRefreshToken();
      if (refreshToken == null || refreshToken.isEmpty) {
        developer.log('No refresh token available for validation', name: 'AuthRepository');
        return false;
      }

      // Use a separate Dio instance to avoid the auth interceptor adding
      // potentially stale access tokens and triggering refresh loops.
      final validationDio = Dio(
        BaseOptions(
          baseUrl: _dio.options.baseUrl,
          contentType: 'application/json',
          connectTimeout: const Duration(seconds: 15),
          receiveTimeout: const Duration(seconds: 15),
        ),
      );

      final response = await validationDio.post(
        '/Account/validate-token',
        data: {'refreshToken': refreshToken},
        options: Options(
          validateStatus: (status) {
            if (status == null) return false;
            return true;
          },
        ),
      );
      
      final status = response.statusCode ?? 0;

      developer.log(
        'Token validation response',
        name: 'AuthRepository',
        error: 'Status: $status',
      );

      if (status == 200) {
        return true;
      }

      // Any non-200 means the token is not valid
      return false;
    } on DioException catch (e) {
      developer.log(
        'Token validation failed',
        name: 'AuthRepository',
        error: 'Status: ${e.response?.statusCode}, Message: ${e.message}',
      );
      return false;
    } catch (e) {
      developer.log('Token validation error', name: 'AuthRepository', error: e.toString());
      return false;
    }
  }

  /// Attempt to refresh the access token using the stored refresh token.
  /// Returns true if refresh succeeded and new tokens were saved.
  Future<bool> tryRefreshTokens() async {
    try {
      final refreshToken = await _tokenStorage.getRefreshToken();
      if (refreshToken == null || refreshToken.isEmpty) return false;

      developer.log('Attempting to refresh tokens...', name: 'AuthRepository');

      final refreshDio = Dio(
        BaseOptions(
          baseUrl: _dio.options.baseUrl,
          contentType: 'application/json',
          connectTimeout: const Duration(seconds: 15),
          receiveTimeout: const Duration(seconds: 15),
        ),
      );

      final response = await refreshDio.post(
        '/Account/Mobile/refresh',
        data: {'refreshToken': refreshToken},
      );

      if (response.statusCode == 200 && response.data != null) {
        final data = response.data as Map<String, dynamic>;
        final newAccessToken = data['accessToken'] ?? data['access_token'];
        final newRefreshToken = data['refreshToken'] ?? data['refresh_token'];

        if (newAccessToken != null && newRefreshToken != null) {
          await _tokenStorage.saveTokens(
            accessToken: newAccessToken.toString(),
            refreshToken: newRefreshToken.toString(),
          );
          developer.log('Token refresh successful', name: 'AuthRepository');
          return true;
        }
      }

      developer.log('Token refresh returned unexpected response', name: 'AuthRepository');
      return false;
    } catch (e) {
      developer.log('Token refresh failed', name: 'AuthRepository', error: e.toString());
      return false;
    }
  }

  Future<void> logout() async {
    try {
      developer.log('Calling backend logout endpoint', name: 'AuthRepository');
      
      await _dio.post('/Account/logout/mobile');
      
      developer.log('Backend logout successful', name: 'AuthRepository');
    } on DioException catch (e) {
      developer.log(
        'Backend logout failed',
        name: 'AuthRepository',
        error: 'Status: ${e.response?.statusCode}, Message: ${e.message}',
      );
      // Continue to clear local tokens even if backend call fails
    } catch (e) {
      developer.log('Logout error', name: 'AuthRepository', error: e.toString());
    } finally {
      // Always clear local tokens
      developer.log('Clearing local tokens', name: 'AuthRepository');
      await _tokenStorage.clearTokens();
    }
  }
}
