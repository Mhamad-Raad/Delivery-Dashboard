    using MalDash.Application.Abstracts.IService;
using MalDash.Application.Requests.UserRequest;
using MalDash.Application.Requests.VendorStaffRequests;
using MalDash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MalDash.API.Controllers
{
    [Route("MalDashApi/[controller]")]
    [ApiController]
    public class VendorStaffController : ControllerBase
    {
        private readonly IVendorStaffService _vendorStaffService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccountService _accountService;

        public VendorStaffController(
            IVendorStaffService vendorStaffService,
            ICurrentUserService currentUserService,
            IAccountService accountService)
        {
            _vendorStaffService = vendorStaffService;
            _currentUserService = currentUserService;
            _accountService = accountService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Vendor,VendorStaff,VendorDriver")]
        [EndpointDescription("Retrieves detailed information for a specific staff member by ID. Returns staff profile including role, contact details, vendor association, and activity status. Staff members can only view their own profile or colleagues from the same vendor. Restricted to SuperAdmin, Admin, Vendor, VendorStaff, and VendorDriver roles.")]
        public async Task<IActionResult> GetById(int id)
        {
            // Staff can only view their own profile or colleagues from the same vendor
            if (User.IsInRole("VendorStaff") || User.IsInRole("VendorDriver"))
            {
                var staffVendorId = await _currentUserService.GetCurrentStaffVendorIdAsync();
                var requestedStaff = await _vendorStaffService.GetByIdAsync(id);

                if (staffVendorId == null || staffVendorId.Value != requestedStaff.VendorId)
                    return Forbid();
            }

            var staff = await _vendorStaffService.GetByIdAsync(id);
            return Ok(staff);
        }

        [HttpGet("me")]
        [Authorize(Roles = "VendorStaff,VendorDriver")]
        [EndpointDescription("Retrieves the staff profile for the currently authenticated staff member or driver. Returns complete profile including role, vendor information, and assigned date. Used for staff self-service profile viewing. Restricted to VendorStaff and VendorDriver roles.")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var staff = await _vendorStaffService.GetByUserIdAsync(userId);

            if (staff == null)
                return NotFound(new { message = "Staff profile not found." });

            return Ok(staff);
        }

        [HttpGet("vendor/{vendorId}")]
        [Authorize(Roles = "SuperAdmin,Admin,Vendor,VendorStaff,VendorDriver")]
        [EndpointDescription("Retrieves a paginated list of staff members for a specific vendor. Supports filtering by active status, search term, and role. Staff members can only view colleagues from their own vendor. Used for vendor staff management. Restricted to SuperAdmin, Admin, Vendor, VendorStaff, and VendorDriver roles.")]
        public async Task<IActionResult> GetStaffByVendorId(
            int vendorId,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] VendorStaffRole? role = null)
        {
            // Staff can only view colleagues from their own vendor
            if (User.IsInRole("VendorStaff") || User.IsInRole("VendorDriver"))
            {
                var staffVendorId = await _currentUserService.GetCurrentStaffVendorIdAsync();
                if (staffVendorId == null || staffVendorId.Value != vendorId)
                    return Forbid();
            }

            var staff = await _vendorStaffService.GetStaffByVendorIdAsync(
                vendorId, page, limit, isActive, searchTerm);
            return Ok(staff);
        }

        [HttpGet("my-staff")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Retrieves a paginated list of all staff members belonging to the authenticated vendor's business. Supports filtering by active status, search term, and role. Used by vendors to manage their team members. Restricted to Vendor role.")]
        public async Task<IActionResult> GetMyStaff(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] VendorStaffRole? role = null)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found." });

            var staff = await _vendorStaffService.GetStaffByVendorIdAsync(
                vendorId.Value, page, limit, isActive, searchTerm);
            return Ok(staff);
        }

        [HttpGet("my-colleagues")]
        [Authorize(Roles = "VendorStaff,VendorDriver")]
        [EndpointDescription("Retrieves a paginated list of colleagues working for the same vendor as the authenticated staff member. Supports filtering by active status, search term, and role. Used by staff members to view their team. Restricted to VendorStaff and VendorDriver roles.")]
        public async Task<IActionResult> GetMyColleagues(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] VendorStaffRole? role = null)
        {
            var vendorId = await _currentUserService.GetCurrentStaffVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Staff profile not found." });

            var staff = await _vendorStaffService.GetStaffByVendorIdAsync(
                vendorId.Value, page, limit, isActive, searchTerm);
            return Ok(staff);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Vendor")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Creates a new staff member for the authenticated vendor's business. Accepts multipart form data with staff details including role (Staff/Driver) and an optional profile image (max 5MB). Creates a user account and associates it with the vendor. Restricted to SuperAdmin, Admin, and Vendor roles.")]
        public async Task<IActionResult> VendorCreate([FromForm] CreateVendorStaffRequest request, IFormFile? ProfileImageUrl)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found." });

            var staff = await _vendorStaffService.CreateAsync(request, vendorId.Value);

            // If an image was provided, upload it
            if (ProfileImageUrl != null && ProfileImageUrl.Length > 0)
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var imageRequest = new UploadUserImageRequest
                {
                    ImageStream = ProfileImageUrl.OpenReadStream(),
                    FileName = ProfileImageUrl.FileName,
                    ContentType = ProfileImageUrl.ContentType,
                    FileSize = ProfileImageUrl.Length
                };

                await _accountService.UploadUserImageAsync(staff.UserId, imageRequest, baseUrl);
            }

            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
        }

        [HttpPost("admin/{vendorId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Admin endpoint to create a new staff member for any vendor. Accepts multipart form data with staff details and an optional profile image (max 5MB). Allows admins to manage staff across all vendors. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> AdminCreate(int vendorId, [FromForm] CreateVendorStaffRequest request, IFormFile? ProfileImageUrl)
        {
            var staff = await _vendorStaffService.CreateAsync(request, vendorId);

            // If an image was provided, upload it
            if (ProfileImageUrl != null && ProfileImageUrl.Length > 0)
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var imageRequest = new UploadUserImageRequest
                {
                    ImageStream = ProfileImageUrl.OpenReadStream(),
                    FileName = ProfileImageUrl.FileName,
                    ContentType = ProfileImageUrl.ContentType,
                    FileSize = ProfileImageUrl.Length
                };

                await _accountService.UploadUserImageAsync(staff.UserId, imageRequest, baseUrl);
            }

            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Vendor")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Updates an existing staff member's information. Accepts multipart form data with updated details and an optional new profile image (max 5MB). Previous image is automatically deleted when replaced. Vendors can only update their own staff members. Restricted to SuperAdmin, Admin, and Vendor roles.")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateVendorStaffRequest request, IFormFile? ProfileImageUrl)
        {
            // If the user is a Vendor, verify they own this staff member
            if (User.IsInRole("Vendor") && !User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            {
                var existingStaff = await _vendorStaffService.GetByIdAsync(id);
                var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

                if (vendorId == null || vendorId.Value != existingStaff.VendorId)
                    return Forbid();
            }

            var staff = await _vendorStaffService.UpdateAsync(id, request);

            // If an image was provided, delete old image and upload new one
            if (ProfileImageUrl != null && ProfileImageUrl.Length > 0)
            {
                // Delete the old image first
                await _accountService.DeleteUserImageAsync(staff.UserId);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var imageRequest = new UploadUserImageRequest
                {
                    ImageStream = ProfileImageUrl.OpenReadStream(),
                    FileName = ProfileImageUrl.FileName,
                    ContentType = ProfileImageUrl.ContentType,
                    FileSize = ProfileImageUrl.Length
                };

                var imageUrl = await _accountService.UploadUserImageAsync(staff.UserId, imageRequest, baseUrl);
                staff.ProfileImageUrl = imageUrl; // Update the response with the new image URL
            }

            return Ok(staff);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Vendor")]
        [EndpointDescription("Permanently deletes a staff member by ID. Removes the staff record and associated user account. Vendors can only delete their own staff members. Cannot be undone. Restricted to SuperAdmin, Admin, and Vendor roles.")]
        public async Task<IActionResult> Delete(int id)
        {
            // If the user is a Vendor, verify they own this staff member
            if (User.IsInRole("Vendor") && !User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            {
                var existingStaff = await _vendorStaffService.GetByIdAsync(id);
                var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

                if (vendorId == null || vendorId.Value != existingStaff.VendorId)
                    return Forbid();
            }

            await _vendorStaffService.DeleteAsync(id);
            return NoContent();
        }
    }
}