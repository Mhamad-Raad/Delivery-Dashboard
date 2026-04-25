import 'package:dio/dio.dart';
import 'notification_model.dart';

class NotificationRepository {
  final Dio _dio;

  NotificationRepository(this._dio);

  /// Get list of notifications with pagination
  /// GET /MalDashApi/Notification?skip={skip}&take={take}
  Future<List<AppNotification>> getNotifications({int skip = 0, int take = 20}) async {
    try {
      final response = await _dio.get('/Notification', queryParameters: {'skip': skip, 'take': take});

      if (response.statusCode == 200 && response.data != null) {
        if (response.data is List) {
          return (response.data as List).map((json) => AppNotification.fromJson(json as Map<String, dynamic>)).toList();
        }
      }

      return [];
    } catch (e) {
      print('❌ Error fetching notifications: $e');
      return [];
    }
  }

  /// Get count of unread notifications
  /// GET /MalDashApi/Notification/unread-count
  Future<int> getUnreadCount() async {
    try {
      final response = await _dio.get('/Notification/unread-count');

      if (response.statusCode == 200 && response.data != null) {
        // Backend might return { "count": 5 } or just 5
        if (response.data is Map) {
          return response.data['count'] as int? ?? 0;
        } else if (response.data is int) {
          return response.data as int;
        }
      }

      return 0;
    } catch (e) {
      print('❌ Error fetching unread count: $e');
      return 0;
    }
  }

  /// Mark a specific notification as read
  /// POST /MalDashApi/Notification/{id}/read
  Future<bool> markAsRead(int notificationId) async {
    try {
      final response = await _dio.post(
        '/Notification/$notificationId/read',
        options: Options(
          validateStatus: (status) {
            if (status == null) return false;
            return status < 500;
          },
        ),
      );
      return response.statusCode == 200;
    } catch (e) {
      print('❌ Error marking notification as read: $e');
      return false;
    }
  }

  /// Mark all notifications as read
  /// POST /MalDashApi/Notification/mark-all-read
  Future<bool> markAllAsRead() async {
    try {
      final response = await _dio.post('/Notification/mark-all-read');
      return response.statusCode == 200;
    } catch (e) {
      print('❌ Error marking all notifications as read: $e');
      return false;
    }
  }

  /// Get a specific notification by ID
  /// GET /MalDashApi/Notification/{id}
  Future<AppNotification?> getNotificationById(int id) async {
    try {
      final response = await _dio.get('/Notification/$id');

      if (response.statusCode == 200 && response.data != null) {
        return AppNotification.fromJson(response.data as Map<String, dynamic>);
      }

      return null;
    } catch (e) {
      print('❌ Error fetching notification $id: $e');
      return null;
    }
  }
}
