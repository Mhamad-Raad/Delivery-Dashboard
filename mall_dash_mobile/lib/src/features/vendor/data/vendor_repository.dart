import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';
import 'vendor_model.dart';

final vendorRepositoryProvider = Provider<VendorRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return VendorRepository(dio: dio);
});

class VendorRepository {
  final Dio _dio;

  VendorRepository({required Dio dio}) : _dio = dio;

  Future<List<Vendor>> getVendors({int page = 1, int limit = 100}) async {
    try {
      final response = await _dio.get(
        '/Vendor/tenant',
        queryParameters: {
          'page': page,
          'limit': limit,
        },
      );

      if (response.statusCode == 200) {
        // Handle both paginated response and direct array
        final dynamic data = response.data;
        
        if (data is Map<String, dynamic>) {
          // Paginated response with items/data array
          final items = data['items'] ?? data['data'] ?? [];
          return (items as List).map((json) => Vendor.fromJson(json as Map<String, dynamic>)).toList();
        } else if (data is List) {
          // Direct array response
          return data.map((json) => Vendor.fromJson(json as Map<String, dynamic>)).toList();
        } else {
          return [];
        }
      } else {
        throw Exception('Failed to load vendors: ${response.statusCode}');
      }
    } on DioException catch (e) {
      // Better error messages for common HTTP errors
      if (e.response?.statusCode == 403) {
        throw Exception('Access denied. Your account may not have permission to view vendors. Please contact support.');
      } else if (e.response?.statusCode == 401) {
        throw Exception('Authentication required. Please log in again.');
      }
      throw Exception('Failed to load vendors: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      rethrow;
    }
  }

  Future<Vendor> getVendorById(int id) async {
    try {
      final response = await _dio.get('/Vendor/$id');

      if (response.statusCode == 200) {
        return Vendor.fromJson(response.data as Map<String, dynamic>);
      } else {
        throw Exception('Failed to load vendor: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw Exception('Failed to load vendor: ${e.message}');
    } catch (e) {
      rethrow;
    }
  }
}
