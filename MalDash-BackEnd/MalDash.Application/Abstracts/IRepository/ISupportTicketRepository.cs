using MalDash.Domain.Entities;
using MalDash.Domain.Enums;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface ISupportTicketRepository
    {
        Task<SupportTicket?> GetByIdAsync(int id);
        Task<(IEnumerable<SupportTicket> Tickets, int Total)> GetTicketsPagedAsync(
            int page, int limit, Guid? userId = null, TicketStatus? status = null, TicketPriority? priority = null);
        Task<SupportTicket> CreateAsync(SupportTicket ticket);
        Task<SupportTicket> UpdateAsync(SupportTicket ticket);
        Task<int> GetTodayTicketCountAsync();
    }
}