using MalDash.Application.Requests.UserRequest;
using MalDash.Application.Responses.Common;
using MalDash.Application.Responses.UserResponses;
using MalDash.Domain.Enums;

namespace MalDash.Application.Abstracts.IService
{
    public interface IAccountService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<LoginResponse> LoginMobileAsync(LoginRequest loginRequest);
        Task<LoginResponse> LoginWebAsync(LoginRequest loginRequest);
        Task<LoginResponse> MobileRefreshTokenAsync(string? refreshToken);
        Task WebNewRefreshTokenAsync(string? refreshToken);
        Task LogoutAsync(string userId);
        Task<UserDetailResponse> GetUserByIdAsync(Guid userId);
        Task<bool> CheckRefreshTokenExistsAsync(string refreshToken);
        Task DeleteUserAsync(Guid userId);
        Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request);
        Task<UserResponse> UpdateCurrentUserAsync(Guid userId, UpdateCurrentUserRequest request);
        Task<PagedResponse<UserResponse>> GetFilteredUsersAsync(
            int page = 1,
            int limit = 10,
            Role? role = null,
            string? searchTerm = null,
            string? buildingNameSearch = null);
        Task<string> UploadUserImageAsync(Guid userId, UploadUserImageRequest request, string? baseUrl = null);
        Task DeleteUserImageAsync(Guid userId);
        Task<UserMeResponse> GetCurrentUserInfoAsync(Guid userId);
        Task UpdateUserProfileImageAsync(Guid userId, string imageUrl);
        Task ForgotPasswordAsync(ForgotPasswordRequest request, string resetUrl);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}