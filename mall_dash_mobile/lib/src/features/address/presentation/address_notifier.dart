import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/address_model.dart';
import '../data/address_repository.dart';

/// Async list of the customer's addresses. The notifier owns CRUD mutations and
/// keeps the list in sync with the backend without needing a full refetch for
/// every action.
final addressesProvider = AsyncNotifierProvider<AddressesNotifier, List<Address>>(
  AddressesNotifier.new,
);

/// Currently-selected delivery address id for the active cart/checkout.
/// `null` until the user picks one (or the default is auto-selected).
final selectedAddressIdProvider =
    NotifierProvider<SelectedAddressIdNotifier, int?>(
  SelectedAddressIdNotifier.new,
);

class SelectedAddressIdNotifier extends Notifier<int?> {
  @override
  int? build() => null;

  void select(int? id) => state = id;
}

/// The currently-selected address resolved from [addressesProvider]. Returns null
/// until the list has loaded AND a selection exists.
final selectedAddressProvider = Provider<Address?>((ref) {
  final list = ref.watch(addressesProvider).asData?.value ?? const <Address>[];
  final selectedId = ref.watch(selectedAddressIdProvider);
  if (selectedId == null) return null;
  for (final a in list) {
    if (a.id == selectedId) return a;
  }
  return null;
});

class AddressesNotifier extends AsyncNotifier<List<Address>> {
  @override
  Future<List<Address>> build() async {
    final repo = ref.read(addressRepositoryProvider);
    final list = await repo.getMyAddresses();
    // Auto-select the default if the caller hasn't picked one yet.
    final currentSelected = ref.read(selectedAddressIdProvider);
    if (currentSelected == null) {
      final defaultOrFirst = _pickDefault(list);
      if (defaultOrFirst != null) {
        // Defer the write to avoid touching another provider during build().
        Future.microtask(() => ref
            .read(selectedAddressIdProvider.notifier)
            .select(defaultOrFirst.id));
      }
    }
    return list;
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      return ref.read(addressRepositoryProvider).getMyAddresses();
    });
  }

  Future<Address> create(AddressRequest request) async {
    final repo = ref.read(addressRepositoryProvider);
    final created = await repo.create(request);
    final previous = state.asData?.value ?? const <Address>[];
    // A brand-new "default" address bumps other entries off default on the server.
    final next = [
      for (final a in previous)
        if (created.isDefault) _unsetDefault(a) else a,
      created,
    ];
    state = AsyncValue.data(next);
    final selector = ref.read(selectedAddressIdProvider.notifier);
    if (created.isDefault || previous.isEmpty) {
      selector.select(created.id);
    }
    return created;
  }

  Future<Address> updateAddress(int id, AddressRequest request) async {
    final repo = ref.read(addressRepositoryProvider);
    final updated = await repo.update(id, request);
    final previous = state.asData?.value ?? const <Address>[];
    final next = [
      for (final a in previous)
        if (a.id == updated.id)
          updated
        else if (updated.isDefault)
          _unsetDefault(a)
        else
          a,
    ];
    state = AsyncValue.data(next);
    return updated;
  }

  Future<void> delete(int id) async {
    final repo = ref.read(addressRepositoryProvider);
    await repo.delete(id);
    final previous = state.asData?.value ?? const <Address>[];
    final next = previous.where((a) => a.id != id).toList();
    state = AsyncValue.data(next);
    final selected = ref.read(selectedAddressIdProvider);
    if (selected == id) {
      ref
          .read(selectedAddressIdProvider.notifier)
          .select(_pickDefault(next)?.id);
    }
  }

  Future<void> setDefault(int id) async {
    final repo = ref.read(addressRepositoryProvider);
    await repo.setDefault(id);
    final previous = state.asData?.value ?? const <Address>[];
    final next = [
      for (final a in previous) _withDefault(a, a.id == id),
    ];
    state = AsyncValue.data(next);
  }

  static Address? _pickDefault(List<Address> list) {
    for (final a in list) {
      if (a.isDefault) return a;
    }
    return list.isEmpty ? null : list.first;
  }

  static Address _unsetDefault(Address a) => _withDefault(a, false);

  static Address _withDefault(Address a, bool isDefault) => Address(
        id: a.id,
        userId: a.userId,
        type: a.type,
        latitude: a.latitude,
        longitude: a.longitude,
        phoneNumber: a.phoneNumber,
        street: a.street,
        buildingName: a.buildingName,
        floor: a.floor,
        apartmentNumber: a.apartmentNumber,
        houseName: a.houseName,
        houseNumber: a.houseNumber,
        companyName: a.companyName,
        additionalDirections: a.additionalDirections,
        label: a.label,
        isDefault: isDefault,
        createdAt: a.createdAt,
        lastModifiedAt: a.lastModifiedAt,
      );
}
