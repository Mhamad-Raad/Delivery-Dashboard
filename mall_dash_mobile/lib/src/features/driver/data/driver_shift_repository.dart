import 'package:dio/dio.dart';
import 'driver_shift_model.dart';

class DriverShiftRepository {
  final Dio _dio;

  DriverShiftRepository(this._dio);

  /// Start a new driver shift
  /// POST /MalDashApi/DriverShift/start
  Future<DriverShift?> startShift() async {
    try {
      print('🚀 Starting driver shift...');
      print('🔗 Endpoint: POST /DriverShift/start');

      final response = await _dio.post('/DriverShift/start');

      print('✅ Start shift response status: ${response.statusCode}');
      print('📦 Start shift response data type: ${response.data.runtimeType}');
      print('📄 Start shift response data: ${response.data}');

      if (response.statusCode == 200) {
        if (response.data != null && response.data is Map<String, dynamic>) {
          final data = response.data as Map<String, dynamic>;
          print('🔑 Start shift response keys: ${data.keys.toList()}');
          return DriverShift.fromJson(data);
        }
        // Return a default active shift if no data returned
        return DriverShift(isActive: true, startTime: DateTime.now());
      }
    } on DioException catch (e) {
      print('❌ DioException starting shift');
      print('📊 Status code: ${e.response?.statusCode}');
      print('📦 Error response type: ${e.response?.data.runtimeType}');
      print('📄 Error response data: ${e.response?.data}');
      print('💬 Error message: ${e.message}');
      print('🔍 Request path: ${e.requestOptions.path}');
      print('🔍 Request method: ${e.requestOptions.method}');
      print('🔍 Request headers: ${e.requestOptions.headers}');
      
      if (e.response?.statusCode == 400) {
        // Parse error message from backend
        final errorData = e.response?.data;
        String errorMessage = 'Failed to start shift: Validation error';
        
        // Try to extract detailed error message
        if (errorData is Map) {
          print('🔍 Error data keys: ${errorData.keys.toList()}');
          
          if (errorData.containsKey('errors')) {
            final errors = errorData['errors'];
            print('🔍 Validation errors: $errors');
            errorMessage = 'Validation failed: $errors';
          } else if (errorData.containsKey('message')) {
            errorMessage = errorData['message'];
          } else if (errorData.containsKey('error')) {
            errorMessage = errorData['error'];
          } else if (errorData.containsKey('title')) {
            errorMessage = errorData['title'];
          } else {
            errorMessage = 'Validation error: $errorData';
          }
        } else if (errorData is String) {
          errorMessage = errorData;
        }
        
        print('⚠️ Final error message: $errorMessage');
        throw Exception(errorMessage);
      } else if (e.response?.statusCode == 401) {
        throw Exception('Not authorized. Please login again.');
      } else if (e.response?.statusCode == 403) {
        throw Exception('Access denied. Driver role required.');
      } else if (e.response?.statusCode == 409) {
        // Conflict - probably already has an active shift
        throw Exception('You already have an active shift. Please end it first.');
      }
      
      rethrow;
    } catch (e) {
      print('💥 Unexpected error starting shift: $e');
      rethrow;
    }
    
    return null;
  }

  /// End the current driver shift
  /// POST /MalDashApi/DriverShift/end
  Future<bool> endShift() async {
    try {
      print('Ending driver shift...');

      final response = await _dio.post('/DriverShift/end');

      print('End shift response status: ${response.statusCode}');
      print('End shift response data: ${response.data}');

      return response.statusCode == 200;
    } catch (e) {
      print('Error ending shift: $e');
      rethrow;
    }
  }

  /// Get current active shift
  /// GET /MalDashApi/DriverShift/current
  Future<DriverShift?> getCurrentShift() async {
    try {
      print('Getting current shift...');

      final response = await _dio.get('/DriverShift/current');

      print('Get current shift response status: ${response.statusCode}');
      print(
        'Get current shift response data type: ${response.data.runtimeType}',
      );
      print('Get current shift response data: ${response.data}');

      if (response.data != null && response.data is Map<String, dynamic>) {
        final data = response.data as Map<String, dynamic>;
        print('Current shift response keys: ${data.keys.toList()}');
        return DriverShift.fromJson(data);
      }

      return null;
    } catch (e) {
      print('Error getting current shift: $e');
      // Return inactive shift on error
      return DriverShift(isActive: false);
    }
  }

  /// Get driver's queue position
  /// GET /MalDashApi/DriverShift/queue-position
  Future<QueuePosition?> getQueuePosition() async {
    try {
      print('Getting queue position...');

      final response = await _dio.get('/DriverShift/queue-position');

      print('Queue position response status: ${response.statusCode}');
      print('Queue position response data type: ${response.data.runtimeType}');
      print('Queue position response data: ${response.data}');

      if (response.data != null) {
        if (response.data is Map<String, dynamic>) {
          final data = response.data as Map<String, dynamic>;
          print('Queue position response keys: ${data.keys.toList()}');
          return QueuePosition.fromJson(data);
        } else if (response.data is int) {
          // If response is just a number, assume it's the position
          return QueuePosition(position: response.data as int, totalDrivers: 0);
        }
      }

      return null;
    } catch (e) {
      print('Error getting queue position: $e');
      return null;
    }
  }
}
