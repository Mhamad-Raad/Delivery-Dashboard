import 'dart:async';
import 'dart:convert';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:signalr_core/signalr_core.dart';
import '../data/notification_repository.dart';
import '../data/notification_model.dart';
import '../../driver/data/driver_order_model.dart';
import '../../driver/presentation/driver_orders_notifier.dart';
import '../../../core/network/dio_provider.dart';
import '../../../core/storage/token_storage_service.dart';

final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return NotificationRepository(dio);
});

final driverNotificationServiceProvider = Provider<DriverNotificationService>((ref) {
  return DriverNotificationService(ref);
});

/// Service that polls for notifications every 5 seconds when driver is on shift
class DriverNotificationService {
  final Ref _ref;
  Timer? _pollingTimer;
  int _lastCheckedCount = 0;
  HubConnection? _hubConnection;

  DriverNotificationService(this._ref);

  /// Start listening for notifications via SignalR (polling disabled to reduce server load)
  void startPolling() {
    print('========================================');
    print('🚀 DRIVER NOTIFICATION SERVICE STARTING');
    print('========================================');

    // DISABLED: Polling was causing too many requests to the server
    // With 189 unread notifications, each poll cycle was making 380+ requests
    // Instead, we rely solely on SignalR for real-time notifications

    // Cancel any existing polling timer
    _pollingTimer?.cancel();
    _pollingTimer = null;

    print('🔗 Starting SignalR connection (polling disabled)...');
    _startSignalRConnection();

    print('========================================');
    print('✅ NOTIFICATION SERVICE STARTED');
    print('   - Polling: DISABLED (to reduce server load)');
    print('   - SignalR: Connecting to /Notification hub');
    print('   - Listening for real-time order notifications');
    print('========================================');
  }

  /// Stop polling for notifications
  void stopPolling() {
    if (_pollingTimer != null) {
      print('🔕 Stopping notification polling');
      _pollingTimer?.cancel();
      _pollingTimer = null;
      _lastCheckedCount = 0;
    }

    _stopSignalRConnection();
  }

  /// Check if there are new notifications
  Future<void> _checkForNewNotifications() async {
    try {
      final repository = _ref.read(notificationRepositoryProvider);

      // First, check unread count (lightweight request)
      final unreadCount = await repository.getUnreadCount();

      print('📬 Unread notifications: $unreadCount');

      // Only fetch full notifications if there are new ones
      if (unreadCount > 0 && unreadCount != _lastCheckedCount) {
        _lastCheckedCount = unreadCount;

        // Fetch the latest unread notifications
        final notifications = await repository.getNotifications(skip: 0, take: unreadCount);

        print('📨 Fetched ${notifications.length} notifications');

        // Handle each notification
        for (final notification in notifications) {
          if (!notification.isRead) {
            await _handleNotification(notification);
          }
        }
      }
    } catch (e) {
      print('❌ Error checking notifications: $e');
    }
  }

