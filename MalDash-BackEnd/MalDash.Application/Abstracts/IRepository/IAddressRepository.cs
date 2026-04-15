using MalDash.Domain.Entities;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface IAddressRepository
    {
        Task<Address?> GetByIdAsync(int id);
        Task<Address?> GetByUserIdAsync(Guid userId);
        Task<Address?> GetByApartmentIdAsync(int apartmentId);
        Task<List<Address>> GetByUserIdsAsync(IEnumerable<Guid> userIds);
        Task<List<Address>> GetByBuildingIdAsync(int buildingId);
        Task<Address> CreateAsync(Address address);
        Task<Address> UpdateAsync(Address address);
        Task DeleteAsync(int id);
    }
}