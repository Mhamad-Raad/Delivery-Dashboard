using DeliveryDash.API.Extensions;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.NotificationRequests;
using DeliveryDash.Domain.Constants;
using DeliveryDash.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDeviceTokenRepository _deviceTokenRepository;
        private readonly IFileStorageService _fileStorageService;

        public NotificationController(
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IDeviceTokenRepository deviceTokenRepository,
            IFileStorageService fileStorageService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _deviceTokenRepository = deviceTokenRepository;
            _fileStorageService = fileStorageService;
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

        [HttpPost("devices")]
        [EndpointDescription("Registers or refreshes the authenticated user's FCM device token so the server can push to this device. Idempotent on the token.")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceTokenRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { message = "Token is required." });

            var userId = _currentUserService.GetCurrentUserId();
            await _deviceTokenRepository.UpsertAsync(new DeviceToken
            {
                UserId = userId,
                Token = request.Token.Trim(),
                Platform = request.Platform,
            }, ct);
            return Ok(new { message = "Device registered." });
        }

        [HttpDelete("devices/{token}")]
        [EndpointDescription("Removes a device token so this device will no longer receive pushes. Called on logout or when the mobile discards its FCM token.")]
        public async Task<IActionResult> DeregisterDevice(string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token is required." });
            await _deviceTokenRepository.DeleteByTokenAsync(token, ct);
            return NoContent();
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = $"{IdentityRoleConstant.SuperAdmin},{IdentityRoleConstant.Admin}")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [EndpointDescription("Admin-only. Sends a notification to all customers or a specific list of customers. Persists a row per target user, broadcasts on SignalR, and pushes via FCM. Optional image is saved via file storage and its URL embedded.")]
        public async Task<IActionResult> Broadcast(
            [FromForm] string title,
            [FromForm] string body,
            [FromForm] string audience,
            [FromForm] List<Guid>? customerIds,
            IFormFile? image,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
                return BadRequest(new { message = "Title and body are required." });

            if (!Enum.TryParse<BroadcastAudience>(audience, ignoreCase: true, out var parsedAudience))
                return BadRequest(new { message = "audience must be 'AllCustomers' or 'SpecificUsers'." });

            if (parsedAudience == BroadcastAudience.SpecificUsers &&
                (customerIds is null || customerIds.Count == 0))
            {
                return BadRequest(new { message = "customerIds must be provided when audience is SpecificUsers." });
            }

            string? imageUrl = null;
            var imageUpload = image.ToImageUpload();
            if (imageUpload.HasValue)
            {
                imageUrl = await _fileStorageService.SaveImageAsync(
                    imageUpload.Value.ImageStream,
                    imageUpload.Value.FileName,
                    "notifications",
                    Request.GetBaseUrl());
            }

            var count = await _notificationService.BroadcastAsync(
                title, body, imageUrl, parsedAudience, customerIds, ct);

            return Ok(new { targeted = count, imageUrl });
        }
    }
}