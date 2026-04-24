import 'dart:async';
import 'dart:developer' as developer;
import 'dart:io' show Platform;

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../firebase_options.dart';
import 'customer_notification_repository.dart';

/// Handles Firebase Cloud Messaging for the customer app only.
/// Call [FcmService.start] after a Customer successfully logs in.
/// Call [FcmService.stop] on logout or when a non-Customer signs in.
final fcmServiceProvider = Provider<FcmService>((ref) {
  return FcmService(ref);
});

class FcmService {
  FcmService(this._ref);

  final Ref _ref;
  static bool _firebaseInitialized = false;

  final FlutterLocalNotificationsPlugin _local = FlutterLocalNotificationsPlugin();
  static const _androidChannel = AndroidNotificationChannel(
    'delivery_dash_default',
    'Delivery Dash notifications',
    description: 'Order updates and announcements from Delivery Dash.',
    importance: Importance.high,
  );

  String? _lastRegisteredToken;
  StreamSubscription<String>? _tokenRefreshSub;
  StreamSubscription<RemoteMessage>? _onMessageSub;
  StreamSubscription<RemoteMessage>? _onOpenedSub;

  bool _started = false;

  /// Idempotent — safe to call multiple times; subsequent calls are no-ops.
  Future<void> start() async {
    if (_started) return;
    _started = true;

    try {
      await _ensureFirebase();
      await _setupLocalNotifications();
      await _requestPermission();
      await _registerToken();
      _listenTokenRefresh();
      _listenForegroundMessages();
      _listenNotificationTaps();
    } catch (e, st) {
      developer.log('FCM start failed',
          name: 'FcmService', error: e, stackTrace: st);
    }
  }

  Future<void> stop() async {
    _tokenRefreshSub?.cancel();
    _tokenRefreshSub = null;
    _onMessageSub?.cancel();
    _onMessageSub = null;
    _onOpenedSub?.cancel();
    _onOpenedSub = null;

    // Deregister token from backend so this device stops receiving pushes.
    final token = _lastRegisteredToken;
    if (token != null) {
      try {
        final repo = _ref.read(customerNotificationRepositoryProvider);
        await repo.deregisterDevice(token);
      } catch (_) {}
    }
    try {
      await FirebaseMessaging.instance.deleteToken();
    } catch (_) {}

    _lastRegisteredToken = null;
    _started = false;
  }

  static Future<void> ensureFirebaseInitialized() async {
    if (_firebaseInitialized) return;
    try {
      await Firebase.initializeApp(
        options: DefaultFirebaseOptions.currentPlatform,
      );
      _firebaseInitialized = true;
    } catch (e) {
      // App may already be initialized (e.g. by background handler isolate).
      if (e.toString().contains('duplicate-app')) {
        _firebaseInitialized = true;
      } else {
        developer.log('Firebase init failed', name: 'FcmService', error: e);
        rethrow;
      }
    }
  }

  Future<void> _ensureFirebase() => ensureFirebaseInitialized();

  Future<void> _setupLocalNotifications() async {
    const androidInit =
        AndroidInitializationSettings('@mipmap/ic_launcher');
    const iosInit = DarwinInitializationSettings();
    await _local.initialize(
      const InitializationSettings(android: androidInit, iOS: iosInit),
    );

    if (!kIsWeb && Platform.isAndroid) {
      await _local
          .resolvePlatformSpecificImplementation<
              AndroidFlutterLocalNotificationsPlugin>()
          ?.createNotificationChannel(_androidChannel);
    }
  }

  Future<void> _requestPermission() async {
    final settings = await FirebaseMessaging.instance.requestPermission(
      alert: true,
      badge: true,
      sound: true,
    );
    developer.log('FCM permission: ${settings.authorizationStatus}',
        name: 'FcmService');
  }

  Future<void> _registerToken() async {
    final token = await FirebaseMessaging.instance.getToken();
    if (token == null || token.isEmpty) return;
    final platform = kIsWeb
        ? 2
        : Platform.isIOS
            ? 1
            : 0;
    final repo = _ref.read(customerNotificationRepositoryProvider);
    await repo.registerDevice(token: token, platform: platform);
    _lastRegisteredToken = token;
    developer.log('FCM token registered: ${token.substring(0, 12)}…',
        name: 'FcmService');
  }

  void _listenTokenRefresh() {
    _tokenRefreshSub =
        FirebaseMessaging.instance.onTokenRefresh.listen((newToken) async {
      final repo = _ref.read(customerNotificationRepositoryProvider);
      await repo.registerDevice(
        token: newToken,
        platform: kIsWeb
            ? 2
            : Platform.isIOS
                ? 1
                : 0,
      );
      _lastRegisteredToken = newToken;
    });
  }

  void _listenForegroundMessages() {
    _onMessageSub = FirebaseMessaging.onMessage.listen((message) async {
      // Foreground: Android won't show the push automatically, so we render
      // it via flutter_local_notifications.
      final notif = message.notification;
      final title = notif?.title ?? message.data['title'] ?? '';
      final body = notif?.body ?? message.data['body'] ?? '';
      if (title.isEmpty && body.isEmpty) return;

      await _local.show(
        message.hashCode,
        title,
        body,
        NotificationDetails(
          android: AndroidNotificationDetails(
            _androidChannel.id,
            _androidChannel.name,
            channelDescription: _androidChannel.description,
            importance: Importance.high,
            priority: Priority.high,
            icon: '@mipmap/ic_launcher',
          ),
          iOS: const DarwinNotificationDetails(
            presentAlert: true,
            presentBadge: true,
            presentSound: true,
          ),
        ),
        payload: message.data.isEmpty
            ? null
            : message.data.entries.map((e) => '${e.key}=${e.value}').join('&'),
      );
    });
  }

  void _listenNotificationTaps() {
    _onOpenedSub = FirebaseMessaging.onMessageOpenedApp.listen((message) {
      developer.log('Notification opened: ${message.data}', name: 'FcmService');
      // Deep-link handling can be added here (e.g. navigate to order details
      // when data contains an orderId). Kept out of scope per "display only".
    });
  }
}

/// Top-level entry for FCM to invoke while the app is in the background or
/// terminated. Must be a top-level function annotated with
/// `@pragma('vm:entry-point')` so the Flutter engine can resume it.
/// The OS displays the system notification itself from `notification:` payload
/// fields — we don't need to do anything unless we want to pre-process data.
@pragma('vm:entry-point')
Future<void> firebaseBackgroundMessageHandler(RemoteMessage message) async {
  await FcmService.ensureFirebaseInitialized();
  developer.log('BG message: ${message.messageId}',
      name: 'FcmService.bg');
}
