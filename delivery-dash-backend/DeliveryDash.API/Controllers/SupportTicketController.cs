using DeliveryDash.API.Extensions;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.SupportTicketRequests;
using DeliveryDash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryDash.API.Controllers
{
    [Route("DeliveryDashApi/[controller]")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _ticketService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;

        public SupportTicketController(
            ISupportTicketService ticketService,
            ICurrentUserService currentUserService,
            IFileStorageService fileStorageService)
        {
            _ticketService = ticketService;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(25 * 1024 * 1024)]
        [EndpointDescription("Creates a new support ticket for the authenticated user. Accepts multipart form data with ticket details including subject, description, category, and optional multiple images (max 25MB total). Automatically associates the ticket with the user. Requires authentication.")]
        public async Task<IActionResult> CreateTicket(
            [FromForm] CreateSupportTicketRequest request,
            List<IFormFile>? Images)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var ticket = await _ticketService.CreateTicketAsync(request, userId);

            if (Images != null && Images.Count > 0)
            {
                var imageUploads = Images.ToImageUploads().ToList();
                var imageUrls = await _fileStorageService.SaveImagesAsync(
                    imageUploads, "tickets", Request.GetBaseUrl());

                if (imageUrls.Count > 0)
                {
                    await _ticketService.AddTicketImagesAsync(ticket.Id, imageUrls);
                    ticket.ImageUrls = imageUrls;
                }
            }

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticket);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Tenant,SuperAdmin,Admin")]
        [EndpointDescription("Retrieves detailed information for a specific support ticket by ID. Returns ticket details including subject, description, status, category, images, and creation date. Tenants can only view their own tickets, while Admins can view all tickets. Restricted to Tenant, SuperAdmin, and Admin roles.")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");

            var ticket = await _ticketService.GetTicketByIdAsync(id, userId, isAdmin);
            return Ok(ticket);
        }

        [HttpGet("my-tickets")]
        [Authorize(Roles = "Tenant, SuperAdmin,Admin")]
        [EndpointDescription("Retrieves a paginated list of support tickets created by the authenticated user. Supports filtering by ticket status (Open, InProgress, Resolved, Closed) and priority (Low, Medium, High, Urgent). Returns ticket summaries with status and creation date. Restricted to Tenant, SuperAdmin, and Admin roles.")]
        public async Task<IActionResult> GetMyTickets(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] TicketStatus? status = null,
            [FromQuery] TicketPriority? priority = null)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var tickets = await _ticketService.GetTicketsAsync(page, limit, userId, false, status, priority);
            return Ok(tickets);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [EndpointDescription("Retrieves a paginated list of all support tickets across the system. Supports filtering by ticket status and priority. Returns ticket summaries including user information, status, priority, and creation date. Used for admin ticket management dashboard. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] TicketStatus? status = null,
            [FromQuery] TicketPriority? priority = null)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var tickets = await _ticketService.GetTicketsAsync(page, limit, userId, true, status, priority);
            return Ok(tickets);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [EndpointDescription("Updates the status of an existing support ticket. Allows admins to move tickets through workflow stages (Open -> InProgress -> Resolved -> Closed). Sends status update notifications to the ticket creator. Restricted to SuperAdmin and Admin roles.")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateTicketStatusRequest request)
        {
            var ticket = await _ticketService.UpdateTicketStatusAsync(id, request);
            return Ok(ticket);
        }
    }
}