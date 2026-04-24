using System.Security.Claims;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Responses.TrackingResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryDash.API.Hubs
{
    [Authorize]
    public class TrackingHub : Hub
    {
        private readonly IDriverLocationService _locationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderAssignmentRepository _assignmentRepository;
        private readonly ILogger<TrackingHub> _logger;

        public TrackingHub(
            IDriverLocationService locationService,
            IOrderRepository orderRepository,
            IOrderAssignmentRepository assignmentRepository,
            ILogger<TrackingHub> logger)
        {
            _locationService = locationService;
            _orderRepository = orderRepository;
            _assignmentRepository = assignmentRepository;
            _logger = logger;
        }

        public async Task SubscribeToOrder(int orderId)
        {
            if (!await IsAuthorizedForOrderAsync(orderId))
                throw new HubException("Not authorized for this order.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(orderId));
        }

        public async Task UnsubscribeFromOrder(int orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(orderId));
        }

        public async Task PushLocation(int orderId, double lat, double lng, double? heading)
        {
            if (!IsInRole("Driver"))
                throw new HubException("Only drivers can push location.");

            var driverId = GetCallerUserId();
            if (driverId == Guid.Empty)
                throw new HubException("Invalid driver context.");

            var assignment = await _assignmentRepository
                .GetAcceptedAssignmentAsync(orderId, driverId, Context.ConnectionAborted);
            if (assignment is null)
                throw new HubException("No accepted assignment for this order.");

            await _locationService.SetAsync(orderId, lat, lng, heading, Context.ConnectionAborted);

            var payload = new DriverLocationResponse
            {
                OrderId = orderId,
                Lat = lat,
                Lng = lng,
                Heading = heading,
                RecordedAt = DateTime.UtcNow,
            };
            await Clients.Group(GroupName(orderId)).SendAsync("DriverLocationUpdated", payload);
        }

        private async Task<bool> IsAuthorizedForOrderAsync(int orderId)
        {
            if (IsInRole("SuperAdmin") || IsInRole("Admin"))
                return true;

            var callerId = GetCallerUserId();
            if (callerId == Guid.Empty)
                return false;

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null)
                return false;

            if (order.UserId == callerId)
                return true;

            if (IsInRole("Driver"))
            {
                var assignment = await _assignmentRepository
                    .GetAcceptedAssignmentAsync(orderId, callerId, Context.ConnectionAborted);
                if (assignment is not null)
                    return true;
            }

            return false;
        }

        private Guid GetCallerUserId()
        {
            var raw = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }

        private bool IsInRole(string role) => Context.User?.IsInRole(role) ?? false;

        private static string GroupName(int orderId) => $"order:{orderId}";
    }
}