  Future<void> _startSignalRConnection() async {
    if (_hubConnection != null &&
        (_hubConnection!.state == HubConnectionState.connecting ||
            _hubConnection!.state == HubConnectionState.connected)) {
      print('========================================');
      print('ℹ️ Notification SignalR connection already active');
      print('   Current state: ${_hubConnection!.state}');
      print('========================================');
      return;
    }

    try {
      final dio = _ref.read(dioProvider);
      final tokenStorage = _ref.read(tokenStorageServiceProvider);

      final baseUrl = dio.options.baseUrl;
      final baseUri = Uri.parse(baseUrl);
      // Backend hubs mount at the root (`/hubs/notifications`), not under the API path prefix.
      final hubUri = baseUri.replace(path: '/hubs/notifications');
      final hubUrl = hubUri.toString();

      print('========================================');
      print('🔗 SIGNALR CONNECTION SETUP');
      print('   Base URL: $baseUrl');
      print('   Hub URL: $hubUrl');
      print('========================================');

      final connection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(accessTokenFactory: () async {
              final token = await tokenStorage.getAccessToken() ?? '';
              print('📝 SignalR using token: ${token.isNotEmpty ? "${token.substring(0, 20)}..." : "NO TOKEN"}');
              return token;
            }),
          )
          .build();

      // Listen for ReceiveNotification event
      connection.on('ReceiveNotification', (arguments) {
        print('========================================');
        print('📨 SIGNALR EVENT: ReceiveNotification');
        print('   Raw arguments: $arguments');
        print('   Arguments type: ${arguments.runtimeType}');
        print('========================================');
        _handleSignalRNotification(arguments);
      });

      // Also listen for any other common event names that the backend might use
      connection.on('NewOrder', (arguments) {
        print('========================================');
        print('📨 SIGNALR EVENT: NewOrder');
        print('   Raw arguments: $arguments');
        print('========================================');
        _handleSignalRNotification(arguments);
      });

      connection.on('OrderNotification', (arguments) {
        print('========================================');
        print('📨 SIGNALR EVENT: OrderNotification');
        print('   Raw arguments: $arguments');
        print('========================================');
        _handleSignalRNotification(arguments);
      });

      print('🔗 Connecting to notification SignalR hub: $hubUrl');
      await connection.start();
      print('========================================');
      print('✅ SIGNALR CONNECTED SUCCESSFULLY');
      print('   Hub URL: $hubUrl');
      print('   Connection state: ${connection.state}');
      print('   Waiting for notifications...');
      print('========================================');

      _hubConnection = connection;
    } catch (e) {
      print('========================================');
      print('❌ ERROR: Failed to connect to SignalR hub');
      print('   Error: $e');
      print('========================================');
    }
  }

  void _stopSignalRConnection() {
    final connection = _hubConnection;
    if (connection != null) {
      try {
        print('🔌 Stopping notification SignalR connection');
        connection.stop();
      } catch (e) {
        print('❌ Error stopping notification SignalR connection: $e');
      } finally {
        _hubConnection = null;
      }
    }
  }

  Future<void> _handleSignalRNotification(List<Object?>? arguments) async {
    print('========================================');
    print('🔔 PROCESSING SIGNALR NOTIFICATION');
    print('========================================');

    try {
      if (arguments == null || arguments.isEmpty) {
        print('⚠️ Arguments are null or empty');
        print('   arguments: $arguments');
        return;
      }

      print('📦 Arguments received:');
      print('   Count: ${arguments.length}');
      for (var i = 0; i < arguments.length; i++) {
        print('   [$i] Type: ${arguments[i].runtimeType}');
        print('   [$i] Value: ${arguments[i]}');
      }

      final payload = arguments.first;
      Map<String, dynamic>? data;

      print('📋 Parsing payload...');
      print('   Payload type: ${payload.runtimeType}');
      print('   Payload value: $payload');

      if (payload is Map<String, dynamic>) {
        data = payload;
        print('   ✓ Payload is already Map<String, dynamic>');
      } else if (payload is String) {
        print('   Payload is String, attempting JSON decode...');
        final decoded = jsonDecode(payload);
        print('   Decoded type: ${decoded.runtimeType}');
        print('   Decoded value: $decoded');
        if (decoded is Map<String, dynamic>) {
          data = decoded;
          print('   ✓ Successfully decoded as Map<String, dynamic>');
        }
      }

      if (data == null) {
        print('❌ Could not parse payload as Map<String, dynamic>');
        return;
      }

      print('📊 Parsed notification data:');
      data.forEach((key, value) {
        print('   $key: $value (${value.runtimeType})');
      });

      // Extract orderId from notification
      // Expected format: { "orderId": 31, "orderNumber": "ORD-20260117-0002", "totalAmount": 17 }
      final orderIdValue = data['orderId'] ?? data['OrderId'] ?? data['order_id'];
      final orderNumberValue = data['orderNumber'] ?? data['OrderNumber'] ?? data['order_number'];
      final totalAmountValue = data['totalAmount'] ?? data['TotalAmount'] ?? data['total_amount'];

      print('📝 Extracted values:');
      print('   orderId: $orderIdValue');
      print('   orderNumber: $orderNumberValue');
      print('   totalAmount: $totalAmountValue');

      final orderId = orderIdValue is int ? orderIdValue : int.tryParse(orderIdValue?.toString() ?? '');

      if (orderId == null) {
        print('❌ Could not extract valid orderId from notification');
        return;
      }

      print('========================================');
      print('🔍 FETCHING ORDER DETAILS');
      print('   Order ID: $orderId');
      print('========================================');

      final orderRepository = _ref.read(driverOrdersRepositoryProvider);
      final order = await orderRepository.getOrderById(orderId);

      if (order == null) {
        print('❌ Could not fetch order $orderId from API');
        return;
      }

      print('========================================');
      print('✅ ORDER FETCHED SUCCESSFULLY');
      print('   Order ID: ${order.id}');
      print('   Order Number: ${order.orderNumber}');
      print('   Status: ${order.status} (${order.statusName})');
      print('   Customer: ${order.customerName}');
      print('   Address: ${order.deliveryAddress}');
      print('   Total: ${order.totalAmount}');
      print('   Items: ${order.items?.length ?? 0}');
      print('========================================');

      // Add order to available orders list regardless of status
      // The driver can decide to accept or reject
      print('➕ Adding order to available orders list...');
      _ref.read(availableOrdersNotifierProvider.notifier).addAssignment(order);

      print('========================================');
      print('🎉 ORDER ADDED TO AVAILABLE ORDERS');
      print('   Order #${order.orderNumber} is now visible to driver');
      print('   Driver has 30 seconds to accept or reject');
      print('========================================');
    } catch (e, stackTrace) {
      print('========================================');
      print('❌ ERROR HANDLING SIGNALR NOTIFICATION');
      print('   Error: $e');
      print('   Stack trace: $stackTrace');
      print('========================================');
    }
  }

  /// Handle a notification based on its type
  Future<void> _handleNotification(AppNotification notification) async {
    print('🔔 Handling notification: ${notification.title}');
    print('   Type: ${notification.type}');
    print('   Message: ${notification.message}');
    print('   Data: ${notification.data}');

    final type = notification.type?.toLowerCase() ?? '';

    if (type.contains('driveroffer')) {
      await _handleDriverOffer(notification);
    } else if (type.contains('order') && type.contains('assign')) {
      await _handleOrderAssignment(notification);
    } else if (type.contains('order') && type.contains('cancel')) {
      print('⚠️ Order cancelled');
    }

    // Mark notification as read
    final repository = _ref.read(notificationRepositoryProvider);
    await repository.markAsRead(notification.id);
  }

  /// Handle order assignment notification
  Future<void> _handleOrderAssignment(AppNotification notification) async {
    try {
      final data = notification.data;

      if (data == null) {
        print('⚠️ Order assignment notification missing data');
        return;
      }

      final assignmentIdValue = data['assignmentId'] ?? data['AssignmentId'];
      final orderIdValue = data['orderId'] ?? data['OrderId'];
      final assignmentId = assignmentIdValue is int
          ? assignmentIdValue
          : int.tryParse(assignmentIdValue?.toString() ?? '');
      final orderId = orderIdValue is int ? orderIdValue : int.tryParse(orderIdValue?.toString() ?? '');

      print('📦 Order assignment received:');
      print('   Assignment ID: $assignmentId');
      print('   Order ID: $orderId');

      if (orderId == null) {
        print('⚠️ Missing orderId in notification data');
        return;
      }

      // Fetch the full order details
      final orderRepository = _ref.read(driverOrdersRepositoryProvider);
      final order = await orderRepository.getOrderById(orderId);

      if (order == null) {
        print('⚠️ Could not fetch order $orderId');
        return;
      }

      // Use copyWith to add assignmentId if needed
      final orderWithAssignment = order.copyWith(
        assignmentId: assignmentId ?? order.assignmentId,
        isAssigned: true,
      );

      print('✅ Adding order to available orders list');
      _ref.read(availableOrdersNotifierProvider.notifier).addAssignment(orderWithAssignment);

      print('🎉 Order assignment handled successfully!');
    } catch (e) {
      print('❌ Error handling order assignment: $e');
    }
  }

  Future<void> _handleDriverOffer(AppNotification notification) async {
    try {
      print('========================================');
      print('🚗 PROCESSING DRIVER OFFER');
      print('   Title: ${notification.title}');
      print('   Message: ${notification.message}');
      print('   Data: ${notification.data}');
      print('========================================');

      final data = notification.data;
      final orderRepository = _ref.read(driverOrdersRepositoryProvider);
      DriverOrder? order;

      // Try to extract order info from data first
      int? orderId;
      int? assignmentId;
      String? orderNumberFromData;

      if (data != null) {
        final assignmentIdValue = data['assignmentId'] ?? data['AssignmentId'];
        final orderIdValue = data['orderId'] ?? data['OrderId'];
        assignmentId = assignmentIdValue is int
            ? assignmentIdValue
            : int.tryParse(assignmentIdValue?.toString() ?? '');
        orderId = orderIdValue is int ? orderIdValue : int.tryParse(orderIdValue?.toString() ?? '');
        orderNumberFromData = (data['orderNumber'] ?? data['OrderNumber'])?.toString();

        print('📦 Data payload found:');
        print('   Assignment ID: $assignmentId');
        print('   Order ID: $orderId');
        print('   Order Number: $orderNumberFromData');
      }

      // If we have orderId from data, fetch by ID
      if (orderId != null) {
        print('🔍 Fetching order by ID: $orderId');
        order = await orderRepository.getOrderById(orderId);
      }

      // If no orderId or fetch failed, try to extract order number from message
      if (order == null && notification.message != null) {
        print('📝 Trying to extract order number from message...');
        print('   Message: ${notification.message}');

        // Extract order number from message like "Order #ORD-20260117-0005 is available..."
        final orderNumberRegex = RegExp(r'#(ORD-\d{8}-\d{4})');
        final match = orderNumberRegex.firstMatch(notification.message!);

        if (match != null) {
          final extractedOrderNumber = match.group(1)!;
          print('   ✓ Extracted order number: $extractedOrderNumber');

          print('🔍 Fetching order by number: $extractedOrderNumber');
          order = await orderRepository.getOrderByNumber(extractedOrderNumber);
        } else {
          print('   ⚠️ Could not extract order number from message');
        }
      }

      if (order == null) {
        print('❌ Could not fetch order - no valid orderId or orderNumber found');
        return;
      }

      print('========================================');
      print('✅ ORDER FETCHED FOR DRIVER OFFER');
      print('   Order ID: ${order.id}');
      print('   Order Number: ${order.orderNumber}');
      print('   Status: ${order.status} (${order.statusName})');
      print('   Customer: ${order.customerName}');
      print('   Total: ${order.totalAmount}');
      print('========================================');

      // Create the order with assignmentId if we have it (use copyWith for simplicity)
      final orderWithOffer = order.copyWith(
        assignmentId: assignmentId ?? order.assignmentId,
        isAssigned: false,
      );

      print('➕ Adding driver offer to available orders list...');
      _ref.read(availableOrdersNotifierProvider.notifier).addAssignment(orderWithOffer);

      print('========================================');
      print('🎉 DRIVER OFFER HANDLED SUCCESSFULLY');
      print('   Order #${order.orderNumber} added to available orders');
      print('   Driver has 30 seconds to accept or reject');
      print('========================================');
    } catch (e, stackTrace) {
      print('========================================');
      print('❌ ERROR HANDLING DRIVER OFFER');
      print('   Error: $e');
      print('   Stack: $stackTrace');
      print('========================================');
    }
  }

  /// Dispose and clean up
  void dispose() {
    stopPolling();
  }
}
