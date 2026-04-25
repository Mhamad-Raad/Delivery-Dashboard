import 'dart:async';
import 'dart:developer' as developer;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:signalr_core/signalr_core.dart';

import '../../../core/network/dio_provider.dart';
import '../../../core/storage/token_storage_service.dart';

final driverLocationTrackerProvider = Provider<DriverLocationTrackerService>((ref) {
  final service = DriverLocationTrackerService(ref);
  ref.onDispose(service.dispose);
  return service;
});

class DriverLocationTrackerService {
  DriverLocationTrackerService(this._ref);

  static const _tickInterval = Duration(seconds: 10);

  final Ref _ref;
  HubConnection? _connection;
  Timer? _timer;
  bool _connecting = false;
  bool _permissionDenied = false;
  final Set<int> _orderIds = {};

  /// Replace the active-order set. If non-empty, ensures the tracker is running.
  /// If empty, stops everything.
  Future<void> syncActiveOrders(Iterable<int> orderIds) async {
    final next = orderIds.toSet();
    if (_orderIds.length == next.length && _orderIds.containsAll(next)) {
      return;
    }
    _orderIds
      ..clear()
      ..addAll(next);

    if (_orderIds.isEmpty) {
      await _stop();
    } else {
      await _start();
    }
  }

  Future<void> _start() async {
    if (_timer != null) return;
    if (_permissionDenied) return;

    if (!await _ensurePermission()) {
      _permissionDenied = true;
      developer.log('Location permission denied; tracker idle',
          name: 'DriverTracker');
      return;
    }

    await _ensureConnection();
    _timer = Timer.periodic(_tickInterval, (_) => _tick());
    unawaited(_tick());
  }

  Future<bool> _ensurePermission() async {
    if (!await Geolocator.isLocationServiceEnabled()) return false;
    var perm = await Geolocator.checkPermission();
    if (perm == LocationPermission.denied) {
      perm = await Geolocator.requestPermission();
    }
    return perm == LocationPermission.always ||
        perm == LocationPermission.whileInUse;
  }

  Future<void> _ensureConnection() async {
    if (_connecting) return;
    final existing = _connection;
    if (existing != null &&
        (existing.state == HubConnectionState.connected ||
            existing.state == HubConnectionState.connecting)) {
      return;
    }

    _connecting = true;
    try {
      final dio = _ref.read(dioProvider);
      final tokenStorage = _ref.read(tokenStorageServiceProvider);
      final baseUri = Uri.parse(dio.options.baseUrl);
      final hubUri = baseUri.replace(path: '/hubs/tracking');

      final connection = HubConnectionBuilder()
          .withUrl(
            hubUri.toString(),
            HttpConnectionOptions(
              accessTokenFactory: () async =>
                  (await tokenStorage.getAccessToken()) ?? '',
            ),
          )
          .withAutomaticReconnect()
          .build();

      await connection.start();
      _connection = connection;
      developer.log('Driver tracker connected to $hubUri',
          name: 'DriverTracker');
    } catch (e) {
      developer.log('Driver tracker connect failed',
          name: 'DriverTracker', error: e);
    } finally {
      _connecting = false;
    }
  }

  Future<void> _tick() async {
    if (_orderIds.isEmpty) return;
    try {
      final pos = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );
      await _ensureConnection();
      final conn = _connection;
      if (conn == null || conn.state != HubConnectionState.connected) return;

      for (final orderId in _orderIds.toList()) {
        try {
          await conn.invoke('PushLocation', args: [
            orderId,
            pos.latitude,
            pos.longitude,
            pos.heading,
          ]);
        } catch (e) {
          developer.log('PushLocation failed for order $orderId',
              name: 'DriverTracker', error: e);
        }
      }
    } catch (e) {
      developer.log('Tracker tick failed', name: 'DriverTracker', error: e);
    }
  }

  Future<void> _stop() async {
    _timer?.cancel();
    _timer = null;
    final conn = _connection;
    _connection = null;
    if (conn != null) {
      try {
        await conn.stop();
      } catch (_) {}
    }
  }

  Future<void> dispose() => _stop();
}
