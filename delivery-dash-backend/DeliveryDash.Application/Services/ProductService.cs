using FluentValidation;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.ProductRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.ProductResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Exceptions.ProductExceptions;
using DeliveryDash.Domain.Exceptions.VendorExceptions;
using DeliveryDash.Domain.Exceptions.CategoryExceptions;

namespace DeliveryDash.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IValidator<CreateProductRequest> _createValidator;
        private readonly IValidator<UpdateProductRequest> _updateValidator;
        private readonly IFileStorageService _fileStorageService;

        public ProductService(
            IProductRepository productRepository,
            IVendorRepository vendorRepository,
            ICategoryRepository categoryRepository,
            IValidator<CreateProductRequest> createValidator,
            IValidator<UpdateProductRequest> updateValidator,
            IFileStorageService fileStorageService)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _categoryRepository = categoryRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _fileStorageService = fileStorageService;
        }

        public async Task<ProductDetailResponse> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new ProductNotFoundException("Product not found");

            return MapToDetailResponse(product);
        }

        public async Task<PagedResponse<ProductResponse>> GetAllProductsAsync(
            int page = 1,
            int limit = 10,
            int? vendorId = null,
            int? categoryId = null,
            string? searchName = null,
            bool? inStock = null)
        {
            var (products, total) = await _productRepository.GetProductsPagedAsync(
                page, limit, vendorId, categoryId, searchName, inStock);

            var productResponses = products.Select(MapToResponse).ToList();

            return new PagedResponse<ProductResponse>
            {
                Data = productResponses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<IEnumerable<ProductDetailResponse>> GetProductsByVendorIdAsync(int vendorId)
        {
            var products = await _productRepository.GetProductsByVendorIdAsync(vendorId);
            return products.Select(MapToDetailResponse);
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
            return products.Select(MapToResponse);
        }

        public async Task<ProductDetailResponse> CreateProductAsync(
            CreateProductRequest request,
            int vendorId,
            string? imageUrl = null)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            // Check if vendor exists
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null)
                throw new VendorNotFoundException("Vendor not found");

            // Check if category exists and belongs to this vendor
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                    throw new CategoryNotFoundException("Category not found");
                if (category.VendorId != vendorId)
                    throw new CategoryVendorMismatchException();
            }

            var product = new Product
            {
                VendorId = vendorId,
                CategoryId = request.CategoryId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                InStock = request.InStock,
                IsWeightable = request.IsWeightable,
                ProductImageUrl = imageUrl ?? string.Empty
            };

            var createdProduct = await _productRepository.CreateAsync(product);

            // Fetch the product with vendor and category details
            var productWithDetails = await _productRepository.GetByIdAsync(createdProduct.Id);
            return MapToDetailResponse(productWithDetails!);
        }

        public async Task<ProductDetailResponse> UpdateProductAsync(
            int id,
            UpdateProductRequest request,
            string? imageUrl = null)
        {
            await _updateValidator.ValidateAndThrowCustomAsync(request);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ProductNotFoundException("Product not found");

            // Check if category exists and belongs to this product's vendor
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                    throw new CategoryNotFoundException("Category not found");
                if (category.VendorId != product.VendorId)
                    throw new CategoryVendorMismatchException();
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                product.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Description))
                product.Description = request.Description;

            if (request.CategoryId.HasValue)
                product.CategoryId = request.CategoryId;

            if (request.Price.HasValue)
                product.Price = request.Price.Value;

            if (request.DiscountPrice.HasValue)
                product.DiscountPrice = request.DiscountPrice;

            if (request.InStock.HasValue)
                product.InStock = request.InStock.Value;

            if (request.IsWeightable.HasValue)
                product.IsWeightable = request.IsWeightable.Value;

            // Update image URL if provided
            if (!string.IsNullOrEmpty(imageUrl))
                product.ProductImageUrl = imageUrl;

            await _productRepository.UpdateAsync(product);

            var updatedProduct = await _productRepository.GetByIdAsync(id);
            return MapToDetailResponse(updatedProduct!);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ProductNotFoundException("Product not found");

            // Delete product image if exists
            if (!string.IsNullOrEmpty(product.ProductImageUrl))
            {
                await _fileStorageService.DeleteImageAsync(product.ProductImageUrl);
            }

            await _productRepository.DeleteAsync(id);
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                InStock = product.InStock,
                IsWeightable = product.IsWeightable,
                ProductImageUrl = product.ProductImageUrl,
                VendorId = product.VendorId,
                VendorName = product.Vendor?.Name ?? string.Empty,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };
        }

        private static ProductDetailResponse MapToDetailResponse(Product product)
        {
            return new ProductDetailResponse
            {
                Id = product.Id,
                VendorId = product.VendorId,
                VendorName = product.Vendor?.Name ?? string.Empty,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                InStock = product.InStock,
                IsWeightable = product.IsWeightable,
                ProductImageUrl = product.ProductImageUrl
            };
        }
    }
}