using MalDash.Application.Abstracts.IRepository;
using MalDash.Domain.Entities;
using MalDash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Repositories
{
    public class SupportTicketRepository : ISupportTicketRepository
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupportTicket?> GetByIdAsync(int id)
        {
            return await _context.SupportTickets
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<(IEnumerable<SupportTicket> Tickets, int Total)> GetTicketsPagedAsync(
            int page, int limit, Guid? userId = null, TicketStatus? status = null, TicketPriority? priority = null)
        {
            var query = _context.SupportTickets
                .Include(t => t.User)
                .AsNoTracking()
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            var total = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (tickets, total);
        }

        public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
        {
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(ticket.Id))!;
        }

        public async Task<SupportTicket> UpdateAsync(SupportTicket ticket)
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(ticket.Id))!;
        }

        public async Task<int> GetTodayTicketCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.SupportTickets
                .Where(t => t.CreatedAt >= today && t.CreatedAt < today.AddDays(1))
                .CountAsync();
        }
    }
}