import 'dart:async';
import 'dart:convert';
import 'dart:developer' as developer;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:signalr_core/signalr_core.dart';

import '../../../core/network/dio_provider.dart';
import '../../../core/storage/token_storage_service.dart';
import '../presentation/order_details_page.dart';
import '../presentation/orders_notifier.dart';

/// Customer-side SignalR listener for `OrderStatusUpdated` events emitted by the
/// backend after every order status transition. On each event we invalidate the
/// affected providers so the UI refetches fresh data (including the new Path 2
/// timestamps).
final customerOrderStreamServiceProvider =
    Provider<CustomerOrderStreamService>((ref) {
  final service = CustomerOrderStreamService(ref);
  ref.onDispose(service.dispose);
  return service;
});

class CustomerOrderStreamService {
  final Ref _ref;
  HubConnection? _connection;

  CustomerOrderStreamService(this._ref);

  Future<void> start() async {
    if (_connection != null &&
        (_connection!.state == HubConnectionState.connected ||
            _connection!.state == HubConnectionState.connecting)) {
      return;
    }

    try {
      final dio = _ref.read(dioProvider);
      final tokenStorage = _ref.read(tokenStorageServiceProvider);

      // Backend hub lives at `{origin}/hubs/notifications` — NOT under `/DeliveryDashApi`.
      final baseUri = Uri.parse(dio.options.baseUrl);
      final hubUri = baseUri.replace(path: '/hubs/notifications');
      final hubUrl = hubUri.toString();

      final connection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(
              accessTokenFactory: () async =>
                  (await tokenStorage.getAccessToken()) ?? '',
            ),
          )
          .withAutomaticReconnect()
          .build();

      connection.on('OrderStatusUpdated', _handleStatusUpdate);

      await connection.start();
      _connection = connection;
      developer.log('Customer order stream connected to $hubUrl',
          name: 'CustomerOrderStream');
    } catch (e) {
      developer.log(
        'Failed to start customer order stream',
        name: 'CustomerOrderStream',
        error: e,
      );
    }
  }

  Future<void> stop() async {
    final conn = _connection;
    _connection = null;
    if (conn != null) {
      try {
        await conn.stop();
      } catch (e) {
        developer.log('Error stopping customer order stream',
            name: 'CustomerOrderStream', error: e);
      }
    }
  }

  void _handleStatusUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;
    try {
      final payload = arguments.first;
      Map<String, dynamic>? data;
      if (payload is Map<String, dynamic>) {
        data = payload;
      } else if (payload is String) {
        final decoded = jsonDecode(payload);
        if (decoded is Map<String, dynamic>) data = decoded;
      }
      if (data == null) return;

      final rawId = data['orderId'] ?? data['OrderId'];
      final orderId = rawId is int ? rawId : int.tryParse(rawId?.toString() ?? '');
      if (orderId == null) return;

      // Refresh the list and the specific order detail so the UI shows the new
      // status and its timestamp.
      _ref.invalidate(orderDetailsProvider(orderId));
      _ref.invalidate(ordersProvider);
      _ref.read(ordersNotifierProvider.notifier).refresh();
    } catch (e) {
      developer.log(
        'Failed to handle OrderStatusUpdated',
        name: 'CustomerOrderStream',
        error: e,
      );
    }
  }

  void dispose() {
    unawaited(stop());
  }
}
