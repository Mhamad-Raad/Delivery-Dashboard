import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/vendor_model.dart';
import '../data/vendor_repository.dart';

final vendorsProvider = AsyncNotifierProvider<VendorsNotifier, List<Vendor>>(VendorsNotifier.new);

class VendorsNotifier extends AsyncNotifier<List<Vendor>> {
  String? _searchTerm;

  String? get searchTerm => _searchTerm;

  @override
  Future<List<Vendor>> build() async {
    return _fetchVendors();
  }

  Future<List<Vendor>> _fetchVendors() async {
    final repository = ref.read(vendorRepositoryProvider);
    return await repository.getVendors(searchName: _searchTerm);
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchVendors());
  }

  Future<void> setSearch(String? term) async {
    final normalized = (term == null || term.trim().isEmpty) ? null : term.trim();
    if (_searchTerm == normalized) return;
    _searchTerm = normalized;
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchVendors());
  }
}
