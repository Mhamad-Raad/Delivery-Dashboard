using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.OrderRequests;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICurrentUserService _currentUserService;

        public OrderController(
            IOrderService orderService,
            UserManager<User> userManager,
            IVendorRepository vendorRepository,
            ICurrentUserService currentUserService)
        {
            _orderService = orderService;
            _userManager = userManager;
            _vendorRepository = vendorRepository;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [EndpointDescription("Creates a new order for the authenticated tenant. Accepts order details including vendor ID, products with quantities, delivery address, and optional notes. Calculates subtotal, delivery fee, and total amount. Triggers order notifications to the vendor. Restricted to Customer role.")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var order = await _orderService.CreateOrderAsync(request, userId);
            return Ok(new { message = "Order created successfully", order });
        }

        [HttpGet("{id}")]
        [EndpointDescription("Retrieves detailed information for a specific order by ID. Returns order details including items, pricing, status, delivery address, and customer information. Access is role-based: Customers see only their orders, Vendors see their vendor's orders, Admins see all orders. Requires authentication.")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var (userRole, vendorId) = await GetUserRoleAndVendorIdAsync();

            var order = await _orderService.GetOrderByIdAsync(id, userId, userRole, vendorId);
            return Ok(order);
        }

        [HttpGet("number/{orderNumber}")]
        [EndpointDescription("Retrieves detailed information for a specific order by its order number. Returns complete order details including items, status, and delivery information. Useful for order tracking by customers. Access is role-based with same restrictions as GetOrderById. Requires authentication.")]
        public async Task<IActionResult> GetOrderByOrderNumber(string orderNumber)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var (userRole, vendorId) = await GetUserRoleAndVendorIdAsync();

            var order = await _orderService.GetOrderByOrderNumberAsync(orderNumber, userId, userRole, vendorId);
            return Ok(order);
        }

        [HttpGet]
        [EndpointDescription("Retrieves a paginated list of orders with optional filtering by status, vendorId, and date. Returns different order sets based on user role: Customers see their orders, Vendors see their vendor's orders, Admins/SuperAdmins see all orders and can filter by vendorId. Supports pagination and filtering by order status (Pending, Confirmed, Preparing, etc.), date range (fromDate/toDate), or specific date. The vendorId filter is only available to SuperAdmin and Admin roles. Requires authentication.")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] int? vendorId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] DateTime? date = null)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var (userRole, currentVendorId) = await GetUserRoleAndVendorIdAsync();

            // Only SuperAdmin and Admin can filter by vendorId
            int? effectiveVendorId = currentVendorId;
            if (userRole == Role.SuperAdmin || userRole == Role.Admin)
            {
                // Admins can optionally filter by vendorId or see all orders
                effectiveVendorId = vendorId;
            }
            else if (vendorId.HasValue && vendorId.Value != currentVendorId)
            {
                // Non-admin users trying to access other vendor's orders
                return Forbid();
            }

            var orders = await _orderService.GetOrdersAsync(
                page, limit, userId, userRole, effectiveVendorId, status, fromDate, toDate, date);
            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "SuperAdmin,Admin,Vendor")]
        [EndpointDescription("Updates the status of an existing order. Allows vendors to move orders through workflow stages (Pending -> Confirmed -> Preparing -> Ready -> Out for Delivery -> Delivered). Sends status update notifications to customers. Vendors can only update their own orders. Restricted to SuperAdmin, Admin, and Vendor roles.")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var (userRole, vendorId) = await GetUserRoleAndVendorIdAsync();

            var order = await _orderService.UpdateOrderStatusAsync(id, request, userId, userRole, vendorId);
            return Ok(new { message = "Order status updated successfully", order });
        }

        [HttpPost("{id}/cancel")]
        [EndpointDescription("Cancels an existing order. Customers can cancel their own orders before they are confirmed. Admins can cancel any order. Updates order status to Cancelled and sends cancellation notifications to relevant parties. Requires authentication.")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var (userRole, _) = await GetUserRoleAndVendorIdAsync();

            var order = await _orderService.CancelOrderAsync(id, userId, userRole);
            return Ok(new { message = "Order cancelled successfully", order });
        }

        private async Task<(Role role, int? vendorId)> GetUserRoleAndVendorIdAsync()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? throw new InvalidOperationException("User must have a role");

            var role = roleName switch
            {
                "SuperAdmin" => Role.SuperAdmin,
                "Admin" => Role.Admin,
                "Vendor" => Role.Vendor,
                "Customer" => Role.Customer,
                "Driver" => Role.Driver,
                _ => throw new InvalidOperationException($"Unknown role: {roleName}")
            };

            int? vendorId = null;
            if (role == Role.Vendor)
            {
                var vendor = await _vendorRepository.GetByUserIdAsync(userId);
                vendorId = vendor?.Id;
            }

            return (role, vendorId);
        }
    }
}