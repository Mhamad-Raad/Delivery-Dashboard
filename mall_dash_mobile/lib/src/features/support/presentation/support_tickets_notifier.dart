import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/support_ticket_model.dart';
import '../data/support_ticket_repository.dart';

final supportTicketsProvider =
    AsyncNotifierProvider<SupportTicketsNotifier, List<SupportTicket>>(
  SupportTicketsNotifier.new,
);

class SupportTicketsNotifier extends AsyncNotifier<List<SupportTicket>> {
  @override
  Future<List<SupportTicket>> build() async {
    return _fetchTickets();
  }

  Future<List<SupportTicket>> _fetchTickets() async {
    final repository = ref.read(supportTicketRepositoryProvider);
    return await repository.getMyTickets();
  }

  Future<void> refresh() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() => _fetchTickets());
  }
}
