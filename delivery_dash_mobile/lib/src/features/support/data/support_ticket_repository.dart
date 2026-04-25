import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/network/dio_provider.dart';
import 'support_ticket_model.dart';

final supportTicketRepositoryProvider = Provider<SupportTicketRepository>((ref) {
  final dio = ref.watch(dioProvider);
  return SupportTicketRepository(dio: dio);
});

class SupportTicketRepository {
  final Dio _dio;

  SupportTicketRepository({required Dio dio}) : _dio = dio;

  Future<SupportTicket> createTicket(CreateSupportTicketRequest request) async {
    try {
      print('Creating support ticket: ${request.toJson()}');
      
      final formData = FormData.fromMap({
        'Subject': request.subject,
        'Description': request.description,
        'Priority': request.priority,
      });

      final response = await _dio.post(
        '/SupportTicket',
        data: formData,
      );

      print('Ticket creation response status: ${response.statusCode}');
      print('Ticket creation response data type: ${response.data.runtimeType}');
      print('Ticket creation response data: ${response.data}');

      if (response.statusCode == 200 || response.statusCode == 201) {
        if (response.data == null) {
          throw Exception('Server returned null data');
        }

        // Handle different response types
        if (response.data is Map<String, dynamic>) {
          final data = response.data as Map<String, dynamic>;
          
          print('Response keys: ${data.keys.toList()}');
          print('Detailed response: $data');
          
          // Check if backend wraps response in a message/ticket structure
          if (data.containsKey('ticket')) {
            print('Parsing from ticket wrapper');
            return SupportTicket.fromJson(data['ticket'] as Map<String, dynamic>);
          }
          
          print('Parsing directly from response');
          return SupportTicket.fromJson(data);
        } else if (response.data is String) {
          throw Exception('Server returned string response instead of JSON: ${response.data}');
        } else {
          throw Exception('Unexpected response type: ${response.data.runtimeType}');
        }
      } else {
        throw Exception('Failed to create ticket: ${response.statusCode}');
      }
    } on DioException catch (e) {
      print('DioException creating ticket: ${e.response?.statusCode} - ${e.response?.data}');
      if (e.response?.statusCode == 400) {
        final errorData = e.response?.data;
        String errorMessage = 'Invalid ticket data.';
        
        if (errorData is Map<String, dynamic>) {
          errorMessage = errorData['error'] ?? errorData['message'] ?? errorMessage;
        }
        
        throw Exception(errorMessage);
      } else if (e.response?.statusCode == 401) {
        throw Exception('Authentication required. Please log in again.');
      }
      throw Exception('Failed to create ticket: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      print('Error creating ticket: $e');
      print('Error stack trace: ${StackTrace.current}');
      rethrow;
    }
  }

  Future<List<SupportTicket>> getMyTickets({
    int page = 1,
    int limit = 20,
    String? status,
  }) async {
    try {
      print('Fetching my tickets - page: $page, limit: $limit, status: $status');
      
      final response = await _dio.get(
        '/SupportTicket/my-tickets',
        queryParameters: {
          'page': page,
          'limit': limit,
          if (status != null) 'status': status,
        },
      );

      print('My tickets response status: ${response.statusCode}');
      print('My tickets response data type: ${response.data.runtimeType}');
      print('My tickets response data: ${response.data}');

      if (response.statusCode == 200) {
        final dynamic data = response.data;
        
        if (data is Map<String, dynamic>) {
          print('Response is Map - Keys: ${data.keys.toList()}');
          final items = data['items'] ?? data['data'] ?? [];
          print('Items count: ${items is List ? items.length : "not a list"}');
          return (items as List).map((json) => SupportTicket.fromJson(json as Map<String, dynamic>)).toList();
        } else if (data is List) {
          print('Response is List - Length: ${data.length}');
          return data.map((json) => SupportTicket.fromJson(json as Map<String, dynamic>)).toList();
        } else {
          print('Response is neither Map nor List - returning empty list');
          return [];
        }
      } else {
        throw Exception('Failed to load tickets: ${response.statusCode}');
      }
    } on DioException catch (e) {
      print('DioException loading tickets: ${e.response?.statusCode} - ${e.response?.data}');
      if (e.response?.statusCode == 401) {
        throw Exception('Authentication required. Please log in again.');
      }
      throw Exception('Failed to load tickets: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      print('Error loading tickets: $e');
      rethrow;
    }
  }

  Future<SupportTicket> getTicketById(int id) async {
    try {
      print('Fetching ticket ID: $id');
      
      final response = await _dio.get('/SupportTicket/$id');

      print('Ticket by ID response status: ${response.statusCode}');
      print('Ticket by ID response data: ${response.data}');

      if (response.statusCode == 200) {
        return SupportTicket.fromJson(response.data as Map<String, dynamic>);
      } else {
        throw Exception('Failed to load ticket: ${response.statusCode}');
      }
    } on DioException catch (e) {
      print('DioException loading ticket by ID: ${e.response?.statusCode} - ${e.response?.data}');
      if (e.response?.statusCode == 404) {
        throw Exception('Ticket not found');
      }
      throw Exception('Failed to load ticket: ${e.response?.statusCode ?? e.message}');
    } catch (e) {
      print('Error loading ticket by ID: $e');
      rethrow;
    }
  }
}
