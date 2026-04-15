import 'dart:async';
import 'dart:developer' as developer;
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/auth_repository.dart';

final loginControllerProvider = AsyncNotifierProvider<LoginController, void>(LoginController.new);

class LoginController extends AsyncNotifier<void> {
  @override
  FutureOr<void> build() {
    // Initial state is null (void)
    return null;
  }

  Future<void> login({required String email, required String password, String applicationContext = 'mobile'}) async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      final repository = ref.read(authRepositoryProvider);
      final response = await repository.login(email: email, password: password, applicationContext: applicationContext);
      // Using dart:developer log to print the full response without truncation
      developer.log('Login Response', name: 'LoginController', error: response);
      developer.log('Login Successful. Response Data: $response');
    });
  }
}
