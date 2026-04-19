using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DeliveryDash.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorStaffRepository _vendorStaffRepository;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IVendorRepository vendorRepository,
            IVendorStaffRepository vendorStaffRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _vendorRepository = vendorRepository;
            _vendorStaffRepository = vendorStaffRepository;
        }

        public Guid GetCurrentUserId()
        {
            var userId = GetCurrentUserIdOrNull();
            
            if (userId is null)
                throw new UnauthorizedAccessException("User ID not found.");

            return userId.Value;
        }

        public Guid? GetCurrentUserIdOrNull()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        public string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection
                .RemoteIpAddress?.ToString();
        }

        public string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request
                .Headers["User-Agent"].ToString();
        }

        public async Task<int?> GetCurrentVendorIdAsync()
        {
            var userId = GetCurrentUserIdOrNull();
            if (userId is null) return null;
            
            var vendor = await _vendorRepository.GetByUserIdAsync(userId.Value);
            return vendor?.Id;
        }

        public async Task<int?> GetCurrentStaffVendorIdAsync()
        {
            var userId = GetCurrentUserIdOrNull();
            if (userId is null) return null;
            
            var staff = await _vendorStaffRepository.GetByUserIdAsync(userId.Value);
            return staff?.VendorId;
        }

        public async Task<int?> GetCurrentCustomerIdAsync()
        {
            // Future implementation
            return null;
        }
    }
}