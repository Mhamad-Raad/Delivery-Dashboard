using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.CategoryRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves a specific category by ID including its subcategories. Returns category details with hierarchical structure. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        [HttpGet]
        [AllowAnonymous]
        [EndpointDescription("Retrieves all categories in a flat list. Returns the complete category hierarchy. Useful for building category trees or navigation menus. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("top-level")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves only top-level categories (categories without a parent). Returns root categories for building hierarchical navigation. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetTopLevelCategories()
        {
            var categories = await _categoryService.GetTopLevelCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("subcategories/{parentCategoryId}")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves all direct subcategories of a specific parent category. Returns child categories for building dynamic category dropdowns or filters. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetSubCategories(int parentCategoryId)
        {
            var categories = await _categoryService.GetSubCategoriesAsync(parentCategoryId);
            return Ok(categories);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves a paginated list of categories with optional filtering by name and parent category. Supports pagination for admin interfaces or large category lists. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetCategoriesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? searchName = null,
            [FromQuery] int? parentCategoryId = null)
        {
            var categories = await _categoryService.GetCategoriesPagedAsync(
                page, limit, searchName, parentCategoryId);
            return Ok(categories);
        }

        [HttpPost]
        [EndpointDescription("Creates a new category with the provided details. Accepts category name and optional parent category ID for creating subcategories. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            var category = await _categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [EndpointDescription("Updates an existing category's details by ID. Allows changing the category name and parent category relationship. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _categoryService.UpdateCategoryAsync(id, request);
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [EndpointDescription("Permanently deletes a category by ID. This action also removes all subcategories and may affect associated products. Cannot be undone. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}