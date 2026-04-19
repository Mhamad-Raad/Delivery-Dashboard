using DeliveryDash.API.Extensions;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests;
using DeliveryDash.Application.Requests.UserRequest;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;

        public AccountController(
            IAccountService accountService,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        [HttpPost("register")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Registers a new user account with the provided details. Accepts multipart form data including user information and an optional profile image (max 5MB). Sends a welcome notification to all SuperAdmins upon successful registration.")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request, IFormFile? ProfileImageUrl)
        {
            var result = await _accountService.RegisterAsync(request);

            // If an image was provided, upload it using the universal service
            var imageUpload = ProfileImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                var imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "users",
                    Request.GetBaseUrl());

                // Update the user's profile image URL
                await _accountService.UpdateUserProfileImageAsync(result.Id, imageUrl);
            }

            return Ok(new { message = "User registered successfully. Please login." });
        }

        [HttpPost("login/mobile")]
        [EndpointDescription("Authenticates a user for mobile applications. Returns JWT access token, refresh token, and user information including role. Tokens are returned in the response body for mobile storage.")]
        public async Task<IActionResult> LoginMobile([FromBody] LoginRequest request)
        {
            var loginResponse = await _accountService.LoginMobileAsync(request);
            return Ok(loginResponse);
        }

        [HttpPost("login")]
        [EndpointDescription("Authenticates a user for web applications. Returns JWT access token, refresh token, user information, and vendor profile if applicable. Tokens are also set as HTTP-only cookies for secure web authentication.")]
        public async Task<IActionResult> LoginWeb([FromBody] LoginRequest request)
        {
            var loginResponse = await _accountService.LoginWebAsync(request);
            return Ok(loginResponse);
        }

        [Authorize]
        [HttpPost("logout/mobile")]
        [EndpointDescription("Logs out the authenticated user from mobile applications. Invalidates the user's refresh token on the server, requiring re-authentication for future requests.")]
        public async Task<IActionResult> LogoutMobile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _accountService.LogoutAsync(userId);

            return Ok(new { message = "User logged out successfully." });
        }

        [Authorize]
        [HttpPost("logout")]
        [EndpointDescription("Logs out the authenticated user from web applications. Invalidates the user's refresh token and clears the ACCESS_TOKEN and REFRESH_TOKEN cookies from the browser.")]
        public async Task<IActionResult> LogoutWeb()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _accountService.LogoutAsync(userId);

            // Remove cookies
            Response.Cookies.Delete("ACCESS_TOKEN");
            Response.Cookies.Delete("REFRESH_TOKEN");

            return Ok(new { message = "User logged out successfully." });
        }

        [HttpGet("me")]
        [Authorize]
        [EndpointDescription("Retrieves the complete profile of the currently authenticated user. Returns user details, role information, vendor profile (if user is a Vendor), and staff profile (if user is VendorStaff or Driver).")]
        public async Task<IActionResult> GetMe()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var userInfo = await _accountService.GetCurrentUserInfoAsync(userId);

            return Ok(userInfo);
        }

        [HttpPost("Mobile/refresh")]
        [EndpointDescription("Refreshes authentication tokens for mobile applications. Accepts the current refresh token and returns new access and refresh tokens. Used to maintain user sessions without re-authentication.")]
        public async Task<IActionResult> MobileRefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _accountService.MobileRefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }

        [HttpPost("Web/refresh")]
        [EndpointDescription("Refreshes authentication tokens for web applications. Reads the refresh token from HTTP-only cookies and sets new ACCESS_TOKEN and REFRESH_TOKEN cookies. Enables seamless session renewal for web clients.")]
        public async Task<IActionResult> WebRefreshToken()
        {
            var refreshToken = HttpContext.Request.Cookies["REFRESH_TOKEN"];

            await _accountService.WebNewRefreshTokenAsync(refreshToken);

            return Ok(new { message = "Tokens refreshed successfully" });
        }

        [HttpPost("validate-token")]
        [EndpointDescription("Validates whether a refresh token is still active and valid. Returns the validation status. Useful for checking session validity before attempting token refresh.")]
        public async Task<IActionResult> ValidateRefreshToken([FromBody] RefreshTokenRequest request)
        {
            var exists = await _accountService.CheckRefreshTokenExistsAsync(request.RefreshToken);

            if (!exists)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            return Ok(new { isValid = exists });
        }

        [HttpGet("user/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [EndpointDescription("Retrieves detailed user information by user ID. Returns user profile including personal details, role, address information (building, floor, apartment), and profile image. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [EndpointDescription("Permanently deletes a user account by ID. This action removes the user and all associated data. Restricted to SuperAdmin and Admin roles. Cannot be undone.")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _accountService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully." });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin, Customer")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Updates a user's information by ID. Accepts multipart form data with updated user details and an optional new profile image (max 5MB). Previous image is automatically deleted when replaced. Can update user role.")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UpdateUserRequest request, IFormFile? ProfileImageUrl)
        {
            var user = await _accountService.UpdateUserAsync(id, request);

            // If an image was provided, replace the old one
            var imageUpload = ProfileImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(user.ProfileImageUrl);
                }

                // Upload new image
                var imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "users",
                    Request.GetBaseUrl());

                // Update the user's profile image URL
                await _accountService.UpdateUserProfileImageAsync(id, imageUrl);
                user.ProfileImageUrl = imageUrl;
            }

            return Ok(user);
        }

        [HttpPut("me")]
        [Authorize]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Allows authenticated users to update their own profile. Accepts multipart form data with optional fields (first name, last name, email, phone) and an optional new profile image (max 5MB). Only provided fields are updated.")]
        public async Task<IActionResult> UpdateCurrentUser([FromForm] UpdateCurrentUserRequest request, IFormFile? ProfileImageUrl)
        {
            var userId = _currentUserService.GetCurrentUserId();

            var user = await _accountService.UpdateCurrentUserAsync(userId, request);

            // If an image was provided, replace the old one
            var imageUpload = ProfileImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(user.ProfileImageUrl);
                }

                // Upload new image
                var imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "users",
                    Request.GetBaseUrl());

                // Update the user's profile image URL
                await _accountService.UpdateUserProfileImageAsync(userId, imageUrl);
                user.ProfileImageUrl = imageUrl;
            }

            return Ok(user);
        }

        [HttpGet("users")]
        [EndpointDescription("Retrieves a paginated list of users with optional filtering. Supports filtering by role, search term (matches name/email), and building name. Returns user details including address information. Excludes VendorStaff users.")]
        public async Task<IActionResult> GetFilteredUsers(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] Role? role = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? buildingNameSearch = null)
        {
            var result = await _accountService.GetFilteredUsersAsync(page, limit, role, searchTerm, buildingNameSearch);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        [EndpointDescription("Initiates the password reset process for a user. Sends a password reset email with a secure token to the provided email address. Always returns success to prevent email enumeration attacks.")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Build reset URL - adjust to your frontend password reset page
            var resetUrl = $"{Request.GetBaseUrl()}/reset-password";
            
            await _accountService.ForgotPasswordAsync(request, resetUrl);
            
            // Always return success to prevent email enumeration
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [EndpointDescription("Completes the password reset process. Validates the reset token and updates the user's password. Invalidates all existing refresh tokens, requiring the user to log in again with the new password.")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _accountService.ResetPasswordAsync(request);
            return Ok(new { message = "Password has been reset successfully. Please login with your new password." });
        }
    }
}