using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IEntityImageRepository
    {
        Task AddImagesAsync(string entityType, string entityId, List<string> imageUrls);
        Task<List<EntityImage>> GetImagesAsync(string entityType, string entityId);
        Task DeleteImagesAsync(string entityType, string entityId);
    }
}