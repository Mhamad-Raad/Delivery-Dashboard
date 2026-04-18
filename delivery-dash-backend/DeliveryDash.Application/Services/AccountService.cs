using FluentValidation;
using DeliveryDash.Application.Abstracts;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.UserRequest;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.UserResponses;
using DeliveryDash.Application.Responses.VendorResponses;
using DeliveryDash.Application.Responses.VendorStaffResponses;
using DeliveryDash.Domain.Constants;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using DeliveryDash.Domain.Exceptions.UserExceptions;
using DeliveryDash.Domain.Exceptions.VendorExceptions;
using Microsoft.AspNetCore.Identity;

namespace DeliveryDash.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAuthTokenProcessor _authTokenProcessor;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorStaffRepository _vendorStaffRepository;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<UpdateUserRequest> _updateValidator;
        private readonly IValidator<UpdateCurrentUserRequest> _updateCurrentUserValidator;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

        public AccountService(
            IAuthTokenProcessor authTokenProcessor,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            IVendorRepository vendorRepository,
            IVendorStaffRepository vendorStaffRepository,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<UpdateUserRequest> updateValidator,
            IValidator<UpdateCurrentUserRequest> updateCurrentUserValidator,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator,
            IFileStorageService fileStorageService,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _authTokenProcessor = authTokenProcessor;
            _userManager = userManager;
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _vendorRepository = vendorRepository;
            _vendorStaffRepository = vendorStaffRepository;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _updateValidator = updateValidator;
            _updateCurrentUserValidator = updateCurrentUserValidator;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest Request)
        {
            await _registerValidator.ValidateAndThrowCustomAsync(Request);

            var userExists = await _userManager.FindByEmailAsync(Request.Email) != null;

            if (userExists)
            {
                throw new UserAlreadyExistsException(email: Request.Email);
            }

            var user = User.Create(
                Request.FirstName,
                Request.LastName,
                Request.Email,
                Request.PhoneNumber
            );

            var result = await _userManager.CreateAsync(user, Request.Password);

            if (!result.Succeeded)
            {
                throw new RegistrationFailedException(result.Errors.Select(x => x.Description));
            }

            await _userManager.AddToRoleAsync(user, GetIdentityRoleName(Request.Role));

            // Send welcome notification to the new user
            var roleDisplayName = Request.Role switch
            {
                Role.SuperAdmin => "Super Admin",
                Role.Admin => "Admin",
                Role.Vendor => "Vendor",
                Role.Tenant => "Tenant",
                Role.Driver => "Driver",
                _ => "User"
            };

            // Get all SuperAdmins
            var superAdmins = await _userManager.GetUsersInRoleAsync(IdentityRoleConstant.SuperAdmin);

            // Send notification to all SuperAdmins
            foreach (var superAdmin in superAdmins)
            {
                await _notificationService.SendNotificationAsync(
                    userId: superAdmin.Id,
                    title: "New User Registered",
                    message: $"A new user has been registered: {user.FirstName} {user.LastName} ({user.Email}) with the role of {roleDisplayName}.",
                    type: "Info",
                    actionUrl: $"/users/{user.Id}"
                );
            }

            return new RegisterResponse
            {
                Id = user.Id
            };
        }

        public async Task<LoginResponse> LoginMobileAsync(LoginRequest loginRequest)
        {
            await _loginValidator.ValidateAndThrowCustomAsync(loginRequest);

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                throw new LoginFailedException(loginRequest.Email);
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);


            var roleName = roles.FirstOrDefault() ?? throw new InvalidOperationException("User must have a role assigned.");
            var role = GetRoleFromIdentityRoleName(roleName);

            var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
            var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAt = expirationDateInUtc,
                RefreshTokenExpiresAt = refreshTokenExpirationDateInUtc,
                User = new UserInfo
                {
                    _id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageUrl = user.ProfileImageUrl
                }
            };
        }

        public async Task<LoginResponse> LoginWebAsync(LoginRequest loginRequest)
        {
            await _loginValidator.ValidateAndThrowCustomAsync(loginRequest);

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                throw new LoginFailedException(loginRequest.Email);
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);


            if (!string.IsNullOrEmpty(loginRequest.ApplicationContext))
            {
                if (!ApplicationContexts.IsValidContext(loginRequest.ApplicationContext, roles))
                {
                    throw new UnauthorizedAccessException(
                        "Access denied. You do not have permission to access this site.");
                }
            }

            var roleName = roles.FirstOrDefault() ?? throw new InvalidOperationException("User must have a role assigned.");
            var role = GetRoleFromIdentityRoleName(roleName);

            var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
            var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

            await _userManager.UpdateAsync(user);

            // Write cookies for web clients
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie(
                "ACCESS_TOKEN",
                jwtToken,
                expirationDateInUtc);
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie(
                "REFRESH_TOKEN",
                user.RefreshToken,
                refreshTokenExpirationDateInUtc);

            // Fetch vendor profile if user is a vendor
            VendorDetailResponse? vendorProfile = null;
            if (role == Role.Vendor)
            {
                var vendor = await _vendorRepository.GetByUserIdAsync(user.Id);
                if (vendor != null)
                {
                    vendorProfile = new VendorDetailResponse
                    {
                        Id = vendor.Id,
                        Name = vendor.Name,
                        Description = vendor.Description,
                        ProfileImageUrl = vendor.ProfileImageUrl,
                        OpeningTime = vendor.OpeningTime,
                        CloseTime = vendor.CloseTime,
                        UserId = user.Id,
                        Phone = user.PhoneNumber ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email ?? string.Empty
                    };
                }
            }

            return new LoginResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAt = expirationDateInUtc,
                RefreshTokenExpiresAt = refreshTokenExpirationDateInUtc,
                User = new UserInfo
                {
                    _id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageUrl = user.ProfileImageUrl
                },
                VendorProfile = vendorProfile
            };
        }

        public async Task<LoginResponse> MobileRefreshTokenAsync(string? refreshToken)
        {
            var (isValid, user, errorMessage) = await _userRepository.ValidateRefreshTokenAsync(refreshToken ?? string.Empty);

            if (!isValid || user == null)
            {
                throw new RefreshTokenException(errorMessage ?? "Invalid refresh token.");
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
            var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

            await _userManager.UpdateAsync(user);

            return new LoginResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAt = expirationDateInUtc,
                RefreshTokenExpiresAt = refreshTokenExpirationDateInUtc,
            };
        }

        public async Task WebNewRefreshTokenAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new RefreshTokenException("Refresh token is missing.");
            }

            var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);

            if (user == null)
            {
                throw new RefreshTokenException("Unable to retrieve user for refresh token");
            }

            if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
            {
                throw new RefreshTokenException("Refresh token is expired.");
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
            var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();

            var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshTokenValue;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

            await _userManager.UpdateAsync(user);

            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException();

            if (user.RefreshToken is null || user.RefreshTokenExpiresAtUtc is null)
                throw new UserIsNotLoggedInException();

            user.RefreshToken = null;
            user.RefreshTokenExpiresAtUtc = null;

            await _userManager.UpdateAsync(user);
        }

        public async Task<UserDetailResponse> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
                throw new UserNotFoundException();

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() 
                ?? throw new InvalidOperationException($"User {user.Email} must have a role assigned.");
            var role = GetRoleFromIdentityRoleName(roleName);

            // Get address info from AddressRepository
            var address = await _addressRepository.GetByUserIdAsync(userId);

            return new UserDetailResponse
            {
                _id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                BuildingName = address?.Building?.Name,
                BuildingId = address?.BuildingId,
                FloorNumber = address?.Floor?.FloorNumber,
                FloorId = address?.FloorId,
                ApartmentName = address?.Apartment?.ApartmentName,
                ApartmentId = address?.ApartmentId,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public async Task<bool> CheckRefreshTokenExistsAsync(string refreshToken)
        {
            var (isValid, _, _) = await _userRepository.ValidateRefreshTokenAsync(refreshToken);
            return isValid;
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
                throw new UserNotFoundException();

            await _userRepository.DeleteAsync(userId);
        }

        public async Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new UserNotFoundException();

            if (user.Email != request.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(request.Email);
                if (emailExists != null)
                    throw new UserAlreadyExistsException(request.Email);
            }

            // Update roles FIRST before updating other properties
            var currentRoles = await _userManager.GetRolesAsync(user);
            var newRoleName = GetIdentityRoleName(request.Role);

            // Always remove all existing roles first to ensure only one role
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    throw new UserUpdateFailedException(removeResult.Errors.Select(e => e.Description));
                }
            }

            // Add the new single role
            var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
            if (!addResult.Succeeded)
            {
                throw new UserUpdateFailedException(addResult.Errors.Select(e => e.Description));
            }

            // Now update other user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.NormalizedEmail = request.Email.ToUpper();
            user.NormalizedUserName = request.Email.ToUpper();

            await _userRepository.UpdateAsync(user);

            // Retrieve the updated role
            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? throw new InvalidOperationException($"User {user.Email} must have a role assigned.");
            var role = GetRoleFromIdentityRoleName(roleName);

            return new UserResponse
            {
                _id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public async Task<UserResponse> UpdateCurrentUserAsync(Guid userId, UpdateCurrentUserRequest request)
        {
            await _updateCurrentUserValidator.ValidateAndThrowCustomAsync(request);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException();

            var hasChanges = false;

            // Update first name if provided
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
                hasChanges = true;
            }

            // Update last name if provided
            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;
                hasChanges = true;
            }

            // Update email if provided and different
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(request.Email);
                if (emailExists != null && emailExists.Id != user.Id)
                    throw new UserAlreadyExistsException(request.Email);

                user.Email = request.Email;
                user.UserName = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.NormalizedUserName = request.Email.ToUpper();
                hasChanges = true;
            }

            // Update phone number if provided
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.PhoneNumber = request.PhoneNumber;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _userRepository.UpdateAsync(user);
            }

            // Get user's current role
            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault()
                ?? throw new InvalidOperationException($"User {user.Email} must have a role assigned.");
            var role = GetRoleFromIdentityRoleName(roleName);

            return new UserResponse
            {
                _id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }

        public async Task<PagedResponse<UserResponse>> GetFilteredUsersAsync(
            int page = 1,
            int limit = 10,
            Role? role = null,
            string? searchTerm = null,
            string? buildingNameSearch = null)
        {
            var (users, totalCount) = await _userRepository.GetFilteredUsersAsync(
                page, limit, role, searchTerm, buildingNameSearch);

            var userList = users.ToList();
            var userIds = userList.Select(u => u.Id).ToList();

            // Batch load addresses in ONE query
            var addresses = await _addressRepository.GetByUserIdsAsync(userIds);
            var addressLookup = addresses.ToDictionary(a => a.UserId!.Value);

            // Batch load all roles in ONE query via repository
            var userRoles = await _userRepository.GetUserRolesDictionaryAsync(userIds);

            var userResponses = new List<UserResponse>();

            foreach (var user in userList)
            {
                // Get role from dictionary instead of hitting DB
                if (!userRoles.TryGetValue(user.Id, out var roleName))
                {
                    throw new InvalidOperationException($"User {user.Email} must have a role assigned.");
                }

                // Skip VendorStaff users - they're managed separately via vendor staff endpoints
                if (roleName == IdentityRoleConstant.VendorStaff)
                {
                    continue;
                }

                var roleEnum = GetRoleFromIdentityRoleName(roleName);

                // Get address from lookup
                addressLookup.TryGetValue(user.Id, out var address);

                userResponses.Add(new UserResponse
                {
                    _id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Role = roleEnum,
                    BuildingName = address?.Building?.Name,
                    FloorNumber = address?.Floor?.FloorNumber,
                    ApartmentName = address?.Apartment?.ApartmentName,
                    ProfileImageUrl = user.ProfileImageUrl
                });
            }

            return new PagedResponse<UserResponse>
            {
                Data = userResponses,
                Page = page,
                Limit = limit,
                Total = totalCount
            };
        }

        private string GetIdentityRoleName(Role role)
        {
            return role switch
            {
                Role.SuperAdmin => IdentityRoleConstant.SuperAdmin,
                Role.Admin => IdentityRoleConstant.Admin,
                Role.Vendor => IdentityRoleConstant.Vendor,
                Role.Tenant => IdentityRoleConstant.Tenant,
                Role.Driver => IdentityRoleConstant.Driver,
                Role.VendorStaff => IdentityRoleConstant.VendorStaff,
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, $"Provided role is not supported"),
            };
        }

        private Role GetRoleFromIdentityRoleName(string roleName)
        {
            return roleName switch
            {
                IdentityRoleConstant.SuperAdmin => Role.SuperAdmin,
                IdentityRoleConstant.Admin => Role.Admin,
                IdentityRoleConstant.Vendor => Role.Vendor,
                IdentityRoleConstant.Tenant => Role.Tenant,
                IdentityRoleConstant.Driver => Role.Driver,
                IdentityRoleConstant.VendorStaff => Role.VendorStaff,
                _ => throw new ArgumentOutOfRangeException(nameof(roleName), roleName, $"Provided role name is not supported"),
            };
        }

        // Update the method signature to accept the domain-agnostic request
        public async Task<string> UploadUserImageAsync(Guid userId, UploadUserImageRequest request, string? baseUrl = null)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException();

            // Validate image
            _fileStorageService.ValidateImage(request.ImageStream, 5 * 1024 * 1024); // 5MB limit

            // Delete old image if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteImageAsync(user.ProfileImageUrl);
            }

            // Save new image with baseUrl
            var imageUrl = await _fileStorageService.SaveImageAsync(
                request.ImageStream, 
                request.FileName, 
                "users",
                baseUrl);
            
            // Update user
            user.ProfileImageUrl = imageUrl;
            await _userRepository.UpdateAsync(user);

            return imageUrl;
        }

        public async Task DeleteUserImageAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException();

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteImageAsync(user.ProfileImageUrl);
                user.ProfileImageUrl = null;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<UserMeResponse> GetCurrentUserInfoAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
                throw new UserNotFoundException();

            IList<string> roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? throw new InvalidOperationException("User must have a role assigned.");
            var role = GetRoleFromIdentityRoleName(roleName);

            // Check if user is vendor staff
            var isVendorStaff = roles.Contains(IdentityRoleConstant.VendorStaff) || 
                                roles.Contains(IdentityRoleConstant.Driver);

            // Fetch vendor profile if user is a vendor
            VendorDetailResponse? vendorProfile = null;
            if (role == Role.Vendor)
            {
                var vendor = await _vendorRepository.GetByUserIdAsync(user.Id);
                if (vendor != null)
                {
                    vendorProfile = new VendorDetailResponse
                    {
                        Id = vendor.Id,
                        Name = vendor.Name,
                        Description = vendor.Description,
                        ProfileImageUrl = vendor.ProfileImageUrl,
                        UserProfileImageUrl = vendor.User?.ProfileImageUrl,
                        OpeningTime = vendor.OpeningTime,
                        CloseTime = vendor.CloseTime,
                        Type = vendor.Type.ToString(),
                        UserId = user.Id,
                        Phone = user.PhoneNumber ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email ?? string.Empty
                    };
                }
            }

            // Fetch staff profile if user is vendor staff/driver
            VendorStaffResponse? staffProfile = null;
            if (isVendorStaff)
            {
                var staffRecord = await _vendorStaffRepository.GetByUserIdAsync(user.Id);
                
                if (staffRecord != null)
                {
                    var staffRoleName = roles.Contains(IdentityRoleConstant.Driver) ? "Driver" : "Staff";
                    
                    // Get vendor details for staff profile
                    var vendor = await _vendorRepository.GetByIdAsync(staffRecord.VendorId);
                    VendorDetailResponse? vendorDetail = null;
                    
                    if (vendor != null)
                    {
                        vendorDetail = new VendorDetailResponse
                        {
                            Id = vendor.Id,
                            Name = vendor.Name,
                            Description = vendor.Description,
                            ProfileImageUrl = vendor.ProfileImageUrl,
                            UserProfileImageUrl = vendor.User?.ProfileImageUrl,
                            OpeningTime = vendor.OpeningTime,
                            CloseTime = vendor.CloseTime,
                            Type = vendor.Type.ToString(),
                            UserId = vendor.UserId,
                            Phone = vendor.User?.PhoneNumber ?? string.Empty,
                            FirstName = vendor.User?.FirstName ?? string.Empty,
                            LastName = vendor.User?.LastName ?? string.Empty,
                            Email = vendor.User?.Email ?? string.Empty
                        };
                    }
                    
                    staffProfile = new VendorStaffResponse
                    {
                        Id = staffRecord.Id,
                        UserId = staffRecord.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email ?? string.Empty,
                        Phone = user.PhoneNumber,
                        ProfileImageUrl = user.ProfileImageUrl,
                        VendorId = staffRecord.VendorId,
                        VendorName = staffRecord.Vendor?.Name ?? string.Empty,
                        Role = staffRoleName,
                        AssignedDate = staffRecord.AssignedDate,
                        IsActive = staffRecord.IsActive,
                        VendorProfile = vendorDetail
                    };
                }
            }

            return new UserMeResponse
            {
                User = new UserInfo
                {
                    _id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageUrl = user.ProfileImageUrl
                },
                VendorProfile = vendorProfile,
                StaffProfile = staffProfile
            };
        }
        public async Task UpdateUserProfileImageAsync(Guid userId, string imageUrl)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UserNotFoundException();

            user.ProfileImageUrl = imageUrl;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string resetUrl)
        {
            await _forgotPasswordValidator.ValidateAndThrowCustomAsync(request);

            var user = await _userManager.FindByEmailAsync(request.Email);
            
            // Always return success to prevent email enumeration attacks
            if (user == null)
                return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            await _emailService.SendPasswordResetEmailAsync(user.Email!, token, resetUrl);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            await _resetPasswordValidator.ValidateAndThrowCustomAsync(request);

            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
                throw new UserNotFoundException();

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Password reset failed: {errors}");
            }

            // Invalidate refresh tokens after password reset
            user.RefreshToken = null;
            user.RefreshTokenExpiresAtUtc = null;
            await _userManager.UpdateAsync(user);
        }
    }
}