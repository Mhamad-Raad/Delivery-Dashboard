using MalDash.Application.Abstracts.IRepository;
using MalDash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MalDash.Infrastructure.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public AddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _context.Addresses
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.Apartment)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Address?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Addresses
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.Apartment)
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Address?> GetByApartmentIdAsync(int apartmentId)
        {
            return await _context.Addresses
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId);
        }

        public async Task<List<Address>> GetByUserIdsAsync(IEnumerable<Guid> userIds)
        {
            return await _context.Addresses
                .Include(a => a.Building)
                .Include(a => a.Floor)
                .Include(a => a.Apartment)
                .AsNoTracking()
                .Where(a => a.UserId != null && userIds.Contains(a.UserId.Value))
                .ToListAsync();
        }

        public async Task<List<Address>> GetByBuildingIdAsync(int buildingId)
        {
            return await _context.Addresses
                .Include(a => a.User)
                .Include(a => a.Floor)
                .Include(a => a.Apartment)
                .AsNoTracking()
                .Where(a => a.BuildingId == buildingId)
                .ToListAsync();
        }

        public async Task<Address> CreateAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAsync(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task DeleteAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }
    }
}