import 'package:dio/dio.dart';
import 'driver_order_model.dart';

class DriverDispatchRepository {
  final Dio _dio;

  DriverDispatchRepository(this._dio);

  /// Accept an assigned order
  /// POST /MalDashApi/DriverDispatch/accept
  Future<bool> acceptOrder(int assignmentId) async {
    try {
      print('Accepting order with assignmentId: $assignmentId');

      final request = OrderAssignmentRequest(assignmentId: assignmentId);

      final response = await _dio.post(
        '/DriverDispatch/accept',
        data: request.toJson(),
        options: Options(contentType: Headers.jsonContentType),
      );

      print('Accept order response status: ${response.statusCode}');
      print('Accept order response data: ${response.data}');

      return response.statusCode == 200;
    } catch (e) {
      print('Error accepting order: $e');
      rethrow;
    }
  }

  /// Reject an assigned order
  /// POST /MalDashApi/DriverDispatch/reject
  Future<bool> rejectOrder(int assignmentId) async {
    try {
      print('Rejecting order with assignmentId: $assignmentId');

      final request = OrderAssignmentRequest(assignmentId: assignmentId);

      final response = await _dio.post(
        '/DriverDispatch/reject',
        data: request.toJson(),
        options: Options(contentType: Headers.jsonContentType),
      );

      print('Reject order response status: ${response.statusCode}');
      print('Reject order response data: ${response.data}');

      return response.statusCode == 200;
    } catch (e) {
      print('Error rejecting order: $e');
      rethrow;
    }
  }

  /// Complete a delivery
  /// POST /MalDashApi/DriverDispatch/complete/{orderId}
  Future<bool> completeDelivery(int orderId) async {
    try {
      print('Completing delivery for orderId: $orderId');

      final response = await _dio.post('/DriverDispatch/complete/$orderId');

      print('Complete delivery response status: ${response.statusCode}');
      print('Complete delivery response data: ${response.data}');

      return response.statusCode == 200;
    } catch (e) {
      print('Error completing delivery: $e');
      rethrow;
    }
  }
}
