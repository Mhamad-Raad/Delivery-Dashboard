using FluentValidation;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.CategoryRequests;
using DeliveryDash.Application.Responses.CategoryResponses;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Exceptions.CategoryExceptions;

namespace DeliveryDash.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private const int MaxCategoriesPerVendor = 50;

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

        public async Task<CategoryDetailResponse> GetByIdAsync(int id, int? requestingVendorId)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new CategoryNotFoundException($"Category with ID {id} was not found.");

            if (requestingVendorId.HasValue && category.VendorId != requestingVendorId.Value)
                throw new CategoryVendorMismatchException();

            var productsCount = await _categoryRepository.CountProductsAsync(id);
            return MapToDetailResponse(category, productsCount);
        }

        public async Task<IEnumerable<CategoryResponse>> GetByVendorIdAsync(int vendorId)
        {
            var categories = (await _categoryRepository.GetByVendorIdAsync(vendorId)).ToList();
            if (categories.Count == 0)
                return Enumerable.Empty<CategoryResponse>();

            var counts = await _categoryRepository.CountProductsByCategoryIdsAsync(categories.Select(c => c.Id));
            return categories.Select(c => MapToResponse(c, counts.GetValueOrDefault(c.Id))).ToList();
        }

        public Task<IEnumerable<CategoryResponse>> GetMineAsync(int vendorId) => GetByVendorIdAsync(vendorId);

        public async Task<PagedResponse<CategoryResponse>> GetPagedAsync(
            int page,
            int limit,
            int? vendorId,
            string? searchName)
        {
            var (categories, total) = await _categoryRepository.GetPagedAsync(page, limit, vendorId, searchName);
            var categoryList = categories.ToList();

            var counts = categoryList.Count > 0
                ? await _categoryRepository.CountProductsByCategoryIdsAsync(categoryList.Select(c => c.Id))
                : new Dictionary<int, int>();

            return new PagedResponse<CategoryResponse>
            {
                Data = categoryList.Select(c => MapToResponse(c, counts.GetValueOrDefault(c.Id))).ToList(),
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<CategoryDetailResponse> CreateAsync(int vendorId, CreateCategoryRequest request)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            var currentCount = await _categoryRepository.CountByVendorIdAsync(vendorId);
            if (currentCount >= MaxCategoriesPerVendor)
                throw new CategoryLimitReachedException(MaxCategoriesPerVendor);

            if (await _categoryRepository.ExistsByNameForVendorAsync(vendorId, request.Name))
                throw new DuplicateCategoryNameException(request.Name);

            var category = new Category
            {
                VendorId = vendorId,
                Name = request.Name.Trim(),
                Description = request.Description,
                SortOrder = request.SortOrder
            };

            var created = await _categoryRepository.CreateAsync(category);
            var reloaded = await _categoryRepository.GetByIdAsync(created.Id);
            return MapToDetailResponse(reloaded!, 0);
        }

        public async Task<CategoryDetailResponse> UpdateAsync(int id, int vendorId, UpdateCategoryRequest request)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new CategoryNotFoundException($"Category with ID {id} was not found.");

            if (category.VendorId != vendorId)
                throw new CategoryVendorMismatchException();

            if (!string.IsNullOrWhiteSpace(request.Name) &&
                !string.Equals(request.Name.Trim(), category.Name, StringComparison.OrdinalIgnoreCase) &&
                await _categoryRepository.ExistsByNameForVendorAsync(vendorId, request.Name.Trim()))
            {
                throw new DuplicateCategoryNameException(request.Name.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
                category.Name = request.Name.Trim();

            if (request.Description != null)
                category.Description = request.Description;

            if (request.SortOrder.HasValue)
                category.SortOrder = request.SortOrder;

            await _categoryRepository.UpdateAsync(category);

            var updated = await _categoryRepository.GetByIdAsync(id);
            var productsCount = await _categoryRepository.CountProductsAsync(id);
            return MapToDetailResponse(updated!, productsCount);
        }

        public async Task DeleteAsync(int id, int vendorId)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new CategoryNotFoundException($"Category with ID {id} was not found.");

            if (category.VendorId != vendorId)
                throw new CategoryVendorMismatchException();

            await _categoryRepository.DeleteAsync(id);
        }

        private static CategoryResponse MapToResponse(Category category, int productsCount)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                VendorId = category.VendorId,
                Name = category.Name,
                Description = category.Description,
                SortOrder = category.SortOrder,
                ProductsCount = productsCount
            };
        }

        private static CategoryDetailResponse MapToDetailResponse(Category category, int productsCount)
        {
            return new CategoryDetailResponse
            {
                Id = category.Id,
                VendorId = category.VendorId,
                VendorName = category.Vendor?.Name ?? string.Empty,
                Name = category.Name,
                Description = category.Description,
                SortOrder = category.SortOrder,
                ProductsCount = productsCount
            };
        }
    }
}
