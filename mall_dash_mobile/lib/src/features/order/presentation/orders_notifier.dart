import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/order_model.dart';
import '../data/order_repository.dart';

final ordersProvider = FutureProvider<List<Order>>((ref) async {
  final repository = ref.watch(orderRepositoryProvider);
  return await repository.getOrders();
});

class OrdersNotifier extends AsyncNotifier<List<Order>> {
  @override
  Future<List<Order>> build() async {
    final repository = ref.watch(orderRepositoryProvider);
    return await repository.getOrders();
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      final repository = ref.read(orderRepositoryProvider);
      return await repository.getOrders();
    });
  }

  Future<void> cancelOrder(int orderId) async {
    try {
      final repository = ref.read(orderRepositoryProvider);
      await repository.cancelOrder(orderId);
      
      // Refresh the orders list
      await refresh();
    } catch (e) {
      rethrow;
    }
  }
}

final ordersNotifierProvider = AsyncNotifierProvider<OrdersNotifier, List<Order>>(() {
  return OrdersNotifier();
});
