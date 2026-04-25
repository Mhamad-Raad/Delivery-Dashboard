import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/user_profile_model.dart';
import '../data/user_profile_repository.dart';

final userProfileProvider = AsyncNotifierProvider<UserProfileNotifier, UserProfile>(
  UserProfileNotifier.new,
);

class UserProfileNotifier extends AsyncNotifier<UserProfile> {
  @override
  Future<UserProfile> build() async {
    return _fetchProfile();
  }

  Future<UserProfile> _fetchProfile() async {
    final repository = ref.read(userProfileRepositoryProvider);
    return await repository.getMyProfile();
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchProfile());
  }

  Future<void> updateProfile(UpdateProfileRequest request) async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      final repository = ref.read(userProfileRepositoryProvider);
      return await repository.updateMyProfile(request);
    });
  }
}
