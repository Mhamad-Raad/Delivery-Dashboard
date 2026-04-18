using FluentValidation;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.VendorStaffRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.VendorStaffResponses;
using DeliveryDash.Domain.Constants;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Exceptions.UserExceptions;
using DeliveryDash.Domain.Exceptions.VendorExceptions;
using Microsoft.AspNetCore.Identity;

namespace DeliveryDash.Application.Services
{
    public class VendorStaffService : IVendorStaffService
    {
        private readonly IVendorStaffRepository _vendorStaffRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IValidator<CreateVendorStaffRequest> _createValidator;
        private readonly IValidator<UpdateVendorStaffRequest> _updateValidator;
        private readonly IAccountService _accountService;

        public VendorStaffService(
            IVendorStaffRepository vendorStaffRepository,
            IVendorRepository vendorRepository,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IValidator<CreateVendorStaffRequest> createValidator,
            IValidator<UpdateVendorStaffRequest> updateValidator,
            IAccountService accountService)
        {
            _vendorStaffRepository = vendorStaffRepository;
            _vendorRepository = vendorRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _accountService = accountService;
        }

        public async Task<VendorStaffResponse> GetByIdAsync(int id)
        {
            var staff = await _vendorStaffRepository.GetByIdAsync(id)
                ?? throw new VendorStaffNotFoundException(id);

            return await MapToResponseAsync(staff);
        }

        public async Task<VendorStaffResponse?> GetByUserIdAsync(Guid userId)
        {
            var staff = await _vendorStaffRepository.GetByUserIdAsync(userId);
            return staff == null ? null : await MapToResponseAsync(staff);
        }

        public async Task<PagedResponse<VendorStaffResponse>> GetStaffByVendorIdAsync(
            int vendorId,
            int page = 1,
            int limit = 10,
            bool? isActive = null,
            string? searchTerm = null)
        {
            var (staffList, total) = await _vendorStaffRepository.GetStaffByVendorIdPagedAsync(
                vendorId, page, limit, isActive, searchTerm);

            var responses = new List<VendorStaffResponse>();
            foreach (var staff in staffList)
            {
                responses.Add(await MapToResponseAsync(staff));
            }

            return new PagedResponse<VendorStaffResponse>
            {
                Data = responses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<VendorStaffResponse> CreateAsync(CreateVendorStaffRequest request, int vendorId)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            // Check if vendor exists
            var vendor = await _vendorRepository.GetByIdAsync(vendorId)
                ?? throw new VendorNotFoundException("Vendor not found");

            // Check if email is already in use
            var userExists = await _userManager.FindByEmailAsync(request.Email) != null;
            if (userExists)
            {
                throw new UserAlreadyExistsException(email: request.Email);
            }

            // Create new user
            var user = User.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber
            );

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new RegistrationFailedException(result.Errors.Select(x => x.Description));
            }

            // Assign VendorStaff role (Driver is now a separate Role assigned during user registration)
            await _userManager.AddToRoleAsync(user, IdentityRoleConstant.VendorStaff);

            // Create VendorStaff assignment
            var vendorStaff = new VendorStaff
            {
                UserId = user.Id,
                VendorId = vendorId,
                AssignedDate = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _vendorStaffRepository.CreateAsync(vendorStaff);

            // Fetch with includes
            var staffWithDetails = await _vendorStaffRepository.GetByIdAsync(created.Id);
            return await MapToResponseAsync(staffWithDetails!);
        }

        public async Task<VendorStaffResponse> UpdateAsync(int id, UpdateVendorStaffRequest request)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var staff = await _vendorStaffRepository.GetByIdAsync(id)
                ?? throw new VendorStaffNotFoundException(id);

            var user = staff.User;
            var hasUserChanges = false;

            // Update user properties if provided
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                user.FirstName = request.FirstName;
                hasUserChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                user.LastName = request.LastName;
                hasUserChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                // Check if new email is already in use
                var emailExists = await _userManager.FindByEmailAsync(request.Email);
                if (emailExists != null && emailExists.Id != user.Id)
                    throw new UserAlreadyExistsException(request.Email);

                user.Email = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.UserName = request.Email;
                user.NormalizedUserName = request.Email.ToUpper();
                hasUserChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.PhoneNumber = request.PhoneNumber;
                hasUserChanges = true;
            }

            // Update user if there are changes
            if (hasUserChanges)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new UserUpdateFailedException(updateResult.Errors.Select(e => e.Description));
                }
            }

            // Update staff properties
            if (request.IsActive.HasValue)
            {
                staff.IsActive = request.IsActive.Value;
                await _vendorStaffRepository.UpdateAsync(staff);
            }

            // Fetch updated staff with all includes
            var updated = await _vendorStaffRepository.GetByIdAsync(id);
            return await MapToResponseAsync(updated!);
        }

        public async Task DeactivateAsync(int id)
        {
            var staff = await _vendorStaffRepository.GetByIdAsync(id)
                ?? throw new VendorStaffNotFoundException(id);

            staff.IsActive = false;
            await _vendorStaffRepository.UpdateAsync(staff);
        }

        public async Task DeleteAsync(int id)
        {
            var staff = await _vendorStaffRepository.GetByIdAsync(id)
                ?? throw new VendorStaffNotFoundException(id);

            // Remove vendor staff roles from user
            var user = await _userRepository.GetByIdAsync(staff.UserId);
            if (user != null)
            {
                // Delete user's profile image if exists
                await _accountService.DeleteUserImageAsync(staff.UserId);

                var vendorRoles = new[] { IdentityRoleConstant.VendorStaff };
                var userRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = userRoles.Intersect(vendorRoles).ToList();

                if (rolesToRemove.Count > 0)
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
            }

            await _vendorStaffRepository.DeleteAsync(id);
        }

        public async Task<int?> GetVendorIdForStaffUserAsync(Guid userId)
        {
            var staff = await _vendorStaffRepository.GetByUserIdAsync(userId);
            return staff?.VendorId;
        }

        private async Task<VendorStaffResponse> MapToResponseAsync(VendorStaff staff)
        {
            var user = staff.User;
            var roles = await _userManager.GetRolesAsync(user);

            // Map identity role to display name
            var role = roles.Contains(IdentityRoleConstant.VendorStaff)
                ? "Staff"
                : roles.FirstOrDefault() ?? "Unknown";

            return new VendorStaffResponse
            {
                Id = staff.Id,
                UserId = staff.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                VendorId = staff.VendorId,
                VendorName = staff.Vendor?.Name ?? string.Empty,
                Role = role,
                AssignedDate = staff.AssignedDate,
                IsActive = staff.IsActive
            };
        }
    }
}