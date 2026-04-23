using FluentValidation;
using DeliveryDash.Application.Abstracts;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Extensions;
using DeliveryDash.Application.Requests.OrderRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.OrderResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using DeliveryDash.Domain.Exceptions.OrderExceptions;
using Microsoft.Extensions.Logging;

namespace DeliveryDash.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly INotificationService _notificationService;
        private readonly INotificationHubService _notificationHubService;
        private readonly IOrderDispatchService _orderDispatchService;
        private readonly IOrderAssignmentRepository _orderAssignmentRepository;
        private readonly IValidator<CreateOrderRequest> _createValidator;
        private readonly IValidator<UpdateOrderStatusRequest> _updateStatusValidator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IVendorRepository vendorRepository,
            IAddressRepository addressRepository,
            INotificationService notificationService,
            INotificationHubService notificationHubService,
            IOrderDispatchService orderDispatchService,
            IOrderAssignmentRepository orderAssignmentRepository,
            IValidator<CreateOrderRequest> createValidator,
            IValidator<UpdateOrderStatusRequest> updateStatusValidator,
            IUnitOfWork unitOfWork,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _addressRepository = addressRepository;
            _notificationService = notificationService;
            _notificationHubService = notificationHubService;
            _orderDispatchService = orderDispatchService;
            _orderAssignmentRepository = orderAssignmentRepository;
            _createValidator = createValidator;
            _updateStatusValidator = updateStatusValidator;
            _unitOfWork = unitOfWork;
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

            // If order has no delivery address, fall back to the user's default address
            if (order.DeliveryAddress == null)
            {
                var userAddresses = await _addressRepository.GetByUserIdAsync(order.UserId);
                var fallbackAddress = userAddresses.FirstOrDefault();
                if (fallbackAddress != null)
                {
                    order.DeliveryAddress = fallbackAddress;
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
            if (userRole == Role.Customer)
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

            if (userRole == Role.Customer)
                throw new UnauthorizedOrderAccessException();

            if (userRole == Role.Vendor)
            {
                if (!vendorId.HasValue || order.VendorId != vendorId.Value)
                    throw new UnauthorizedOrderAccessException();
            }

            ValidateStatusTransition(order.Status, request.Status, userRole);

            var previousStatus = order.Status;
            var now = DateTime.UtcNow;

            await using var tx = await _unitOfWork.BeginTransactionAsync();

            order.Status = request.Status;
            ApplyStatusTimestamp(order, request.Status, now);

            var updatedOrder = await _orderRepository.UpdateAsync(order);

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

            OrderAssignmentResponse? dispatchAssignment = null;
            if (request.Status == OrderStatus.Preparing)
            {
                _logger.LogInformation("Dispatching order {OrderId} to driver", order.Id);
                dispatchAssignment = await _orderDispatchService.DispatchOrderAsync(order.Id);
            }

            await tx.CommitAsync();

            _logger.LogInformation(
                "Order {OrderId} status updated from {PreviousStatus} to {NewStatus} by role {Role}",
                order.Id, previousStatus, request.Status, userRole);

            if (request.Status == OrderStatus.Preparing && dispatchAssignment == null)
            {
                _logger.LogWarning("Order {OrderId} is Preparing but no driver could be dispatched yet", order.Id);
            }

            var vendorForBroadcast = await _vendorRepository.GetByIdAsync(order.VendorId);
            if (vendorForBroadcast != null)
            {
                await _notificationHubService.BroadcastOrderStatusAsync(
                    new OrderStatusUpdatedPayload(
                        order.Id, order.OrderNumber, order.Status, previousStatus, now, userRole.ToString()),
                    order.UserId,
                    vendorForBroadcast.UserId);
            }

            return MapToDetailResponse(updatedOrder);
        }

        public async Task<OrderDetailResponse> CancelOrderAsync(int id, Guid userId, Role userRole)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new OrderNotFoundException(id);

            if (userRole == Role.Customer && order.UserId != userId)
                throw new UnauthorizedOrderAccessException();

            // Strict: customer/vendor can only cancel Pending or Confirmed.
            // Admin/SuperAdmin can cancel any non-terminal state.
            var isAdmin = userRole == Role.Admin || userRole == Role.SuperAdmin;
            var isTerminal = order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled;

            if (isTerminal)
                throw new InvalidOrderStatusTransitionException(order.Status, OrderStatus.Cancelled);

            if (!isAdmin && order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                throw new InvalidOrderStatusTransitionException(order.Status, OrderStatus.Cancelled);

            var now = DateTime.UtcNow;
            var previousStatus = order.Status;

            await using var tx = await _unitOfWork.BeginTransactionAsync();

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = now;
            order.CompletedAt = now;

            var updatedOrder = await _orderRepository.UpdateAsync(order);

            var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);
            if (vendor != null)
            {
                await _notificationService.SendNotificationAsync(
                    vendor.UserId,
                    "Order Cancelled",
                    $"Order #{order.OrderNumber} worth {order.TotalAmount:C} has been cancelled.",
                    "Order",
                    $"/orders/{order.Id}",
                    $"{{\"orderId\":{order.Id},\"orderNumber\":\"{order.OrderNumber}\",\"totalAmount\":{order.TotalAmount},\"status\":\"Cancelled\"}}");
            }

            await tx.CommitAsync();

            if (vendor != null)
            {
                await _notificationHubService.BroadcastOrderStatusAsync(
                    new OrderStatusUpdatedPayload(
                        order.Id, order.OrderNumber, OrderStatus.Cancelled, previousStatus, now, userRole.ToString()),
                    order.UserId,
                    vendor.UserId);
            }

            return MapToDetailResponse(updatedOrder);
        }

        private static void ApplyStatusTimestamp(Order order, OrderStatus newStatus, DateTime now)
        {
            switch (newStatus)
            {
                case OrderStatus.Confirmed:
                    order.ConfirmedAt = now;
                    break;
                case OrderStatus.Preparing:
                    order.PreparingAt = now;
                    break;
                case OrderStatus.OutForDelivery:
                    order.OutForDeliveryAt = now;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredAt = now;
                    order.CompletedAt = now;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledAt = now;
                    order.CompletedAt = now;
                    break;
            }
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

            if (userRole == Role.Customer && order.UserId != userId)
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

        private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus, Role userRole)
        {
            var isAdmin = userRole == Role.Admin || userRole == Role.SuperAdmin;

            // Domain-allowed transitions (shape of the state machine).
            var allowedTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
                { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.OutForDelivery, OrderStatus.Cancelled } },
                { OrderStatus.OutForDelivery, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } },
                { OrderStatus.Delivered, new List<OrderStatus>() },
                { OrderStatus.Cancelled, new List<OrderStatus>() }
            };

            if (!allowedTransitions[currentStatus].Contains(newStatus))
                throw new InvalidOrderStatusTransitionException(currentStatus, newStatus);

            // Strict policy: non-admins cannot cancel once the order is Preparing or later.
            if (newStatus == OrderStatus.Cancelled && !isAdmin &&
                currentStatus != OrderStatus.Pending && currentStatus != OrderStatus.Confirmed)
            {
                throw new InvalidOrderStatusTransitionException(currentStatus, newStatus);
            }
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
                ConfirmedAt = order.ConfirmedAt,
                PreparingAt = order.PreparingAt,
                OutForDeliveryAt = order.OutForDeliveryAt,
                DeliveredAt = order.DeliveredAt,
                CancelledAt = order.CancelledAt,
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
                    Type = order.DeliveryAddress.Type,
                    Latitude = order.DeliveryAddress.Latitude,
                    Longitude = order.DeliveryAddress.Longitude,
                    Street = order.DeliveryAddress.Street,
                    PhoneNumber = order.DeliveryAddress.PhoneNumber,
                    BuildingName = order.DeliveryAddress.BuildingName,
                    Floor = order.DeliveryAddress.Floor,
                    ApartmentNumber = order.DeliveryAddress.ApartmentNumber,
                    HouseName = order.DeliveryAddress.HouseName,
                    HouseNumber = order.DeliveryAddress.HouseNumber,
                    CompanyName = order.DeliveryAddress.CompanyName,
                    AdditionalDirections = order.DeliveryAddress.AdditionalDirections,
                    Label = order.DeliveryAddress.Label
                } : null,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                ConfirmedAt = order.ConfirmedAt,
                PreparingAt = order.PreparingAt,
                OutForDeliveryAt = order.OutForDeliveryAt,
                DeliveredAt = order.DeliveredAt,
                CancelledAt = order.CancelledAt,
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