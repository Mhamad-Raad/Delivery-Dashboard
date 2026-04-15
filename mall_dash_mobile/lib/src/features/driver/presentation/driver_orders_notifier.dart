import 'dart:async';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/driver_order_model.dart';
import '../data/driver_orders_repository.dart';
import '../data/driver_dispatch_repository.dart';
import '../../../core/network/dio_provider.dart';
import '../../../core/storage/token_storage_service.dart';
import 'package:dio/dio.dart';
import 'package:signalr_core/signalr_core.dart';

final driverOrdersRepositoryProvider = Provider<DriverOrdersRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return DriverOrdersRepository(dio);
});

final driverDispatchRepositoryProvider = Provider<DriverDispatchRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return DriverDispatchRepository(dio);
});

// Available orders (status 4 or 5 - ready for pickup or assigned)
final availableOrdersNotifierProvider = AsyncNotifierProvider<AvailableOrdersNotifier, List<DriverOrder>>(
  AvailableOrdersNotifier.new,
);

// Active deliveries (status 6 or 7 - picked up or in transit)
final activeDeliveriesNotifierProvider = AsyncNotifierProvider<ActiveDeliveriesNotifier, List<DriverOrder>>(
  ActiveDeliveriesNotifier.new,
);

// Single order details
final orderDetailsProvider = FutureProvider.family<DriverOrder?, int>((ref, orderId) async {
  final repository = ref.watch(driverOrdersRepositoryProvider);
  return await repository.getOrderById(orderId);
});

final driverOrderSocketProvider = StreamProvider.family<DriverOrder?, int>((ref, orderId) async* {
  final dio = ref.watch(dioProvider);
  final tokenStorage = ref.watch(tokenStorageServiceProvider);

  final baseUrl = dio.options.baseUrl;
  final baseUri = Uri.parse(baseUrl);
  final hubUri = baseUri.replace(path: '${baseUri.path}/orderHub');
  final hubUrl = hubUri.toString();

  final controller = StreamController<DriverOrder?>();

  final connection = HubConnectionBuilder()
      .withUrl(hubUrl, HttpConnectionOptions(accessTokenFactory: () async => await tokenStorage.getAccessToken() ?? ''))
      .build();

  connection.on('OrderUpdated', (arguments) {
    try {
      if (arguments == null || arguments.isEmpty) {
        return;
      }

      final payload = arguments.first;
      if (payload is Map<String, dynamic>) {
        final order = DriverOrder.fromJson(payload);
        if (order.id == orderId) {
          controller.add(order);
        }
      }
    } catch (e) {
      controller.addError(e);
    }
  });

  try {
    await connection.start();
  } catch (e) {
    controller.addError(e);
  }

  ref.onDispose(() async {
    await connection.stop();
    await controller.close();
  });

  yield* controller.stream.handleError((error) {
    print('Order SignalR error for $orderId: $error');
  });
});

class AvailableOrdersNotifier extends AsyncNotifier<List<DriverOrder>> {
  final Map<int, Timer> _expirationTimers = {};

  @override
  Future<List<DriverOrder>> build() async {
    ref.onDispose(() {
      for (final timer in _expirationTimers.values) {
        timer.cancel();
      }
      _expirationTimers.clear();
    });

    return await _loadAvailableOrders();
  }

  Future<List<DriverOrder>> _loadAvailableOrders() async {
    // ℹ️ BACKEND INFO: Orders are automatically assigned to drivers via notifications
    // when they start their shift and are in the queue.
    //
    // The backend dispatch system:
    // 1. Driver starts shift → enters queue at position
    // 2. When order comes in → backend assigns to next driver in queue
    // 3. Driver receives NOTIFICATION about the assignment
    // 4. Driver can accept or reject via DriverDispatch endpoints
    //
    // This means we DON'T fetch orders - they come to us via notifications!
    // For now, return empty list. Orders will be added when notifications arrive.

    print('ℹ️ Driver orders are pushed via notifications, not fetched');
    print('Waiting for order assignments from backend...');

    // TODO: Integrate with notification system to receive assigned orders
    // When notification arrives with assignmentId, add order to this list
    return [];
  }

