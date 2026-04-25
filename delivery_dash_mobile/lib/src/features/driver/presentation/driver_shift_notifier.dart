import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/driver_shift_model.dart';
import '../data/driver_shift_repository.dart';
import '../../../core/network/dio_provider.dart';
import '../../notifications/data/driver_notification_service.dart';
import 'package:dio/dio.dart';

final driverShiftRepositoryProvider = Provider<DriverShiftRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return DriverShiftRepository(dio);
});

final driverShiftNotifierProvider =
    AsyncNotifierProvider<DriverShiftNotifier, DriverShift>(
      DriverShiftNotifier.new,
    );

final queuePositionProvider = FutureProvider<QueuePosition?>((ref) async {
  final repository = ref.watch(driverShiftRepositoryProvider);
  return await repository.getQueuePosition();
});

class DriverShiftNotifier extends AsyncNotifier<DriverShift> {
  @override
  Future<DriverShift> build() async {
    // Load initial shift status
    return await _loadCurrentShift();
  }

  Future<DriverShift> _loadCurrentShift() async {
    final repository = ref.read(driverShiftRepositoryProvider);
    final shift = await repository.getCurrentShift();
    return shift ?? DriverShift(isActive: false);
  }

  Future<void> startShift() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      final repository = ref.read(driverShiftRepositoryProvider);
      final shift = await repository.startShift();

      // Refresh queue position after starting shift
      ref.invalidate(queuePositionProvider);

      // Start notification polling (checks every 5 seconds for new orders)
      final notificationService = ref.read(driverNotificationServiceProvider);
      notificationService.startPolling();

      return shift ?? DriverShift(isActive: true, startTime: DateTime.now());
    });
  }

  Future<void> endShift() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      final repository = ref.read(driverShiftRepositoryProvider);
      final success = await repository.endShift();

      // Stop notification polling when shift ends
      final notificationService = ref.read(driverNotificationServiceProvider);
      notificationService.stopPolling();

      if (success) {
        return DriverShift(isActive: false);
      } else {
        throw Exception('Failed to end shift');
      }
    });
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _loadCurrentShift());

    // Also refresh queue position
    ref.invalidate(queuePositionProvider);
  }
}
