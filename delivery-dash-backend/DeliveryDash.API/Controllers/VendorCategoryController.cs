using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.VendorCategoryRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    public class VendorCategoryController : ControllerBase
    {
        private readonly IVendorCategoryService _service;

        public VendorCategoryController(IVendorCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        [EndpointDescription("Lists all vendor categories. Public — used by customer app to browse shops by category. Optionally filter to active ones only.")]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var items = await _service.GetAllAsync(activeOnly);
            return Ok(items);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [EndpointDescription("Paginated list of vendor categories with optional search and active-only filter.")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? searchName = null,
            [FromQuery] bool activeOnly = false)
        {
            var result = await _service.GetPagedAsync(page, limit, searchName, activeOnly);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves a single vendor category by id.")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        [EndpointDescription("Creates a new vendor category. Restricted to SuperAdmin/Admin.")]
        public async Task<IActionResult> Create([FromBody] CreateVendorCategoryRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        [EndpointDescription("Updates an existing vendor category. Restricted to SuperAdmin/Admin.")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorCategoryRequest request)
        {
            var updated = await _service.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        [EndpointDescription("Deletes a vendor category. Blocked with 409 if any vendors still reference it.")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
