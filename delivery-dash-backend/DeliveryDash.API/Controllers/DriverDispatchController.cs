using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.DriverRequests;
using DeliveryDash.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = IdentityRoleConstant.Driver)]
    public class DriverDispatchController : ControllerBase
    {
        private readonly IOrderDispatchService _dispatchService;
        private readonly ICurrentUserService _currentUserService;

        public DriverDispatchController(
            IOrderDispatchService dispatchService,
            ICurrentUserService currentUserService)
        {
            _dispatchService = dispatchService;
            _currentUserService = currentUserService;
        }

        [HttpPost("accept")]
        [EndpointDescription("Allows a driver to accept an assigned order delivery. Updates the order assignment status to accepted and notifies relevant parties. Returns the updated assignment details. Restricted to Driver role.")]
        public async Task<IActionResult> AcceptOrder(
            [FromBody] OrderAssignmentRequest request,
            CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            var result = await _dispatchService.AcceptOrderAsync(request.AssignmentId, driverId, ct);
            return Ok(result);
        }

        [HttpPost("reject")]
        [EndpointDescription("Allows a driver to reject an assigned order delivery. Updates the order assignment status and may trigger reassignment to another driver. Returns the updated assignment details. Restricted to Driver role.")]
        public async Task<IActionResult> RejectOrder(
            [FromBody] OrderAssignmentRequest request,
            CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            var result = await _dispatchService.RejectOrderAsync(request.AssignmentId, driverId, ct);
            return Ok(result);
        }

        [HttpPost("complete/{orderId:int}")]
        [EndpointDescription("Marks an order delivery as completed by the driver. Updates the order status to delivered and records the completion timestamp. Triggers notifications to customer and vendor. Restricted to Driver role.")]
        public async Task<IActionResult> CompleteDelivery(int orderId, CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            await _dispatchService.CompleteDeliveryAsync(orderId, driverId, ct);
            return Ok(new { Message = "Delivery completed successfully" });
        }
    }
}