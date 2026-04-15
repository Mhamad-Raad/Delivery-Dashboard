using MalDash.API.Extensions;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Requests.ProductRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MalDash.API.Controllers
{
    [Route("MalDashApi/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;

        public ProductController(
            IProductService productService,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService)
        {
            _productService = productService;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves detailed information for a specific product by ID. Returns product details including name, description, price, stock status, category, vendor information, and product image. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(product);
        }

        [HttpGet]
        [AllowAnonymous]
        [EndpointDescription("Retrieves a paginated list of all products with optional filtering. Supports filtering by vendor ID, category ID, name search, and stock availability. Returns product summaries with pricing and images. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int? vendorId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchName = null,
            [FromQuery] bool? inStock = null)
        {
            var products = await _productService.GetAllProductsAsync(
                page, limit, vendorId, categoryId, searchName, inStock);
            return Ok(products);
        }

        [HttpGet("tenant")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves a paginated list of products specifically for tenant browsing. Identical functionality to GetAllProducts with same filtering options. Provides a dedicated endpoint for customer-facing product catalogs. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetAllProductsTenants(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] int? vendorId = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchName = null,
            [FromQuery] bool? inStock = null)
        {
            var products = await _productService.GetAllProductsAsync(
                page, limit, vendorId, categoryId, searchName, inStock);
            return Ok(products);
        }

        [HttpGet("my-products/{productId}")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Retrieves a specific product belonging to the authenticated vendor. Returns product details only if the product belongs to the vendor's catalog. Used for vendor product management. Restricted to Vendor role.")]
        public async Task<IActionResult> GetMyProductById(int productId)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found. Please complete your vendor setup." });

            var product = await _productService.GetProductByIdAsync(productId);

            if (product.VendorId != vendorId.Value)
                return NotFound(new { message = "Product not found or you don't have permission to access it." });

            return Ok(product);
        }

        [HttpGet("my-products")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Retrieves a paginated list of products belonging to the authenticated vendor. Supports filtering by name search and stock availability. Used for vendor inventory management. Restricted to Vendor role.")]
        public async Task<IActionResult> GetMyProducts(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? searchName = null,
            [FromQuery] bool? inStock = null)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found. Please complete your vendor setup." });

            var products = await _productService.GetAllProductsAsync(
                page,
                limit,
                vendorId: vendorId.Value,
                categoryId: null,
                searchName: searchName,
                inStock: inStock);

            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        [EndpointDescription("Retrieves all products belonging to a specific category. Returns complete product list for the given category without pagination. Useful for category browsing pages. Public endpoint - no authentication required.")]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Creates a new product for the authenticated vendor. Accepts multipart form data with product details and an optional product image (max 5MB). Automatically associates the product with the vendor's catalog. Restricted to Vendor role.")]
        public async Task<IActionResult> CreateProduct(
            [FromForm] CreateProductRequest request,
            IFormFile? ProductImageUrl)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found. Please complete your vendor setup." });

            // Upload image if provided
            string? imageUrl = null;
            var imageUpload = ProductImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "products",
                    Request.GetBaseUrl());
            }

            var product = await _productService.CreateProductAsync(request, vendorId.Value, imageUrl);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Updates an existing product owned by the authenticated vendor. Accepts multipart form data with updated product details and an optional new product image (max 5MB). Previous image is automatically deleted when replaced. Vendors can only update their own products. Restricted to Vendor role.")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromForm] UpdateProductRequest request,
            IFormFile? ProductImageUrl)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found." });

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct.VendorId != vendorId.Value)
                return Forbid();

            // Handle image replacement if provided
            string? imageUrl = null;
            var imageUpload = ProductImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingProduct.ProductImageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(existingProduct.ProductImageUrl);
                }

                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "products",
                    Request.GetBaseUrl());
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, request, imageUrl);
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")]
        [EndpointDescription("Permanently deletes a product owned by the authenticated vendor. Removes the product and its associated image from storage. Vendors can only delete their own products. Cannot be undone. Restricted to Vendor role.")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var vendorId = await _currentUserService.GetCurrentVendorIdAsync();

            if (vendorId == null)
                return BadRequest(new { message = "Vendor profile not found." });

            var product = await _productService.GetProductByIdAsync(id);

            if (product.VendorId != vendorId.Value)
                return Forbid();

            await _productService.DeleteProductAsync(id);
            return Ok(new { message = "Product has successfully been deleted." });
        }

        [HttpPut("admin/{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Admin endpoint to update any product regardless of vendor ownership. Accepts multipart form data with updated product details and an optional new product image (max 5MB). Previous image is automatically deleted when replaced. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> AdminUpdateProduct(
            int id,
            [FromForm] UpdateProductRequest request,
            IFormFile? ProductImageUrl)
        {
            var existingProduct = await _productService.GetProductByIdAsync(id);

            // Handle image replacement if provided
            string? imageUrl = null;
            var imageUpload = ProductImageUrl.ToImageUpload();
            if (imageUpload.HasValue)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingProduct.ProductImageUrl))
                {
                    await _fileStorageService.DeleteImageAsync(existingProduct.ProductImageUrl);
                }

                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "products",
                    Request.GetBaseUrl());
            }

            var product = await _productService.UpdateProductAsync(id, request, imageUrl);
            return Ok(product);
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        [EndpointDescription("Admin endpoint to permanently delete any product regardless of vendor ownership. Removes the product and its associated image from storage. Cannot be undone. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> AdminDeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new { message = "Product has successfully been deleted." });
        }
    }
}