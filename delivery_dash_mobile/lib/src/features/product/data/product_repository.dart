import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';
import 'product_model.dart';

final productRepositoryProvider = Provider<ProductRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return ProductRepository(dio: dio);
});

class ProductRepository {
  final Dio _dio;

  ProductRepository({required Dio dio}) : _dio = dio;

  Future<List<Product>> getProducts({
    int? vendorId,
    int? categoryId,
    int page = 1,
    int limit = 100,
    String? searchName,
    bool? inStock,
  }) async {
    try {
      final queryParams = <String, dynamic>{
        'page': page,
        'limit': limit,
      };
      if (vendorId != null) queryParams['vendorId'] = vendorId;
      if (categoryId != null) queryParams['categoryId'] = categoryId;
      if (searchName != null) queryParams['searchName'] = searchName;
      if (inStock != null) queryParams['inStock'] = inStock;

      final response = await _dio.get('/Product/tenant', queryParameters: queryParams);

      if (response.statusCode == 200) {
        // Handle both paginated response and direct array
        final dynamic data = response.data;
        
        if (data is Map<String, dynamic>) {
          // Paginated response with items/data array
          final items = data['items'] ?? data['data'] ?? [];
          return (items as List).map((json) => Product.fromJson(json as Map<String, dynamic>)).toList();
        } else if (data is List) {
          // Direct array response
          return data.map((json) => Product.fromJson(json as Map<String, dynamic>)).toList();
        } else {
          return [];
        }
      } else {
        throw Exception('Failed to load products: ${response.statusCode}');
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 403) {
        throw Exception('Access denied. Your account may not have permission to view products.');
      } else if (e.response?.statusCode == 401) {
        throw Exception('Authentication required. Please log in again.');
      }
      throw Exception('Failed to load products: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      rethrow;
    }
  }

  Future<Product> getProductById(int id) async {
    try {
      final response = await _dio.get('/Product/$id');

      if (response.statusCode == 200) {
        return Product.fromJson(response.data as Map<String, dynamic>);
      } else {
        throw Exception('Failed to load product: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw Exception('Failed to load product: ${e.message}');
    } catch (e) {
      rethrow;
    }
  }
}
