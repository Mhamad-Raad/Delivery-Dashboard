using DeliveryDash.Application.Abstracts;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Responses.OrderResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeliveryDash.Application.Services
{
    public class OrderDispatchSettings
    {
        public int OfferTimeoutSeconds { get; set; } = 30;
        public int MaxDispatchAttempts { get; set; } = 10;
    }

    public class OrderDispatchService : IOrderDispatchService
    {
        private readonly IOrderAssignmentRepository _assignmentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IDriverQueueService _queueService;
        private readonly IDriverShiftService _shiftService;
        private readonly IDriverNotificationService _notificationService;
        private readonly INotificationService _userNotificationService;
        private readonly INotificationHubService _notificationHubService;
        private readonly IDriverLocationService _driverLocationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrderDispatchSettings _settings;
        private readonly ILogger<OrderDispatchService> _logger;

        public OrderDispatchService(
            IOrderAssignmentRepository assignmentRepository,
            IOrderRepository orderRepository,
            IVendorRepository vendorRepository,
            IDriverQueueService queueService,
            IDriverShiftService shiftService,
            IDriverNotificationService notificationService,
            INotificationService userNotificationService,
            INotificationHubService notificationHubService,
            IDriverLocationService driverLocationService,
            IUnitOfWork unitOfWork,
            IOptions<OrderDispatchSettings> settings,
            ILogger<OrderDispatchService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _orderRepository = orderRepository;
            _vendorRepository = vendorRepository;
            _queueService = queueService;
            _shiftService = shiftService;
            _notificationService = notificationService;
            _userNotificationService = userNotificationService;
            _notificationHubService = notificationHubService;
            _driverLocationService = driverLocationService;
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<OrderAssignmentResponse?> DispatchOrderAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new InvalidOperationException($"Order {orderId} not found");

            // Check for existing pending assignment
            var existingAssignment = await _assignmentRepository.GetPendingAssignmentForOrderAsync(orderId, ct);
            if (existingAssignment != null)
            {
                _logger.LogWarning("Order {OrderId} already has a pending assignment", orderId);
                return MapToResponse(existingAssignment);
            }

            // Check dispatch attempt count
            var attemptCount = await _assignmentRepository.GetAssignmentCountForOrderAsync(orderId, ct);
            if (attemptCount >= _settings.MaxDispatchAttempts)
            {
                _logger.LogWarning(
                    "Order {OrderId} exceeded max dispatch attempts ({MaxAttempts}), cancelling order",
                    orderId, _settings.MaxDispatchAttempts);

                await CancelOrderDueToNoDriverAsync(order, ct);
                return null;
            }

            // Get next available driver from queue
            var driverEntry = await _queueService.DequeueDriverAsync(ct);
            if (driverEntry == null)
            {
                _logger.LogWarning("No drivers available for order {OrderId}", orderId);
                return null;
            }

            // Verify driver is still on shift
            if (!await _shiftService.IsDriverOnShiftAsync(driverEntry.DriverId, ct))
            {
                _logger.LogWarning("Driver {DriverId} is no longer on shift, retrying dispatch", driverEntry.DriverId);
                return await DispatchOrderAsync(orderId, ct); // Recursive retry
            }

            // Create assignment
            var assignment = new OrderAssignment
            {
                OrderId = orderId,
                DriverId = driverEntry.DriverId,
                ShiftId = driverEntry.ShiftId,
                OfferedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(_settings.OfferTimeoutSeconds),
                Status = OrderAssignmentStatus.Pending
            };

            var createdAssignment = await _assignmentRepository.CreateAsync(assignment, ct);

            // Notify driver via SignalR
            await _notificationService.SendOrderOfferAsync(
                driverEntry.DriverId,
                createdAssignment.Id,
                order,
                _settings.OfferTimeoutSeconds,
                ct);

            _logger.LogInformation(
                "Order {OrderId} dispatched to driver {DriverId}, assignment {AssignmentId} (attempt {AttemptNumber}/{MaxAttempts})",
                orderId, driverEntry.DriverId, createdAssignment.Id, attemptCount + 1, _settings.MaxDispatchAttempts);

            return MapToResponse(createdAssignment);
        }

        private async Task CancelOrderDueToNoDriverAsync(Order order, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var previousStatus = order.Status;

            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = now;
            order.CompletedAt = now;
            await _orderRepository.UpdateAsync(order);

            await _userNotificationService.SendNotificationAsync(
                order.UserId,
                "Order Cancelled",
                $"Your order #{order.OrderNumber} has been cancelled because no delivery driver was available. We apologize for the inconvenience.",
                "Order",
                $"/orders/{order.Id}",
                $"{{\"orderId\":{order.Id},\"orderNumber\":\"{order.OrderNumber}\",\"reason\":\"NoDriverAvailable\"}}");

            var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);
            if (vendor != null)
            {
                await _userNotificationService.SendNotificationAsync(
                    vendor.UserId,
                    "Order Cancelled - No Driver",
                    $"Order #{order.OrderNumber} has been cancelled because no delivery driver accepted the order after {_settings.MaxDispatchAttempts} attempts.",
                    "Order",
                    $"/orders/{order.Id}",
                    $"{{\"orderId\":{order.Id},\"orderNumber\":\"{order.OrderNumber}\",\"reason\":\"NoDriverAvailable\"}}");
            }

            await tx.CommitAsync(ct);

            if (vendor != null)
            {
                await _notificationHubService.BroadcastOrderStatusAsync(
                    new OrderStatusUpdatedPayload(
                        order.Id, order.OrderNumber, OrderStatus.Cancelled, previousStatus, now, "System"),
                    order.UserId,
                    vendor.UserId);
            }

            _logger.LogInformation(
                "Order {OrderId} cancelled due to no driver available after {MaxAttempts} attempts",
                order.Id, _settings.MaxDispatchAttempts);
        }

        public async Task<OrderAssignmentResponse> AcceptOrderAsync(
            int assignmentId, Guid driverId, CancellationToken ct = default)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, ct)
                ?? throw new InvalidOperationException("Assignment not found");

            if (assignment.DriverId != driverId)
                throw new UnauthorizedAccessException("Driver not authorized for this assignment");

            if (assignment.Status != OrderAssignmentStatus.Pending)
                throw new InvalidOperationException($"Assignment is not pending (status: {assignment.Status})");

            if (DateTime.UtcNow > assignment.ExpiresAt)
                throw new InvalidOperationException("Assignment has expired");

            var now = DateTime.UtcNow;

            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);

            assignment.Status = OrderAssignmentStatus.Accepted;
            assignment.RespondedAt = now;
            var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment, ct);

            OrderStatus? previousStatus = null;
            var order = await _orderRepository.GetByIdAsync(assignment.OrderId);
            if (order != null)
            {
                previousStatus = order.Status;
                order.Status = OrderStatus.OutForDelivery;
                order.OutForDeliveryAt = now;
                await _orderRepository.UpdateAsync(order);
            }

            await tx.CommitAsync(ct);

            if (order != null)
            {
                var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);
                if (vendor != null)
                {
                    await _notificationHubService.BroadcastOrderStatusAsync(
                        new OrderStatusUpdatedPayload(
                            order.Id, order.OrderNumber, OrderStatus.OutForDelivery, previousStatus, now, "Driver"),
                        order.UserId,
                        vendor.UserId);
                }
            }

            _logger.LogInformation(
                "Driver {DriverId} accepted order {OrderId}",
                driverId, assignment.OrderId);

            return MapToResponse(updatedAssignment);
        }

        public async Task<OrderAssignmentResponse> RejectOrderAsync(
            int assignmentId, Guid driverId, CancellationToken ct = default)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, ct)
                ?? throw new InvalidOperationException("Assignment not found");

            if (assignment.DriverId != driverId)
                throw new UnauthorizedAccessException("Driver not authorized for this assignment");

            if (assignment.Status != OrderAssignmentStatus.Pending)
                throw new InvalidOperationException($"Assignment is not pending (status: {assignment.Status})");

            OrderAssignment updatedAssignment;
            await using (var tx = await _unitOfWork.BeginTransactionAsync(ct))
            {
                assignment.Status = OrderAssignmentStatus.Rejected;
                assignment.RespondedAt = DateTime.UtcNow;
                updatedAssignment = await _assignmentRepository.UpdateAsync(assignment, ct);

                if (assignment.ShiftId.HasValue)
                {
                    await _queueService.RequeueDriverAsync(driverId, assignment.ShiftId.Value, ct);
                }

                await tx.CommitAsync(ct);
            }

            // Re-dispatch after the rejection is committed so the next assignment is written in its own transaction.
            await DispatchOrderAsync(assignment.OrderId, ct);

            _logger.LogInformation(
                "Driver {DriverId} rejected order {OrderId}, requeued and redispatching",
                driverId, assignment.OrderId);

            return MapToResponse(updatedAssignment);
        }

        public async Task CompleteDeliveryAsync(int orderId, Guid driverId, CancellationToken ct = default)
        {
            var assignment = await _assignmentRepository.GetAcceptedAssignmentAsync(orderId, driverId, ct)
                ?? throw new InvalidOperationException("No accepted assignment found for this order");

            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new InvalidOperationException("Order not found");

            var now = DateTime.UtcNow;
            var previousStatus = order.Status;

            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);

            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = now;
            order.CompletedAt = now;
            await _orderRepository.UpdateAsync(order);

            if (await _shiftService.IsDriverOnShiftAsync(driverId, ct) && assignment.ShiftId.HasValue)
            {
                await _queueService.EnqueueDriverAsync(driverId, assignment.ShiftId.Value, ct);
            }

            await tx.CommitAsync(ct);

            await _driverLocationService.ClearAsync(orderId, ct);

            var vendor = await _vendorRepository.GetByIdAsync(order.VendorId);
            if (vendor != null)
            {
                await _notificationHubService.BroadcastOrderStatusAsync(
                    new OrderStatusUpdatedPayload(
                        order.Id, order.OrderNumber, OrderStatus.Delivered, previousStatus, now, "Driver"),
                    order.UserId,
                    vendor.UserId);
            }

            _logger.LogInformation(
                "Driver {DriverId} completed delivery for order {OrderId}",
                driverId, orderId);
        }

        public async Task ProcessExpiredAssignmentsAsync(CancellationToken ct = default)
        {
            var expiredAssignments = await _assignmentRepository.GetExpiredAssignmentsAsync(ct);

            foreach (var assignment in expiredAssignments)
            {
                try
                {
                    await using (var tx = await _unitOfWork.BeginTransactionAsync(ct))
                    {
                        assignment.Status = OrderAssignmentStatus.Expired;
                        assignment.RespondedAt = DateTime.UtcNow;
                        await _assignmentRepository.UpdateAsync(assignment, ct);

                        if (assignment.ShiftId.HasValue &&
                            await _shiftService.IsDriverOnShiftAsync(assignment.DriverId, ct))
                        {
                            await _queueService.RequeueDriverAsync(
                                assignment.DriverId, assignment.ShiftId.Value, ct);
                        }

                        await tx.CommitAsync(ct);
                    }

                    // Redispatch runs outside the expire-commit so the next assignment gets its own transaction.
                    await DispatchOrderAsync(assignment.OrderId, ct);

                    _logger.LogInformation(
                        "Processed expired assignment {AssignmentId} for order {OrderId}",
                        assignment.Id, assignment.OrderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expired assignment {AssignmentId}", assignment.Id);
                }
            }
        }

        private static OrderAssignmentResponse MapToResponse(OrderAssignment assignment)
        {
            return new OrderAssignmentResponse
            {
                Id = assignment.Id,
                OrderId = assignment.OrderId,
                DriverId = assignment.DriverId,
                OfferedAt = assignment.OfferedAt,
                ExpiresAt = assignment.ExpiresAt,
                RespondedAt = assignment.RespondedAt,
                Status = assignment.Status
            };
        }
    }
}