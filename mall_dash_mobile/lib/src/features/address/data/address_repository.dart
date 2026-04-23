import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_provider.dart';
import 'address_model.dart';

final addressRepositoryProvider = Provider<AddressRepository>((ref) {
  return AddressRepository(dio: ref.watch(dioProvider));
});

/// Wraps the backend `/Address/*` endpoints.
///
/// All endpoints are scoped to the authenticated user — backend uses the JWT subject
/// as the owner, so no explicit `userId` is passed.
class AddressRepository {
  final Dio _dio;

  AddressRepository({required Dio dio}) : _dio = dio;

  /// `GET /Address/me` — list the current user's addresses.
  Future<List<Address>> getMyAddresses() async {
    try {
      final response = await _dio.get('/Address/me');
      final data = response.data;
      if (data is List) {
        return data
            .whereType<Map<String, dynamic>>()
            .map(Address.fromJson)
            .toList();
      }
      if (data is Map<String, dynamic>) {
        final list = data['items'] ?? data['data'] ?? data['addresses'];
        if (list is List) {
          return list
              .whereType<Map<String, dynamic>>()
              .map(Address.fromJson)
              .toList();
        }
      }
      return const [];
    } on DioException catch (e) {
      throw Exception(_errorMessage(e, fallback: 'Failed to load addresses'));
    }
  }

  /// `GET /Address/{id}`.
  Future<Address> getById(int id) async {
    try {
      final response = await _dio.get('/Address/$id');
      return Address.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw Exception(_errorMessage(e, fallback: 'Failed to load address'));
    }
  }

  /// `POST /Address`.
  Future<Address> create(AddressRequest request) async {
    try {
      final response = await _dio.post('/Address', data: request.toJson());
      return Address.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw Exception(_errorMessage(e, fallback: 'Failed to create address'));
    }
  }

  /// `PUT /Address/{id}`.
  Future<Address> update(int id, AddressRequest request) async {
    try {
      final response = await _dio.put('/Address/$id', data: request.toJson());
      return Address.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw Exception(_errorMessage(e, fallback: 'Failed to update address'));
    }
  }

  /// `DELETE /Address/{id}`.
  Future<void> delete(int id) async {
    try {
      await _dio.delete('/Address/$id');
    } on DioException catch (e) {
      throw Exception(_errorMessage(e, fallback: 'Failed to delete address'));
    }
  }

  /// `PATCH /Address/{id}/default` — promote to the default address.
  Future<void> setDefault(int id) async {
    try {
      await _dio.patch('/Address/$id/default');
    } on DioException catch (e) {
      throw Exception(_errorMessage(e, fallback: 'Failed to set default address'));
    }
  }

  String _errorMessage(DioException e, {required String fallback}) {
    final status = e.response?.statusCode;
    final body = e.response?.data;
    if (body is Map<String, dynamic>) {
      final msg = body['message'] ?? body['Message'] ?? body['error'] ?? body['title'];
      if (msg is String && msg.isNotEmpty) return msg;
    }
    if (status == 401) return 'You must be signed in to manage addresses.';
    if (status == 403) return 'You are not allowed to do that.';
    if (status == 404) return 'Address not found.';
    return fallback;
  }
}
