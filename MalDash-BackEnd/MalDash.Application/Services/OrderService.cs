using FluentValidation;
using MalDash.Application.Abstracts.IRepository;
using MalDash.Application.Abstracts.IService;
using MalDash.Application.Extensions;
using MalDash.Application.Requests.OrderRequests;
using MalDash.Application.Responses.Common;
using MalDash.Application.Responses.OrderResponses;
using MalDash.Domain.Entities;
using MalDash.Domain.Enums;
using MalDash.Domain.Exceptions.OrderExceptions;
using Microsoft.Extensions.Logging;

namespace MalDash.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly INotificationService _notificationService;
        private readonly IOrderDispatchService _orderDispatchService;
        private readonly IOrderAssignmentRepository _orderAssignmentRepository;
        private readonly IValidator<CreateOrderRequest> _createValidator;
        private readonly IValidator<UpdateOrderStatusRequest> _updateStatusValidator;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IVendorRepository vendorRepository,
            IAddressRepository addressRepository,
            INotificationService notificationService,
            IOrderDispatchService orderDispatchService,
            IOrderAssignmentRepository orderAssignmentRepository,
            IValidator<CreateOrderRequest> createValidator,
            IValidator<UpdateOrderStatusRequest> updateStatusValidator,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _addressRepository = addressRepository;
            _notificationService = notificationService;
            _orderDispatchService = orderDispatchService;
            _orderAssignmentRepository = orderAssignmentRepository;
            _createValidator = createValidator;
            _updateStatusValidator = updateStatusValidator;
            _logger = logger;
        }

        public async Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request, Guid userId)
        {
            await _createValidator.ValidateAndThrowCustomAsync(request);

            // Validate vendor exists
            var vendor = await _vendorRepository.GetByIdAsync(request.VendorId);
            if (vendor == null)
                throw new InvalidOperationException($"Vendor with ID {request.VendorId} not found");

            // Validate address if provided
            if (request.AddressId.HasValue)
            {
                var address = await _addressRepository.GetByIdAsync(request.AddressId.Value);
                if (address == null || address.UserId != userId)
                    throw new InvalidOperationException("Invalid delivery address");
            }

            // Validate and fetch products
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = new List<Product>();

            foreach (var productId in productIds)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {productId} not found");

                if (!product.InStock)
                    throw new ProductNotAvailableException(product.Name);

                if (product.VendorId != request.VendorId)
                    throw new InvalidOperationException($"Product '{product.Name}' does not belong to the selected vendor");

                products.Add(product);
            }

            // Calculate totals
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                var unitPrice = product.DiscountPrice ?? product.Price;
                var totalPrice = unitPrice * item.Quantity;
                subtotal += totalPrice;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            var totalAmount = subtotal + request.DeliveryFee;

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                VendorId = request.VendorId,
                AddressId = request.AddressId,
                Subtotal = subtotal,
                DeliveryFee = request.DeliveryFee,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Notify vendor about new order
            await _notificationService.SendNotificationAsync(
                vendor.UserId,
                "New Order Received",
                $"You have a new order #{orderNumber} worth {totalAmount:C}. Please review and confirm.",
                "Order",
                $"/orders/{createdOrder.Id}",
                $"{{\"orderId\":{createdOrder.Id},\"orderNumber\":\"{orderNumber}\",\"totalAmount\":{totalAmount}}}");

            return MapToDetailResponse(createdOrder);
        }

        public async Task<OrderDetailResponse> GetOrderByIdAsync(int id, Guid userId, Role userRole, int? vendorId = null)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new OrderNotFoundException(id);

            await ValidateOrderAccessAsync(order, userId, userRole, vendorId);

            // If order has no delivery address, get the user's address
            if (order.DeliveryAddress == null)
            {
                var userAddress = await _addressRepository.GetByUserIdAsync(order.UserId);
                if (userAddress != null)
                {
                    order.DeliveryAddress = userAddress;
                }
            }

            return MapToDetailResponse(order);
        }

        public async Task<OrderDetailResponse> GetOrderByOrderNumberAsync(string orderNumber, Guid userId, Role userRole, int? vendorId = null)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null)
                throw new OrderNotFoundException(orderNumber);

            await ValidateOrderAccessAsync(order, userId, userRole, vendorId);

            return MapToDetailResponse(order);
        }

        public async Task<PagedResponse<OrderResponse>> GetOrdersAsync(
            int page,
            int limit,
            Guid userId,
            Role userRole,
            int? vendorId = null,
            OrderStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? date = null)
        {
            Guid? filterUserId = null;
            int? filterVendorId = null;
            IEnumerable<int>? filterOrderIds = null;

            // Apply role-based filtering
            if (userRole == Role.Tenant)
            {
                filterUserId = userId;
            }
            else if (userRole == Role.Vendor && vendorId.HasValue)
            {
                filterVendorId = vendorId.Value;
            }
            else if (userRole == Role.Driver)
            {
                // Drivers see only orders assigned to them
                filterOrderIds = await _orderAssignmentRepository.GetOrderIdsForDriverAsync(userId);
            }
            // SuperAdmin and Admin can see all orders (no filters)

            var (orders, total) = await _orderRepository.GetOrdersPagedAsync(
                page, limit, filterUserId, filterVendorId, status, filterOrderIds, fromDate, toDate, date);

            var orderResponses = orders.Select(MapToResponse).ToList();

            return new PagedResponse<OrderResponse>
            {
                Data = orderResponses,
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<OrderDetailResponse> UpdateOrderStatusAsync(
            int id,
            UpdateOrderStatusRequest request,
            Guid userId,
            Role userRole,
            int? vendorId = null)
        {
            await _updateStatusValidator.ValidateAndThrowCustomAsync(request);

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new OrderNotFoundException(id);

            // Only vendors and admins can update status
            if (userRole == Role.Tenant)
                throw new UnauthorizedOrderAccessException();

            if (userRole == Role.Vendor)
            {
                if (!vendorId.HasValue || order.VendorId != vendorId.Value)
                    throw new UnauthorizedOrderAccessException();
            }

            // Validate status transition
            ValidateStatusTransition(order.Status, request.Status);

            var previousStatus = order.Status;
            order.Status = request.Status;

            if (request.Status == OrderStatus.Delivered || request.Status == OrderStatus.Cancelled)
            {
                order.CompletedAt = DateTime.UtcNow;
            }

            var updatedOrder = await _orderRepository.UpdateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} status updated from {PreviousStatus} to {NewStatus}",
                order.Id, previousStatus, request.Status);

            // Notify customer when order is confirmed
            if (previousStatus == OrderStatus.Pending && request.Status == OrderStatus.Confirmed)
            {
                await _notificationService.SendNotificationAsync(
                    order.UserId,
                    "Order Confirmed",
                    $"Your order #{order.OrderNumber} has been confirmed and is being prepared.",
                    "Order",
                    $"/orders/{order.Id}",
                    $"{{\"orderId\":{order.Id},\"orderNumber\":\"{order.OrderNumber}\",\"status\":\"Confirmed\"}}");
            }

            // Dispatch to driver when order is ready for delivery
            if (request.Status == OrderStatus.Preparing)
            {
                _logger.LogInformation("Dispatching order {OrderId} to driver", order.Id);
                
                try
                {
                    var assignment = await _orderDispatchService.DispatchOrderAsync(order.Id);
                    
                    if (assignment != null)
                    {
                        _logger.LogInformation(
                            "Order {OrderId} dispatched successfully. Assignment: {AssignmentId}, Driver: {DriverId}",
                            order.Id, assignment.Id, assignment.DriverId);
                    }
                    else
                    {
                        _logger.LogWarning("Order {OrderId} dispatch returned null - no drivers available", order.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispatch order {OrderId} to driver", order.Id);
                }
            }

            return MapToDetailResponse(updatedOrder);
        }

        public async Task<OrderDetailResponse> CancelOrderAsync(int id, Guid userId, Role userRole)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new OrderNotFoundException(id);

            // Users can only cancel their own orders
            if (userRole == Role.Tenant && order.UserId != userId)
                throw new UnauthorizedOrderAccessException();

            // Can only cancel pending or confirmed orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                throw new InvalidOrderStatusTransitionException(order.Status, OrderStatus.Cancelled);

            order.Status = OrderStatus.Cancelled;
            order.CompletedAt = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateAsync(order);

            // Notify vendor about order cancellation
            var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);
            if (vendor != null)
            {
                await _notificationService.SendNotificationAsync(
                    vendor.UserId,
                    "Order Cancelled",
                    $"Order #{order.OrderNumber} worth {order.TotalAmount:C} has been cancelled by the customer.",
                    "Order",
                    $"/orders/{order.Id}",
                    $"{{\"orderId\":{order.Id},\"orderNumber\":\"{order.OrderNumber}\",\"totalAmount\":{order.TotalAmount},\"status\":\"Cancelled\"}}");
            }

            return MapToDetailResponse(updatedOrder);
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = "ORD";

            var todayOrderCount = await _orderRepository.GetTodayOrderCountAsync();
            var sequence = (todayOrderCount + 1).ToString("D4");

            return $"{prefix}-{date}-{sequence}";
        }

        private async Task ValidateOrderAccessAsync(Order order, Guid userId, Role userRole, int? vendorId)
        {
            if (userRole == Role.SuperAdmin || userRole == Role.Admin)
                return;

            if (userRole == Role.Tenant && order.UserId != userId)
                throw new UnauthorizedOrderAccessException();

            if (userRole == Role.Vendor)
            {
                if (!vendorId.HasValue || order.VendorId != vendorId.Value)
                    throw new UnauthorizedOrderAccessException();
            }

            if (userRole == Role.Driver)
            {
                // Driver can access orders that have been assigned to them (any status)
                var hasAssignment = await _orderAssignmentRepository.HasAssignmentForDriverAsync(order.Id, userId);
                if (!hasAssignment)
                    throw new UnauthorizedOrderAccessException();
            }
        }

        private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define allowed transitions
            var allowedTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
                { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.OutForDelivery, OrderStatus.Cancelled } },
                { OrderStatus.OutForDelivery, new List<OrderStatus> { OrderStatus.Delivered } },
                { OrderStatus.Delivered, new List<OrderStatus>() },
                { OrderStatus.Cancelled, new List<OrderStatus>() }
            };

            if (!allowedTransitions[currentStatus].Contains(newStatus))
                throw new InvalidOrderStatusTransitionException(currentStatus, newStatus);
        }

        private OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserName = $"{order.User.FirstName} {order.User.LastName}",
                VendorId = order.VendorId,
                VendorName = order.Vendor.Name,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                CompletedAt = order.CompletedAt,
                ItemCount = order.OrderItems.Count
            };
        }

        private OrderDetailResponse MapToDetailResponse(Order order)
        {
            return new OrderDetailResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserName = $"{order.User.FirstName} {order.User.LastName}",
                UserEmail = order.User.Email ?? string.Empty,
                UserPhone = order.User.PhoneNumber ?? string.Empty,
                VendorId = order.VendorId,
                VendorName = order.Vendor.Name,
                DeliveryAddress = order.DeliveryAddress != null ? new DeliveryAddressInfo
                {
                    Id = order.DeliveryAddress.Id,
                    BuildingName = order.DeliveryAddress.Building?.Name,
                    FloorNumber = order.DeliveryAddress.Floor?.FloorNumber,
                    ApartmentName = order.DeliveryAddress.Apartment?.ApartmentName
                } : null,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                CompletedAt = order.CompletedAt,
                Items = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ProductImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }
    }
}