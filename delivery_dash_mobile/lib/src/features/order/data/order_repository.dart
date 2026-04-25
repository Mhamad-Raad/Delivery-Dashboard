import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';
import 'order_model.dart';

final orderRepositoryProvider = Provider<OrderRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return OrderRepository(dio: dio);
});

class OrderRepository {
  final Dio _dio;

  OrderRepository({required Dio dio}) : _dio = dio;

  Future<Order> createOrder(CreateOrderRequest request) async {
    try {
      final response = await _dio.post(
        '/Order',
        data: request.toJson(),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        if (response.data == null) {
          throw Exception('Server returned null data');
        }
        
        final data = response.data as Map<String, dynamic>;
        
        // Backend returns {message: "...", order: {...}}
        if (data.containsKey('order')) {
          return Order.fromJson(data['order'] as Map<String, dynamic>);
        }
        
        return Order.fromJson(data);
      } else {
        throw Exception('Failed to create order: ${response.statusCode}');
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final errorMessage = e.response?.data?['message'] ?? 
                            e.response?.data?['error'] ?? 
                            'Invalid order data';
        throw Exception('Order validation failed: $errorMessage');
      } else if (e.response?.statusCode == 403) {
        throw Exception('Access denied. You may not have permission to create orders.');
      } else if (e.response?.statusCode == 401) {
        throw Exception('Authentication required. Please log in again.');
      }
      throw Exception('Failed to create order: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      rethrow;
    }
  }

  Future<List<Order>> getOrders({
    int page = 1,
    int limit = 20,
    String? status,
  }) async {
    try {
      final response = await _dio.get(
        '/Order',
        queryParameters: {
          'page': page,
          'limit': limit,
          if (status != null) 'status': status,
        },
      );

      if (response.statusCode == 200) {
        final dynamic data = response.data;
        
        if (data is Map<String, dynamic>) {
          final items = data['items'] ?? data['data'] ?? [];
          return (items as List).map((json) => Order.fromJson(json as Map<String, dynamic>)).toList();
        } else if (data is List) {
          return data.map((json) => Order.fromJson(json as Map<String, dynamic>)).toList();
        } else {
          return [];
        }
      } else {
        throw Exception('Failed to load orders: ${response.statusCode}');
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 403) {
        throw Exception('Access denied. Your account may not have permission to view orders.');
      } else if (e.response?.statusCode == 401) {
        throw Exception('Authentication required. Please log in again.');
      }
      throw Exception('Failed to load orders: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      rethrow;
    }
  }

  Future<Order> getOrderById(int id) async {
    try {
      final response = await _dio.get('/Order/$id');

      if (response.statusCode == 200) {
        return Order.fromJson(response.data as Map<String, dynamic>);
      } else {
        throw Exception('Failed to load order: ${response.statusCode}');
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Order not found');
      }
      throw Exception('Failed to load order: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      rethrow;
    }
  }

  Future<Order> getOrderByNumber(String orderNumber) async {
    try {
      final response = await _dio.get('/Order/number/$orderNumber');

      if (response.statusCode == 200) {
        return Order.fromJson(response.data as Map<String, dynamic>);
      } else {
        throw Exception('Failed to load order: ${response.statusCode}');
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        throw Exception('Order not found');
      }
      throw Exception('Failed to load order: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      rethrow;
    }
  }

  Future<void> cancelOrder(int id) async {
    try {
      print('Attempting to cancel order ID: $id');
      final response = await _dio.post('/Order/$id/cancel');
      print('Cancel order response status: ${response.statusCode}');
      print('Cancel order response data: ${response.data}');

      if (response.statusCode != 200) {
        throw Exception('Failed to cancel order: ${response.statusCode}');
      }
      
      print('Order $id cancelled successfully');
    } on DioException catch (e) {
      print('DioException cancelling order: ${e.response?.statusCode} - ${e.response?.data}');
      if (e.response?.statusCode == 400) {
        final errorData = e.response?.data;
        String errorMessage = 'Cannot cancel this order.';
        
        if (errorData is Map<String, dynamic>) {
          errorMessage = errorData['error'] ?? errorData['message'] ?? errorMessage;
        }
        
        throw Exception(errorMessage);
      } else if (e.response?.statusCode == 404) {
        throw Exception('Order not found');
      }
      throw Exception('Failed to cancel order: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      print('Error cancelling order: $e');
      rethrow;
    }
  }
}
