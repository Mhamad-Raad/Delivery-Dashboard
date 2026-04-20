using FluentValidation;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.VendorCategoryRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.VendorCategoryResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Exceptions.VendorCategoryExceptions;

namespace DeliveryDash.Application.Services
{
    public class VendorCategoryService : IVendorCategoryService
    {
        private readonly IVendorCategoryRepository _repository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IValidator<CreateVendorCategoryRequest> _createValidator;
        private readonly IValidator<UpdateVendorCategoryRequest> _updateValidator;

        public VendorCategoryService(
            IVendorCategoryRepository repository,
            IVendorRepository vendorRepository,
            IValidator<CreateVendorCategoryRequest> createValidator,
            IValidator<UpdateVendorCategoryRequest> updateValidator)
        {
            _repository = repository;
            _vendorRepository = vendorRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<VendorCategoryResponse> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new VendorCategoryNotFoundException(id);

            var vendorsCount = await _vendorRepository.CountByVendorCategoryIdAsync(id);
            return MapToResponse(entity, vendorsCount);
        }

        public async Task<IEnumerable<VendorCategoryResponse>> GetAllAsync(bool activeOnly = false)
        {
            var items = (await _repository.GetAllAsync(activeOnly)).ToList();
            if (items.Count == 0)
                return Enumerable.Empty<VendorCategoryResponse>();

            var counts = await _repository.CountVendorsByIdsAsync(items.Select(i => i.Id));
            return items.Select(i => MapToResponse(i, counts.GetValueOrDefault(i.Id))).ToList();
        }

        public async Task<PagedResponse<VendorCategoryResponse>> GetPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            bool activeOnly = false)
        {
            var (items, total) = await _repository.GetPagedAsync(page, limit, searchName, activeOnly);
            var itemList = items.ToList();

            var counts = itemList.Count > 0
                ? await _repository.CountVendorsByIdsAsync(itemList.Select(i => i.Id))
                : new Dictionary<int, int>();

            return new PagedResponse<VendorCategoryResponse>
            {
                Data = itemList.Select(i => MapToResponse(i, counts.GetValueOrDefault(i.Id))).ToList(),
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<VendorCategoryResponse> CreateAsync(CreateVendorCategoryRequest request)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            var name = request.Name.Trim();
            if (await _repository.ExistsByNameAsync(name))
                throw new DuplicateVendorCategoryNameException(name);

            var now = DateTime.UtcNow;
            var entity = new VendorCategory
            {
                Name = name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _repository.CreateAsync(entity);
            return MapToResponse(created, 0);
        }

        public async Task<VendorCategoryResponse> UpdateAsync(int id, UpdateVendorCategoryRequest request)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new VendorCategoryNotFoundException(id);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var newName = request.Name.Trim();
                if (!string.Equals(newName, entity.Name, StringComparison.OrdinalIgnoreCase) &&
                    await _repository.ExistsByNameAsync(newName, id))
                {
                    throw new DuplicateVendorCategoryNameException(newName);
                }
                entity.Name = newName;
            }

            if (request.Description != null)
                entity.Description = request.Description;

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);

            var vendorsCount = await _vendorRepository.CountByVendorCategoryIdAsync(id);
            return MapToResponse(entity, vendorsCount);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new VendorCategoryNotFoundException(id);

            var vendorsCount = await _vendorRepository.CountByVendorCategoryIdAsync(id);
            if (vendorsCount > 0)
                throw new VendorCategoryInUseException(id, vendorsCount);

            await _repository.DeleteAsync(id);
        }

        private static VendorCategoryResponse MapToResponse(VendorCategory entity, int vendorsCount)
        {
            return new VendorCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                VendorsCount = vendorsCount,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
