import 'package:dio/dio.dart';
import 'driver_order_model.dart';

class DriverOrdersRepository {
  final Dio _dio;

  DriverOrdersRepository(this._dio);

  /// Get paginated list of orders
  /// GET /MalDashApi/Order
  Future<List<DriverOrder>> getOrders({
    int page = 1,
    int limit = 10,
    String? status,
  }) async {
    try {
      print('Getting orders - page: $page, limit: $limit, status: $status');

      final queryParams = <String, dynamic>{'page': page, 'limit': limit};

      if (status != null && status.isNotEmpty) {
        queryParams['status'] = status;
      }

      final response = await _dio.get('/Order', queryParameters: queryParams);

      print('Get orders response status: ${response.statusCode}');
      print('Get orders response data type: ${response.data.runtimeType}');
      print('Get orders response data: ${response.data}');

      if (response.data != null) {
        if (response.data is List) {
          final data = response.data as List;
          print('Orders list length: ${data.length}');
          return data.map((order) => DriverOrder.fromJson(order)).toList();
        } else if (response.data is Map<String, dynamic>) {
          final data = response.data as Map<String, dynamic>;
          print('Orders response keys: ${data.keys.toList()}');

          // Check for nested data structure
          if (data.containsKey('orders') && data['orders'] is List) {
            final orders = data['orders'] as List;
            return orders.map((order) => DriverOrder.fromJson(order)).toList();
          } else if (data.containsKey('data') && data['data'] is List) {
            final orders = data['data'] as List;
            return orders.map((order) => DriverOrder.fromJson(order)).toList();
          } else if (data.containsKey('items') && data['items'] is List) {
            final orders = data['items'] as List;
            return orders.map((order) => DriverOrder.fromJson(order)).toList();
          }
        }
      }

      return [];
    } catch (e) {
      print('Error getting orders: $e');
      return [];
    }
  }

  /// Get order by ID
  /// GET /MalDashApi/Order/{id}
  Future<DriverOrder?> getOrderById(int id) async {
    try {
      print('========================================');
      print('🔍 FETCHING ORDER BY ID');
      print('   Endpoint: /Order/$id');
      print('========================================');

      final response = await _dio.get('/Order/$id');

      print('📡 API Response:');
      print('   Status: ${response.statusCode}');
      print('   Data type: ${response.data.runtimeType}');

      if (response.data != null && response.data is Map<String, dynamic>) {
        final data = response.data as Map<String, dynamic>;
        print('   Keys: ${data.keys.toList()}');
        print('   Full data: $data');

        final order = DriverOrder.fromJson(data);
        print('✅ Order parsed successfully:');
        print('   ID: ${order.id}');
        print('   Number: ${order.orderNumber}');
        print('   Status: ${order.status} (${order.statusName})');
        print('   Customer: ${order.customerName}');
        print('   Phone: ${order.customerPhone}');
        print('   Address: ${order.deliveryAddress}');
        print('   Total: ${order.totalAmount}');
        print('   Items count: ${order.items?.length ?? 0}');
        print('========================================');
        return order;
      }

      print('⚠️ Response data is null or not a Map');
      print('========================================');
      return null;
    } catch (e) {
      print('========================================');
      print('❌ ERROR FETCHING ORDER $id');
      print('   Error: $e');
      print('========================================');
      rethrow;
    }
  }

  /// Get order by order number
  /// GET /MalDashApi/Order/number/{orderNumber}
  Future<DriverOrder?> getOrderByNumber(String orderNumber) async {
    try {
      print('========================================');
      print('🔍 FETCHING ORDER BY NUMBER');
      print('   Endpoint: /Order/number/$orderNumber');
      print('========================================');

      final response = await _dio.get('/Order/number/$orderNumber');

      print('📡 API Response:');
      print('   Status: ${response.statusCode}');
      print('   Data type: ${response.data.runtimeType}');

      if (response.data != null && response.data is Map<String, dynamic>) {
        final data = response.data as Map<String, dynamic>;
        print('   Keys: ${data.keys.toList()}');
        print('   Full data: $data');

        final order = DriverOrder.fromJson(data);
        print('✅ Order parsed successfully:');
        print('   ID: ${order.id}');
        print('   Number: ${order.orderNumber}');
        print('   Status: ${order.status} (${order.statusName})');
        print('   Customer: ${order.customerName}');
        print('   Phone: ${order.customerPhone}');
        print('   Address: ${order.deliveryAddress}');
        print('   Total: ${order.totalAmount}');
        print('   Items count: ${order.items?.length ?? 0}');
        print('========================================');
        return order;
      }

      print('⚠️ Response data is null or not a Map');
      print('========================================');
      return null;
    } catch (e) {
      print('========================================');
      print('❌ ERROR FETCHING ORDER BY NUMBER $orderNumber');
      print('   Error: $e');
      print('========================================');
      rethrow;
    }
  }

  /// Update order status
  /// PUT /MalDashApi/Order/{id}/status
  Future<bool> updateOrderStatus(int id, int status) async {
    try {
      print('Updating order status - ID: $id, Status: $status');

      final request = UpdateOrderStatusRequest(status: status);

      final response = await _dio.put(
        '/Order/$id/status',
        data: request.toJson(),
        options: Options(contentType: Headers.jsonContentType),
      );

      print('Update order status response status: ${response.statusCode}');
      print('Update order status response data: ${response.data}');

      return response.statusCode == 200;
    } catch (e) {
      print('Error updating order status: $e');
      rethrow;
    }
  }
}
