using DeliveryDash.Domain.Entities;

namespace DeliveryDash.Application.Abstracts.IRepository
{
    public interface IDeviceTokenRepository
    {
        Task<DeviceToken> UpsertAsync(DeviceToken token, CancellationToken ct = default);
        Task<bool> DeleteByTokenAsync(string token, CancellationToken ct = default);
        Task<List<string>> GetTokensForUsersAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
        Task<List<Guid>> GetAllCustomerUserIdsAsync(CancellationToken ct = default);
        Task RemoveTokensAsync(IEnumerable<string> tokens, CancellationToken ct = default);
    }
}
