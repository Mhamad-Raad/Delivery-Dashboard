import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';
import 'category_model.dart';

final categoryRepositoryProvider = Provider<CategoryRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return CategoryRepository(dio: dio);
});

class CategoryRepository {
  final Dio _dio;

  CategoryRepository({required Dio dio}) : _dio = dio;

  Future<List<Category>> getByVendor(int vendorId) async {
    try {
      final response = await _dio.get('/Category/by-vendor/$vendorId/public');

      if (response.statusCode == 200) {
        final dynamic data = response.data;
        if (data is List) {
          return data
              .map((json) => Category.fromJson(json as Map<String, dynamic>))
              .toList();
        }
        if (data is Map<String, dynamic>) {
          final items = data['items'] ?? data['data'] ?? [];
          return (items as List)
              .map((json) => Category.fromJson(json as Map<String, dynamic>))
              .toList();
        }
        return [];
      }
      throw Exception('Failed to load categories: ${response.statusCode}');
    } on DioException catch (e) {
      throw Exception(
          'Failed to load categories: ${e.response?.statusCode ?? e.message}');
    }
  }
}
