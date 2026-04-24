import 'dart:convert';
import 'dart:developer' as developer;

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';

final customerNotificationRepositoryProvider =
    Provider<CustomerNotificationRepository>((ref) {
  return CustomerNotificationRepository(ref.watch(dioProvider));
});

class CustomerNotification {
  final int id;
  final String title;
  final String message;
  final String type;
  final bool isRead;
  final DateTime createdAt;
  final String? actionUrl;
  final String? imageUrl;
  final Map<String, dynamic>? metadata;

  CustomerNotification({
    required this.id,
    required this.title,
    required this.message,
    required this.type,
    required this.isRead,
    required this.createdAt,
    this.actionUrl,
    this.imageUrl,
    this.metadata,
  });

  factory CustomerNotification.fromJson(Map<String, dynamic> json) {
    Map<String, dynamic>? meta;
    final rawMeta = json['metadata'];
    if (rawMeta is Map<String, dynamic>) {
      meta = rawMeta;
    } else if (rawMeta is String && rawMeta.isNotEmpty) {
      try {
        final decoded = jsonDecode(rawMeta);
        if (decoded is Map<String, dynamic>) meta = decoded;
      } catch (_) {}
    }
    return CustomerNotification(
      id: (json['id'] as num?)?.toInt() ?? 0,
      title: json['title']?.toString() ?? '',
      message: json['message']?.toString() ?? '',
      type: json['type']?.toString() ?? 'Info',
      isRead: json['isRead'] == true,
      createdAt: DateTime.tryParse(json['createdAt']?.toString() ?? '') ??
          DateTime.now(),
      actionUrl: json['actionUrl']?.toString(),
      imageUrl: json['imageUrl']?.toString(),
      metadata: meta,
    );
  }
}

class CustomerNotificationRepository {
  final Dio _dio;
  CustomerNotificationRepository(this._dio);

  Future<List<CustomerNotification>> list({int skip = 0, int take = 50}) async {
    try {
      final res = await _dio.get('/Notification', queryParameters: {
        'skip': skip,
        'take': take,
      });
      final data = res.data;
      if (data is List) {
        return data
            .map((e) => CustomerNotification.fromJson(e as Map<String, dynamic>))
            .toList();
      }
      return [];
    } catch (e) {
      developer.log('Failed to load notifications',
          name: 'CustomerNotifications', error: e);
      return [];
    }
  }

  Future<int> unreadCount() async {
    try {
      final res = await _dio.get('/Notification/unread-count');
      final c = res.data?['count'];
      if (c is num) return c.toInt();
      return 0;
    } catch (_) {
      return 0;
    }
  }

  Future<void> markAsRead(int id) async {
    try {
      await _dio.put('/Notification/$id/read');
    } catch (e) {
      developer.log('markAsRead failed',
          name: 'CustomerNotifications', error: e);
    }
  }

  Future<void> markAllAsRead() async {
    try {
      await _dio.put('/Notification/mark-all-read');
    } catch (e) {
      developer.log('markAllAsRead failed',
          name: 'CustomerNotifications', error: e);
    }
  }

  Future<void> registerDevice({
    required String token,
    required int platform,
  }) async {
    try {
      await _dio.post('/Notification/devices', data: {
        'token': token,
        'platform': platform,
      });
    } catch (e) {
      developer.log('registerDevice failed',
          name: 'CustomerNotifications', error: e);
    }
  }

  Future<void> deregisterDevice(String token) async {
    try {
      await _dio.delete('/Notification/devices/$token');
    } catch (e) {
      developer.log('deregisterDevice failed',
          name: 'CustomerNotifications', error: e);
    }
  }
}
