import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/vendor_model.dart';
import '../data/vendor_repository.dart';

final vendorsProvider = AsyncNotifierProvider<VendorsNotifier, List<Vendor>>(VendorsNotifier.new);

class VendorsNotifier extends AsyncNotifier<List<Vendor>> {
  @override
  Future<List<Vendor>> build() async {
    return _fetchVendors();
  }

  Future<List<Vendor>> _fetchVendors() async {
    final repository = ref.read(vendorRepositoryProvider);
    return await repository.getVendors();
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchVendors());
  }
}
