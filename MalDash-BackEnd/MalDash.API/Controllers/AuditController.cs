using MalDash.Application.Abstracts.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MalDash.API.Controllers
{
    [Route("MalDashApi/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        [EndpointDescription("Retrieves a paginated list of audit logs with optional filtering. Supports filtering by entity name, entity ID, user ID, action type, and date range. Useful for tracking all system changes and user activities. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? entityName = null,
            [FromQuery] string? entityId = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _auditService.GetAuditLogsAsync(
                page, limit, entityName, entityId, userId, action, fromDate, toDate);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [EndpointDescription("Retrieves a specific audit log entry by its unique identifier. Returns detailed information including the entity affected, action performed, user who made the change, timestamp, and before/after values. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetById(long id)
        {
            var log = await _auditService.GetByIdAsync(id);
            
            if (log == null)
                return NotFound();

            return Ok(log);
        }

        [HttpGet("history/{entityName}/{entityId}")]
        [EndpointDescription("Retrieves the complete change history for a specific entity. Returns all audit log entries for the given entity type and ID in chronological order, allowing administrators to track all modifications made to that entity over time. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetEntityHistory(string entityName, string entityId)
        {
            var history = await _auditService.GetEntityHistoryAsync(entityName, entityId);
            return Ok(history);
        }
    }
}