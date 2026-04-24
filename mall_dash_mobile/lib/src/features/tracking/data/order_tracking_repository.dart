import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';
import 'driver_location_model.dart';

final orderTrackingRepositoryProvider = Provider<OrderTrackingRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return OrderTrackingRepository(dio: dio);
});

class OrderTrackingRepository {
  final Dio _dio;

  OrderTrackingRepository({required Dio dio}) : _dio = dio;

  /// Returns the last known driver location for this order, or null if none.
  Future<DriverLocation?> getDriverLocation(int orderId) async {
    try {
      final response =
          await _dio.get('/OrderTracking/$orderId/driver-location');
      if (response.statusCode == 200 && response.data is Map<String, dynamic>) {
        return DriverLocation.fromJson(response.data as Map<String, dynamic>);
      }
      return null;
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) return null;
      rethrow;
    }
  }
}
