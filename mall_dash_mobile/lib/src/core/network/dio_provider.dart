import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'dart:async';
import 'dart:developer' as developer;
import '../storage/token_storage_service.dart';

// Create a stream controller for authentication errors that need to trigger logout
final authErrorStreamProvider = StreamProvider<void>((ref) {
  return ref.watch(_authErrorStreamControllerProvider).stream;
});

final _authErrorStreamControllerProvider = Provider((ref) {
  return StreamController<void>.broadcast();
});

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: 'https://delivery-dashboard-tpnq.onrender.com/DeliveryDashApi',
      contentType: 'application/json',
      connectTimeout: const Duration(seconds: 30),
      receiveTimeout: const Duration(seconds: 30),
    ),
  );

  final tokenStorage = ref.watch(tokenStorageServiceProvider);
  final authErrorController = ref.watch(_authErrorStreamControllerProvider);

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        // Don't add auth header to public/auth endpoints to avoid
        // stale tokens interfering with login, register, or refresh requests.
        final path = options.path.toLowerCase();
        final isAuthEndpoint = path.contains('/account/login') ||
            path.contains('/account/register') ||
            path.contains('/account/mobile/refresh') ||
            path.contains('/account/validate-token') ||
            path.contains('/account/forgot-password') ||
            path.contains('/account/reset-password');

        if (!isAuthEndpoint) {
          final accessToken = await tokenStorage.getAccessToken();
          if (accessToken != null && accessToken.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $accessToken';
            developer.log('Added auth token to request: ${options.path}', name: 'DioInterceptor');
          } else {
            developer.log('No access token available for request: ${options.path}', name: 'DioInterceptor');
          }
        } else {
          developer.log('Skipping auth header for auth endpoint: ${options.path}', name: 'DioInterceptor');
        }
        return handler.next(options);
      },
      onError: (DioException error, handler) async {
        developer.log(
          'Request failed: ${error.requestOptions.path}',
          name: 'DioInterceptor',
          error: 'Status: ${error.response?.statusCode}, Message: ${error.message}',
        );

        // Don't try to refresh tokens for auth endpoints themselves
        final errorPath = error.requestOptions.path.toLowerCase();
        final isAuthEndpoint = errorPath.contains('/account/login') ||
            errorPath.contains('/account/register') ||
            errorPath.contains('/account/mobile/refresh') ||
            errorPath.contains('/account/validate-token');

        // Handle 401 Unauthorized - token expired or invalid
        if (error.response?.statusCode == 401 && !isAuthEndpoint) {
          developer.log('Received 401 Unauthorized - attempting token refresh', name: 'DioInterceptor');
          
          final refreshToken = await tokenStorage.getRefreshToken();

          if (refreshToken != null && refreshToken.isNotEmpty) {
            try {
              // Create a new Dio instance to avoid circular dependencies/interceptors
              final refreshDio = Dio(
                BaseOptions(
                  baseUrl: 'https://delivery-dashboard-tpnq.onrender.com/DeliveryDashApi',
                  contentType: 'application/json',
                ),
              );

              developer.log('Attempting to refresh token...', name: 'DioInterceptor');
              
              final response = await refreshDio.post(
                '/Account/Mobile/refresh',
                data: {'refreshToken': refreshToken},
              );

              developer.log(
                'Token refresh response received',
                name: 'DioInterceptor',
                error: 'Status: ${response.statusCode}, Data keys: ${response.data?.keys}',
              );

              if (response.statusCode == 200 && response.data != null) {
                final data = response.data as Map<String, dynamic>;
                
                // Handle different possible response formats
                final newAccessToken = data['accessToken'] ?? data['access_token'];
                final newRefreshToken = data['refreshToken'] ?? data['refresh_token'];

                if (newAccessToken != null && newRefreshToken != null) {
                  developer.log('Token refresh successful, saving new tokens', name: 'DioInterceptor');
                  
                  await tokenStorage.saveTokens(
                    accessToken: newAccessToken.toString(),
                    refreshToken: newRefreshToken.toString(),
                  );

                  // Retry the original request with the new token
                  error.requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';

                  final opts = Options(
                    method: error.requestOptions.method,
                    headers: error.requestOptions.headers,
                  );

                  developer.log('Retrying original request with new token', name: 'DioInterceptor');
                  
                  final cloneReq = await dio.request(
                    error.requestOptions.path,
                    options: opts,
                    data: error.requestOptions.data,
                    queryParameters: error.requestOptions.queryParameters,
                  );

                  return handler.resolve(cloneReq);
                } else {
                  developer.log(
                    'Token refresh response missing required fields',
                    name: 'DioInterceptor',
                    error: 'Response data: $data',
                  );
                  throw Exception('Invalid refresh token response');
                }
              }
            } catch (e) {
              // Refresh failed - clear tokens and trigger logout
              developer.log(
                'Token refresh failed - clearing tokens and triggering logout',
                name: 'DioInterceptor',
                error: e.toString(),
              );
              
              await tokenStorage.clearTokens();
              
              // Notify auth system to logout
              authErrorController.add(null);
              
              // Reject with the original error — don't continue the chain
              return handler.reject(error);
            }
          } else {
            // No refresh token available - clear tokens and trigger logout
            developer.log('No refresh token available - clearing tokens', name: 'DioInterceptor');
            await tokenStorage.clearTokens();
            authErrorController.add(null);
            
            // Reject with the original error
            return handler.reject(error);
          }
        }
        
        return handler.next(error);
      },
      onResponse: (response, handler) {
        developer.log(
          'Response received: ${response.requestOptions.path}',
          name: 'DioInterceptor',
          error: 'Status: ${response.statusCode}',
        );
        return handler.next(response);
      },
    ),
  );

  return dio;
});