  Future<void> acceptOrder(int assignmentId, DriverOrder order) async {
    final dispatchRepository = ref.read(driverDispatchRepositoryProvider);

    try {
      final success = await dispatchRepository.acceptOrder(assignmentId);

      if (success) {
        // Remove from available orders
        removeOrder(order.id);

        // Add to active deliveries (client-side tracking until backend provides endpoint)
        ref.read(activeDeliveriesNotifierProvider.notifier).addActiveDelivery(order);
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> rejectOrder(int assignmentId) async {
    final dispatchRepository = ref.read(driverDispatchRepositoryProvider);

    try {
      final success = await dispatchRepository.rejectOrder(assignmentId);

      if (success) {
        await refresh();
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _loadAvailableOrders());
  }

  /// Add an order assignment received from notification
  /// Call this when the driver receives a notification about a new order assignment
  void addAssignment(DriverOrder order) {
    print('========================================');
    print('📥 ADDING ORDER TO AVAILABLE ORDERS');
    print('========================================');
    print('   Order ID: ${order.id}');
    print('   Order Number: ${order.orderNumber}');
    print('   Status: ${order.status} (${order.statusName})');
    print('   Total: ${order.totalAmount}');
    print('   Customer: ${order.customerName}');

    final currentOrders = state.value ?? <DriverOrder>[];
    print('   Current orders count: ${currentOrders.length}');

    final existingIndex = currentOrders.indexWhere((o) => o.id == order.id);
    List<DriverOrder> updatedOrders;

    if (existingIndex >= 0) {
      print('   ⚠️ Order already exists at index $existingIndex, updating...');
      updatedOrders = [...currentOrders];
      updatedOrders[existingIndex] = order;
    } else {
      print('   ➕ Adding new order to list');
      updatedOrders = [...currentOrders, order];
    }

    print('   Updated orders count: ${updatedOrders.length}');
    state = AsyncValue.data(updatedOrders);

    // Cancel any existing timer for this order
    _expirationTimers[order.id]?.cancel();

    // Start 30-second timer for this order
    print('   ⏱️ Starting 30-second timer for order ${order.id}');
    _expirationTimers[order.id] = Timer(const Duration(seconds: 30), () {
      print('========================================');
      print('⏰ TIMER EXPIRED FOR ORDER ${order.id}');
      print('   Order #${order.orderNumber} will be removed');
      print('========================================');
      removeOrder(order.id);
    });

    print('========================================');
    print('✅ ORDER ADDED SUCCESSFULLY');
    print('   Total available orders: ${updatedOrders.length}');
    for (var o in updatedOrders) {
      print('   - Order #${o.orderNumber} (ID: ${o.id})');
    }
    print('========================================');
  }

  /// Remove an order after it's been accepted or rejected
  void removeOrder(int orderId) {
    print('========================================');
    print('🗑️ REMOVING ORDER FROM AVAILABLE ORDERS');
    print('   Order ID: $orderId');
    print('========================================');

    final timer = _expirationTimers.remove(orderId);
    if (timer != null) {
      timer.cancel();
      print('   ⏱️ Timer cancelled for order $orderId');
    }

    final currentOrders = state.value ?? <DriverOrder>[];
    final orderToRemove = currentOrders.where((o) => o.id == orderId).firstOrNull;
    if (orderToRemove != null) {
      print('   Found order: #${orderToRemove.orderNumber}');
    }

    final updatedOrders = currentOrders.where((o) => o.id != orderId).toList();
    state = AsyncValue.data(updatedOrders);

    print('   Remaining orders: ${updatedOrders.length}');
    for (var o in updatedOrders) {
      print('   - Order #${o.orderNumber} (ID: ${o.id})');
    }
    print('========================================');
  }
}

class ActiveDeliveriesNotifier extends AsyncNotifier<List<DriverOrder>> {
  @override
  Future<List<DriverOrder>> build() async {
    return await _loadActiveDeliveries();
  }

  Future<List<DriverOrder>> _loadActiveDeliveries() async {
    // ℹ️ BACKEND INFO: Driver cannot access /Order endpoint with status filters
    // This returns 400 because drivers don't have permission to query all orders
    //
    // BACKEND NEEDS TO ADD:
    // GET /DriverDispatch/my-active-orders
    //   → Returns orders currently being delivered by this driver
    //   → Should include status 6 (picked up) and 7 (in transit)
    //
    // For now, we'll track accepted orders client-side using a local list.
    // When driver accepts an order via addAssignment(), we'll move it here.

    print('ℹ️ Active deliveries: Waiting for backend endpoint /DriverDispatch/my-active-orders');
    print('TODO: Backend needs to provide endpoint for driver\'s active orders');

    // TODO: Remove this attempt to use /Order endpoint once backend provides proper endpoint
    // For now, return empty list to avoid 400 errors
    return [];

    /* COMMENTED OUT UNTIL BACKEND ADDS ENDPOINT
    final repository = ref.read(driverOrdersRepositoryProvider);

    try {
      // This doesn't work - drivers can't access /Order endpoint
      final pickedUpOrders = await repository.getOrders(
        page: 1,
        limit: 50,
        status: '6', // Picked up
      );
      final inTransitOrders = await repository.getOrders(
        page: 1,
        limit: 50,
        status: '7', // In transit
      );

      final allOrders = [...pickedUpOrders, ...inTransitOrders];
      final uniqueOrders = <int, DriverOrder>{};

      for (final order in allOrders) {
        uniqueOrders[order.id] = order;
      }

      final sortedOrders = uniqueOrders.values.toList()
        ..sort((a, b) {
          final aTime = a.deliveryTime ?? a.createdAt ?? DateTime.now();
          final bTime = b.deliveryTime ?? b.createdAt ?? DateTime.now();
          return aTime.compareTo(bTime);
        });

      return sortedOrders;
    } catch (e) {
      print('⚠️ Error loading active deliveries: $e');
      return [];
    }
    */
  }

  Future<void> updateOrderStatus(int orderId, DeliveryStatus status) async {
    final repository = ref.read(driverOrdersRepositoryProvider);

    try {
      final success = await repository.updateOrderStatus(orderId, status.value);

      if (success) {
        await refresh();

        // If status is delivered, also refresh available orders
        if (status == DeliveryStatus.delivered) {
          ref.invalidate(availableOrdersNotifierProvider);
        }
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> completeDelivery(int orderId) async {
    final dispatchRepository = ref.read(driverDispatchRepositoryProvider);

    try {
      final success = await dispatchRepository.completeDelivery(orderId);

      if (success) {
        // Remove from active deliveries
        removeDelivery(orderId);

        // Refresh available orders in case new assignment came in
        ref.invalidate(availableOrdersNotifierProvider);
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _loadActiveDeliveries());
  }

  /// Add an accepted order to active deliveries
  /// Call this after driver accepts an assignment
  void addActiveDelivery(DriverOrder order) {
    state.whenData((deliveries) {
      final updatedDeliveries = [...deliveries, order];
      state = AsyncValue.data(updatedDeliveries);
    });
  }

  /// Remove a delivery after it's completed
  void removeDelivery(int orderId) {
    state.whenData((deliveries) {
      final updatedDeliveries = deliveries.where((o) => o.id != orderId).toList();
      state = AsyncValue.data(updatedDeliveries);
    });
  }
}
