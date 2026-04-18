using DeliveryDash.Application.Abstracts.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationController(
            INotificationService notificationService, 
            ICurrentUserService currentUserService
            )
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [EndpointDescription("Retrieves a paginated list of notifications for the authenticated user. Returns notifications sorted by creation date (newest first) with skip/take pagination. Includes notification details such as title, message, type, read status, and action URL. Requires authentication.")]
        public async Task<IActionResult> GetNotifications([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, skip, take);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        [EndpointDescription("Retrieves the count of unread notifications for the authenticated user. Used to display notification badges in the UI. Returns a simple count value. Requires authentication.")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        [EndpointDescription("Marks a specific notification as read for the authenticated user. Updates the read status and timestamp. Used when a user clicks on or views a notification. Requires authentication.")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(new { message = "Notification marked as read." });
        }

        [HttpPut("mark-all-read")]
        [EndpointDescription("Marks all notifications as read for the authenticated user. Bulk operation to clear all unread notifications at once. Updates read status and timestamp for all unread notifications. Requires authentication.")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _currentUserService.GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read." });
        }

        [HttpDelete("{id}")]
        [EndpointDescription("Permanently deletes a specific notification for the authenticated user. Removes the notification from the user's notification list. Cannot be undone. Requires authentication.")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);
            return Ok(new { message = "Notification deleted." });
        }
    }
}