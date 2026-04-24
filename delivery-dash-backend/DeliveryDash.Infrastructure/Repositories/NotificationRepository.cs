using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Responses.NotificationResponses;
using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int skip, int take)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task DeleteOldNotificationsAsync(Guid userId, int maxCount)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(maxCount)
                .Select(n => n.Id)
                .ToListAsync();

            if (notifications.Any())
            {
                await _context.Notifications
                    .Where(n => notifications.Contains(n.Id))
                    .ExecuteDeleteAsync();
            }
        }

        // A "broadcast" is identified by (Title, Message, CreatedAt) with Type="Broadcast".
        // CreatedAt is assigned once per BroadcastAsync call, so the tuple is unique per broadcast.
        public async Task<List<BroadcastSummaryResponse>> GetBroadcastsAsync(int skip, int take)
        {
            return await _context.Notifications
                .Where(n => n.Type == "Broadcast")
                .GroupBy(n => new { n.Title, n.Message, n.ImageUrl, n.CreatedAt })
                .Select(g => new BroadcastSummaryResponse
                {
                    Key = g.Min(n => n.Id),
                    Title = g.Key.Title,
                    Message = g.Key.Message,
                    ImageUrl = g.Key.ImageUrl,
                    CreatedAt = g.Key.CreatedAt,
                    Recipients = g.Count(),
                })
                .OrderByDescending(b => b.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<BroadcastSummaryResponse?> GetBroadcastByKeyAsync(int key)
        {
            var seed = await _context.Notifications.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == key && n.Type == "Broadcast");
            if (seed is null) return null;

            var recipients = await _context.Notifications.CountAsync(n =>
                n.Type == "Broadcast" &&
                n.Title == seed.Title &&
                n.Message == seed.Message &&
                n.CreatedAt == seed.CreatedAt);

            return new BroadcastSummaryResponse
            {
                Key = key,
                Title = seed.Title,
                Message = seed.Message,
                ImageUrl = seed.ImageUrl,
                CreatedAt = seed.CreatedAt,
                Recipients = recipients,
            };
        }

        public async Task<int> DeleteBroadcastByKeyAsync(int key)
        {
            var seed = await _context.Notifications.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == key && n.Type == "Broadcast");
            if (seed is null) return 0;

            return await _context.Notifications
                .Where(n =>
                    n.Type == "Broadcast" &&
                    n.Title == seed.Title &&
                    n.Message == seed.Message &&
                    n.CreatedAt == seed.CreatedAt)
                .ExecuteDeleteAsync();
        }
    }
}