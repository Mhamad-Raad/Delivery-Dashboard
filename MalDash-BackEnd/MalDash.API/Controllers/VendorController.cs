using MalDash.API.Extensions;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Requests.VendorRequests;
using MalDash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MalDash.API.Controllers
{
    [Route("MalDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Admin, Vendor")]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDashboardService _dashboardService;

        public VendorController(
            IVendorService vendorService,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService,
            IDashboardService dashboardService)
        {
            _vendorService = vendorService;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
            _dashboardService = dashboardService;
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Retrieves dashboard statistics for the authenticated vendor, including total orders, pending orders, completed orders, total products, revenue metrics, and daily order/profit data for the last 30 days.")]
        public async Task<IActionResult> GetVendorDashboard()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var vendor = await _vendorService.GetVendorByUserIdAsync(userId);

            if (vendor == null)
                return NotFound(new { message = "No vendor found for this user" });

            var dashboard = await _dashboardService.GetVendorDashboardAsync(vendor.Id);

            return Ok(dashboard);
        }

        [HttpGet("{id}")]
        [EndpointDescription("Retrieves a vendor's detailed information by their unique identifier. Returns vendor profile including name, description, opening/closing times, type, and associated user details.")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            return Ok(vendor);
        }

        [HttpGet("user/{userId}")]
        [EndpointDescription("Retrieves a vendor's detailed information by the associated user's unique identifier. Useful for fetching vendor data when only the user ID is known.")]
        public async Task<IActionResult> GetVendorByUserId(Guid userId)
        {
            var vendor = await _vendorService.GetVendorByUserIdAsync(userId);
            if (vendor == null)
                return NotFound(new { message = "No vendor found for this user" });

            return Ok(vendor);
        }

        [HttpGet]
        [EndpointDescription("Retrieves a paginated list of all vendors with optional filtering by name search and vendor type. Supports pagination with customizable page size.")]
        public async Task<IActionResult> GetAllVendors(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? searchName = null,
            [FromQuery] VendorType? type = null)
        {
            var vendors = await _vendorService.GetAllVendorsAsync(page, limit, searchName, type);
            return Ok(vendors);
        }

        [HttpGet("tenant")]
        [AllowAnonymous]
        [EndpointDescription("Public endpoint for tenants to browse available vendors. Returns a paginated list of vendors with optional filtering by name and type. No authentication required.")]
        public async Task<IActionResult> GetAllVendorsTenant(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? searchName = null,
            [FromQuery] VendorType? type = null)
        {
            var vendors = await _vendorService.GetAllVendorsAsync(page, limit, searchName, type);
            return Ok(vendors);
        }

        [HttpGet("me")]
        [EndpointDescription("Retrieves the vendor profile for the currently authenticated user. Returns the vendor's complete profile including business details and contact information.")]
        public async Task<IActionResult> GetVendorProfile()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var vendor = await _vendorService.GetVendorByUserIdAsync(userId);
            if (vendor == null)
                return NotFound(new { message = "No vendor found for this user" });

            return Ok(vendor);
        }

        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Creates a new vendor with the provided details. Accepts multipart form data with vendor information and an optional profile image (max 5MB). The user must have the Vendor role assigned.")]
        public async Task<IActionResult> CreateVendor([FromForm] CreateVendorRequest request, IFormFile? ProfileImageUrl)
        {
            // Upload image if provided
            string? imageUrl = null;
            var imageUpload = ProfileImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "vendors",
                    Request.GetBaseUrl());
            }

            var vendor = await _vendorService.CreateVendorAsync(request, imageUrl);
            return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Updates an existing vendor's information by ID. Accepts multipart form data with updated vendor details and an optional new profile image (max 5MB). Previous image is automatically deleted when replaced.")]
        public async Task<IActionResult> UpdateVendor(int id, [FromForm] UpdateVendorRequest request, IFormFile? ProfileImageUrl)
        {
            var existingVendor = await _vendorService.GetVendorByIdAsync(id);

            // Handle image replacement if provided
            string? imageUrl = null;
            var imageUpload = ProfileImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingVendor.ProfileImageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(existingVendor.ProfileImageUrl);
                }

                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "vendors",
                    Request.GetBaseUrl());
            }

            var vendor = await _vendorService.UpdateVendorAsync(id, request, imageUrl);
            return Ok(vendor);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Vendor")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Allows authenticated vendors to update their own profile. Accepts multipart form data with updated details and an optional new profile image (max 5MB). Vendors can only modify their own profile.")]
        public async Task<IActionResult> UpdateVendorProfile([FromForm] UpdateVendorRequest request, IFormFile? ProfileImageUrl)
        {
            var userId = _currentUserService.GetCurrentUserId();

            // Get the vendor associated with the current user
            var vendor = await _vendorService.GetVendorByUserIdAsync(userId);
            if (vendor == null)
                return NotFound(new { message = "No vendor found for this user" });

            // Handle image replacement if provided
            string? imageUrl = null;
            var imageUpload = ProfileImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(vendor.ProfileImageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(vendor.ProfileImageUrl);
                }

                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "vendors",
                    Request.GetBaseUrl());
            }

            var updatedVendor = await _vendorService.UpdateVendorAsync(vendor.Id, request, imageUrl);
            return Ok(updatedVendor);
        }

        [HttpDelete("{id}")]
        [EndpointDescription("Permanently deletes a vendor by ID. This action also removes the vendor's profile image from storage. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            await _vendorService.DeleteVendorAsync(id);
            return NoContent();
        }
    }
}
