using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Constants;
using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class DeviceTokenRepository : IDeviceTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceToken> UpsertAsync(DeviceToken token, CancellationToken ct = default)
        {
            var existing = await _context.DeviceTokens
                .FirstOrDefaultAsync(d => d.Token == token.Token, ct);

            var now = DateTime.UtcNow;

            if (existing is null)
            {
                token.CreatedAt = now;
                token.LastSeenAt = now;
                await _context.DeviceTokens.AddAsync(token, ct);
            }
            else
            {
                existing.UserId = token.UserId;
                existing.Platform = token.Platform;
                existing.LastSeenAt = now;
                _context.DeviceTokens.Update(existing);
                token = existing;
            }

            await _context.SaveChangesAsync(ct);
            return token;
        }

        public async Task<bool> DeleteByTokenAsync(string token, CancellationToken ct = default)
        {
            var rows = await _context.DeviceTokens
                .Where(d => d.Token == token)
                .ExecuteDeleteAsync(ct);
            return rows > 0;
        }

        public async Task<List<string>> GetTokensForUsersAsync(
            IEnumerable<Guid> userIds,
            CancellationToken ct = default)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new List<string>();

            return await _context.DeviceTokens
                .Where(d => ids.Contains(d.UserId))
                .Select(d => d.Token)
                .ToListAsync(ct);
        }

        public async Task<List<Guid>> GetAllCustomerUserIdsAsync(CancellationToken ct = default)
        {
            // Customers are users in the "Customer" role. Join AspNetUserRoles + AspNetRoles.
            var customerRoleId = IdentityRoleConstant.CustomerRoleGuid;

            return await _context.Set<IdentityUserRole<Guid>>()
                .Where(ur => ur.RoleId == customerRoleId)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken ct = default)
        {
            var list = tokens.Distinct().ToList();
            if (list.Count == 0) return;

            await _context.DeviceTokens
                .Where(d => list.Contains(d.Token))
                .ExecuteDeleteAsync(ct);
        }
    }
}
