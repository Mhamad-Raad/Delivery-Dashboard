using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IBuildingRepository
    {
        Task<Building?> GetByIdAsync(int id);
        Task<Building?> GetByNameAsync(string name);
        Task<Building?> GetByFloorIdForApartmentAsync(int floorId);
        Task<Building?> GetByApartmentIdAsync(int apartmentId);
        Task<(Building? Building, bool HasOccupant)> GetByApartmentIdForDeleteAsync(int apartmentId);
        Task<(Building? Building, bool HasOccupants)> GetByFloorIdAsync(int floorId);
        Task<(IEnumerable<Building> Buildings, int Total)> GetBuildingsPagedAsync(int page, int limit, string? searchName = null);
        Task<Building> CreateAsync(Building building);
        Task<Building> UpdateAsync(Building building);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
    }
}