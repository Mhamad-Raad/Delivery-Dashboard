using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderTrackingController : ControllerBase
    {
        private readonly IDriverLocationService _locationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderAssignmentRepository _assignmentRepository;
        private readonly ICurrentUserService _currentUserService;

        public OrderTrackingController(
            IDriverLocationService locationService,
            IOrderRepository orderRepository,
            IOrderAssignmentRepository assignmentRepository,
            ICurrentUserService currentUserService)
        {
            _locationService = locationService;
            _orderRepository = orderRepository;
            _assignmentRepository = assignmentRepository;
            _currentUserService = currentUserService;
        }

        [HttpGet("{orderId:int}/driver-location")]
        [EndpointDescription("Returns the last known live location of the driver assigned to this order. 404 if no location has been reported yet. Authorized to the order's customer, its assigned driver, or admins.")]
        public async Task<IActionResult> GetDriverLocation(int orderId, CancellationToken ct)
        {
            var callerId = _currentUserService.GetCurrentUserId();
            var isAdmin = User.IsInRole(IdentityRoleConstant.SuperAdmin)
                || User.IsInRole(IdentityRoleConstant.Admin);

            if (!isAdmin)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order is null)
                    return NotFound();

                var isCustomer = order.UserId == callerId;
                var isAssignedDriver = User.IsInRole(IdentityRoleConstant.Driver)
                    && await _assignmentRepository.GetAcceptedAssignmentAsync(orderId, callerId, ct) is not null;

                if (!isCustomer && !isAssignedDriver)
                    return Forbid();
            }

            var location = await _locationService.GetAsync(orderId, ct);
            return location is null ? NotFound() : Ok(location);
        }
    }
}
