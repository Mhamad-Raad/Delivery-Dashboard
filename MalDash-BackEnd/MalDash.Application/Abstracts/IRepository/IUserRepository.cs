using MalDash.Domain.Entities;
using MalDash.Domain.Enums;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface IUserRepository
    {
        Task<(bool IsValid, User? User, string? ErrorMessage)> ValidateRefreshTokenAsync(string refreshToken);
        Task<User?> GetByIdAsync(Guid userId);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task<Dictionary<Guid, string>> GetUserRolesDictionaryAsync(IEnumerable<Guid> userIds);
        Task<(IEnumerable<User> Users, int TotalCount)> GetFilteredUsersAsync(
            int page,
            int limit,
            Role? role = null,
            string? searchTerm = null,
            string? buildingNameSearch = null);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid userId);
    }
}