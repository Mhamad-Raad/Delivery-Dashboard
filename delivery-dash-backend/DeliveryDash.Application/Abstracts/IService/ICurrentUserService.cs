namespace DeliveryDash.Application.Abstracts.IService
{
    public interface ICurrentUserService
    {
        Guid GetCurrentUserId();
        Guid? GetCurrentUserIdOrNull();
        string? GetCurrentUserEmail();
        string? GetIpAddress();
        string? GetUserAgent();
        Task<int?> GetCurrentVendorIdAsync();
        Task<int?> GetCurrentStaffVendorIdAsync();
        Task<int?> GetCurrentTenantIdAsync();
    }
}