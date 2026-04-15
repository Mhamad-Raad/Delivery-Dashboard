using FluentValidation;
using MalDash.Application.Abstracts.IRepository;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Extensions;
using MalDash.Application.Requests.CategoryRequests;
using MalDash.Application.Responses.CategoryResponses;
using MalDash.Application.Responses.Common;
using MalDash.Domain.Entities;
using MalDash.Domain.Exceptions.CategoryExceptions;

namespace MalDash.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IValidator<CreateCategoryRequest> _createValidator;
        private readonly IValidator<UpdateCategoryRequest> _updateValidator;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IValidator<CreateCategoryRequest> createValidator,
            IValidator<UpdateCategoryRequest> updateValidator)
        {
            _categoryRepository = categoryRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<CategoryDetailResponse> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                throw new CategoryNotFoundException("Category not found");

            return MapToDetailResponse(category);
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return categories.Select(MapToResponse);
        }

        public async Task<IEnumerable<CategoryResponse>> GetTopLevelCategoriesAsync()
        {
            var categories = await _categoryRepository.GetTopLevelCategoriesAsync();
            return categories.Select(MapToResponse);
        }

        public async Task<IEnumerable<CategoryResponse>> GetSubCategoriesAsync(int parentCategoryId)
        {
            var categories = await _categoryRepository.GetSubCategoriesAsync(parentCategoryId);
            return categories.Select(MapToResponse);
        }

        public async Task<PagedResponse<CategoryResponse>> GetCategoriesPagedAsync(
            int page = 1,
            int limit = 10,
            string? searchName = null,
            int? parentCategoryId = null)
        {
            var (categories, total) = await _categoryRepository.GetCategoriesPagedAsync(
                page, limit, searchName, parentCategoryId);

            var categoryResponses = categories.Select(MapToResponse).ToList();

            return new PagedResponse<CategoryResponse>
            {
                Data = categoryResponses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<CategoryDetailResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            // Check if category name already exists
            if (await _categoryRepository.ExistsByNameAsync(request.Name))
                throw new DuplicateCategoryNameException(request.Name);

            // Check if parent category exists (if provided)
            if (request.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
                if (parentCategory == null)
                    throw new CategoryNotFoundException("Parent category not found");
            }

            var category = new Catagory
            {
                Name = request.Name,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId
            };

            var createdCategory = await _categoryRepository.CreateAsync(category);

            // Fetch the category with parent and subcategories
            var categoryWithDetails = await _categoryRepository.GetByIdAsync(createdCategory.Id);
            return MapToDetailResponse(categoryWithDetails!);
        }

        public async Task<CategoryDetailResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new CategoryNotFoundException("Category not found");

            // Check if new name conflicts with existing category
            if (!string.IsNullOrWhiteSpace(request.Name) &&
                request.Name != category.Name &&
                await _categoryRepository.ExistsByNameAsync(request.Name))
            {
                throw new DuplicateCategoryNameException(request.Name);
            }

            // Check if parent category exists (if provided)
            if (request.ParentCategoryId.HasValue)
            {
                // Prevent self-referencing
                if (request.ParentCategoryId == id)
                    throw new InvalidOperationException("A category cannot be its own parent");

                var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
                if (parentCategory == null)
                    throw new CategoryNotFoundException("Parent category not found");
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                category.Name = request.Name;

            if (request.Description != null)
                category.Description = request.Description;

            if (request.ParentCategoryId.HasValue)
                category.ParentCategoryId = request.ParentCategoryId;

            await _categoryRepository.UpdateAsync(category);

            var updatedCategory = await _categoryRepository.GetByIdAsync(id);
            return MapToDetailResponse(updatedCategory!);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new CategoryNotFoundException("Category not found");

            // Check if category has subcategories
            if (await _categoryRepository.HasSubCategoriesAsync(id))
                throw new CategoryHasSubCategoriesException(id);

            // Check if category has products
            if (await _categoryRepository.HasProductsAsync(id))
                throw new CategoryHasProductsException(id);

            await _categoryRepository.DeleteAsync(id);
        }

        private static CategoryResponse MapToResponse(Catagory category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategoriesCount = category.SubCategories?.Count ?? 0,
                ProductsCount = category.Products?.Count ?? 0
            };
        }

        private static CategoryDetailResponse MapToDetailResponse(Catagory category)
        {
            return new CategoryDetailResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategories = category.SubCategories?.Select(MapToResponse).ToList() ?? new List<CategoryResponse>(),
                ProductsCount = category.Products?.Count ?? 0
            };
        }
    }
}