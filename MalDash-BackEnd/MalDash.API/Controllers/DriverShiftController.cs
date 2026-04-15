using MalDash.Application.Abstracts.IService;
using MalDash.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MalDash.API.Controllers
{
    [Route("MalDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = IdentityRoleConstant.Driver)]
    public class DriverShiftController : ControllerBase
    {
        private readonly IDriverShiftService _shiftService;
        private readonly IDriverQueueService _queueService;
        private readonly ICurrentUserService _currentUserService;

        public DriverShiftController(
            IDriverShiftService shiftService,
            IDriverQueueService queueService,
            ICurrentUserService currentUserService)
        {
            _shiftService = shiftService;
            _queueService = queueService;
            _currentUserService = currentUserService;
        }

        [HttpPost("start")]
        [EndpointDescription("Starts a new shift for the authenticated driver. Creates a shift record with start timestamp and adds the driver to the delivery assignment queue. Driver becomes eligible to receive order assignments. Restricted to Driver role.")]
        public async Task<IActionResult> StartShift(CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            var shift = await _shiftService.StartShiftAsync(driverId, ct);
            return Ok(shift);
        }

        [HttpPost("end")]
        [EndpointDescription("Ends the current active shift for the authenticated driver. Records the end timestamp, removes the driver from the assignment queue, and prevents new order assignments. Restricted to Driver role.")]
        public async Task<IActionResult> EndShift(CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            var shift = await _shiftService.EndShiftAsync(driverId, ct);
            return Ok(shift);
        }

        [HttpGet("current")]
        [EndpointDescription("Retrieves the currently active shift for the authenticated driver. Returns shift details including start time and duration. Returns 404 if no active shift exists. Restricted to Driver role.")]
        public async Task<IActionResult> GetCurrentShift(CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            var shift = await _shiftService.GetActiveShiftAsync(driverId, ct);

            if (shift == null)
            {
                return NotFound(new { Message = "No active shift found" });
            }

            return Ok(shift);
        }

        [HttpGet("queue-position")]
        [EndpointDescription("Retrieves the driver's current position in the order assignment queue. Returns the driver's position, total queue length, and queue status. Used to show drivers their place in line for receiving order assignments. Restricted to Driver role.")]
        public async Task<IActionResult> GetQueuePosition(CancellationToken ct)
        {
            var driverId = _currentUserService.GetCurrentUserId();
            var position = await _queueService.GetDriverPositionAsync(driverId, ct);
            var queueLength = await _queueService.GetQueueLengthAsync(ct);

            return Ok(new
            {
                Position = position,
                TotalInQueue = queueLength,
                IsInQueue = position.HasValue
            });
        }
    }
}