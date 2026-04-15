using FluentValidation;
using MalDash.Application.Abstracts.IRepository;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Extensions;
using MalDash.Application.Requests.VendorRequests;
using MalDash.Application.Responses.Common;
using MalDash.Application.Responses.VendorResponses;
using MalDash.Domain.Constants;
using MalDash.Domain.Entities;
using MalDash.Domain.Enums;
using MalDash.Domain.Exceptions.UserExceptions;
using MalDash.Domain.Exceptions.VendorExceptions;
using Microsoft.AspNetCore.Identity;

namespace MalDash.Application.Services
{
    public class VendorService : IVendorService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IValidator<CreateVendorRequest> _createValidator;
        private readonly IValidator<UpdateVendorRequest> _updateValidator;
        private readonly IFileStorageService _fileStorageService;

        public VendorService(
            IVendorRepository vendorRepository,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IValidator<CreateVendorRequest> createValidator,
            IValidator<UpdateVendorRequest> updateValidator,
            IFileStorageService fileStorageService)
        {
            _vendorRepository = vendorRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _fileStorageService = fileStorageService;
        }

        public async Task<VendorDetailResponse> GetVendorByIdAsync(int id)
        {
            var vendor = await _vendorRepository.GetByIdAsync(id);

            if (vendor == null)
                throw new VendorNotFoundException("Vendor not found");

            return MapToDetailResponse(vendor);
        }

        public async Task<VendorDetailResponse?> GetVendorByUserIdAsync(Guid userId)
        {
            var vendor = await _vendorRepository.GetByUserIdAsync(userId);

            return vendor == null ? null : MapToDetailResponse(vendor);
        }

        public async Task<PagedResponse<VendorResponse>> GetAllVendorsAsync(
            int page = 1,
            int limit = 10,
            string? searchName = null,
            VendorType? type = null)
        {
            var (vendors, total) = await _vendorRepository.GetVendorsPagedAsync(page, limit, searchName, type);

            var vendorResponses = vendors.Select(MapToResponse).ToList();

            return new PagedResponse<VendorResponse>
            {
                Data = vendorResponses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<VendorDetailResponse> CreateVendorAsync(CreateVendorRequest request, string? imageUrl = null)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            // Check if vendor name already exists
            if (await _vendorRepository.ExistsByNameAsync(request.Name))
                throw new DuplicateVendorNameException(request.Name);

            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new UserNotFoundException();

            // Check if user has the Vendor role
            var isVendor = await _userManager.IsInRoleAsync(user, IdentityRoleConstant.Vendor);
            if (!isVendor)
                throw new UserNotVendorRoleException(request.UserId);

            // Check if user already has a vendor
            if (await _vendorRepository.UserHasVendorAsync(request.UserId))
                throw new UserAlreadyHasVendorException(request.UserId);

            var vendor = new Vendor
            {
                Name = request.Name,
                Description = request.Description,
                OpeningTime = request.OpeningTime,
                CloseTime = request.CloseTime,
                Type = request.Type,
                UserId = request.UserId,
                ProfileImageUrl = imageUrl
            };

            var createdVendor = await _vendorRepository.CreateAsync(vendor);

            // Fetch the vendor with user details
            var vendorWithDetails = await _vendorRepository.GetByIdAsync(createdVendor.Id);
            return MapToDetailResponse(vendorWithDetails!);
        }

        public async Task<VendorDetailResponse> UpdateVendorAsync(int id, UpdateVendorRequest request, string? imageUrl = null)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var vendor = await _vendorRepository.GetByIdAsync(id);
            if (vendor == null)
                throw new VendorNotFoundException("Vendor not found");

            // Check if new name conflicts with existing vendor
            if (!string.IsNullOrWhiteSpace(request.Name) &&
                request.Name != vendor.Name &&
                await _vendorRepository.ExistsByNameAsync(request.Name))
            {
                throw new DuplicateVendorNameException(request.Name);
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                vendor.Name = request.Name;

            if (request.Description != null)
                vendor.Description = request.Description;

            if (request.OpeningTime.HasValue)
                vendor.OpeningTime = request.OpeningTime.Value;

            if (request.CloseTime.HasValue)
                vendor.CloseTime = request.CloseTime.Value;

            if (request.Type.HasValue)
                vendor.Type = request.Type.Value;

            // Update image URL if provided
            if (!string.IsNullOrEmpty(imageUrl))
                vendor.ProfileImageUrl = imageUrl;

            await _vendorRepository.UpdateAsync(vendor);

            var updatedVendor = await _vendorRepository.GetByIdAsync(id);
            return MapToDetailResponse(updatedVendor!);
        }

        public async Task DeleteVendorAsync(int id)
        {
            var vendor = await _vendorRepository.GetByIdAsync(id);
            if (vendor == null)
                throw new VendorNotFoundException("Vendor not found");

            // Delete vendor image if exists
            if (!string.IsNullOrEmpty(vendor.ProfileImageUrl))
            {
                await _fileStorageService.DeleteImageAsync(vendor.ProfileImageUrl);
            }

            await _vendorRepository.DeleteAsync(id);
        }

        private static VendorResponse MapToResponse(Vendor vendor)
        {
            return new VendorResponse
            {
                Id = vendor.Id,
                Name = vendor.Name,
                ProfileImageUrl = vendor.ProfileImageUrl,
                OpeningTime = vendor.OpeningTime,
                CloseTime = vendor.CloseTime,
                Type = vendor.Type.ToString(),
                FirstName = vendor.User?.FirstName ?? string.Empty,
                LastName = vendor.User?.LastName ?? string.Empty,
                Phone = vendor.User?.PhoneNumber ?? string.Empty,
                Email = vendor.User?.Email ?? string.Empty
            };
        }

        private static VendorDetailResponse MapToDetailResponse(Vendor vendor)
        {
            return new VendorDetailResponse
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
                FirstName = vendor.User?.FirstName ?? string.Empty,
                LastName = vendor.User?.LastName ?? string.Empty,
                Phone = vendor.User?.PhoneNumber ?? string.Empty,
                Email = vendor.User?.Email ?? string.Empty
            };
        }
    }
}