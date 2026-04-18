using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryDash.Infrastructure.Repositories
{
    public class EntityImageRepository : IEntityImageRepository
    {
        private readonly ApplicationDbContext _context;

        public EntityImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddImagesAsync(string entityType, string entityId, List<string> imageUrls)
        {
            var images = imageUrls.Select((url, index) => new EntityImage
            {
                EntityType = entityType,
                EntityId = entityId,
                ImageUrl = url,
                DisplayOrder = index,
                IsPrimary = index == 0
            });

            _context.EntityImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EntityImage>> GetImagesAsync(string entityType, string entityId)
        {
            return await _context.EntityImages
                .Where(e => e.EntityType == entityType && e.EntityId == entityId)
                .OrderBy(e => e.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteImagesAsync(string entityType, string entityId)
        {
            await _context.EntityImages
                .Where(e => e.EntityType == entityType && e.EntityId == entityId)
                .ExecuteDeleteAsync();
        }
    }
}