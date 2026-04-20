using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.CategoryRequests;
using DeliveryDash.Domain.Exceptions.VendorExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ICurrentUserService _currentUserService;

        public CategoryController(
            ICategoryService categoryService,
            ICurrentUserService currentUserService)
        {
            _categoryService = categoryService;
            _currentUserService = currentUserService;
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Vendor, VendorStaff")]
        [EndpointDescription("Returns the calling vendor's (or vendor staff's) own product categories.")]
        public async Task<IActionResult> GetMine()
        {
            var vendorId = await ResolveVendorContextAsync();
            var categories = await _categoryService.GetMineAsync(vendorId);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Vendor, VendorStaff")]
        [EndpointDescription("Returns a single product category. Vendors/VendorStaff can only read categories belonging to their own vendor.")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendorId = await ResolveVendorContextAsync();
            var category = await _categoryService.GetByIdAsync(id, vendorId);
            return Ok(category);
        }

        [HttpGet("by-vendor/{vendorId:int}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        [EndpointDescription("Admin read of a vendor's product categories (includes product counts). Used by the vendor-details page.")]
        public async Task<IActionResult> GetByVendorForAdmin(int vendorId)
        {
            var categories = await _categoryService.GetByVendorIdAsync(vendorId);
            return Ok(categories);
        }

        [HttpGet("by-vendor/{vendorId:int}/public")]
        [AllowAnonymous]
        [EndpointDescription("Public read of a vendor's product categories. Used by the customer app to render menu sections while browsing a shop.")]
        public async Task<IActionResult> GetByVendorPublic(int vendorId)
        {
            var categories = await _categoryService.GetByVendorIdAsync(vendorId);
            return Ok(categories);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "SuperAdmin, Admin, Vendor, VendorStaff")]
        [EndpointDescription("Paged list of categories. Admins may pass ?vendorId= to filter. Vendors/VendorStaff are auto-scoped to their own vendor.")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int? vendorId = null,
            [FromQuery] string? searchName = null)
        {
            var callerVendorId = await _currentUserService.GetCurrentVendorIdAsync()
                ?? await _currentUserService.GetCurrentStaffVendorIdAsync();

            if (callerVendorId.HasValue)
            {
                vendorId = callerVendorId;
            }

            var result = await _categoryService.GetPagedAsync(page, limit, vendorId, searchName);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Creates a new product category owned by the calling vendor. VendorStaff are not allowed.")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var vendorId = await ResolveVendorOwnerIdAsync();
            var category = await _categoryService.CreateAsync(vendorId, request);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Updates a product category. Vendor can only update categories they own.")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            var vendorId = await ResolveVendorOwnerIdAsync();
            var category = await _categoryService.UpdateAsync(id, vendorId, request);
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Deletes a product category. Products under it have their CategoryId set to null (uncategorized).")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendorId = await ResolveVendorOwnerIdAsync();
            await _categoryService.DeleteAsync(id, vendorId);
            return NoContent();
        }

        private async Task<int> ResolveVendorContextAsync()
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync()
                ?? await _currentUserService.GetCurrentStaffVendorIdAsync();

            if (!vendorId.HasValue)
                throw new VendorNotFoundException("No vendor context found for the current user.");

            return vendorId.Value;
        }

        private async Task<int> ResolveVendorOwnerIdAsync()
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();
            if (!vendorId.HasValue)
                throw new VendorNotFoundException("No vendor found for the current user.");

            return vendorId.Value;
        }
    }
}
