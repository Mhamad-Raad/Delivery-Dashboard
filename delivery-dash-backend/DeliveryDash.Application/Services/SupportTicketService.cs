using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Requests.SupportTicketRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.SupportTicketResponses;
using DeliveryDash.Domain.Entities;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _ticketRepository;
        private readonly IEntityImageRepository _imageRepository;

        public SupportTicketService(
            ISupportTicketRepository ticketRepository,
            IEntityImageRepository imageRepository)
        {
            _ticketRepository = ticketRepository;
            _imageRepository = imageRepository;
        }

        public async Task<SupportTicketDetailResponse> CreateTicketAsync(CreateSupportTicketRequest request, Guid userId)
        {
            var ticketNumber = await GenerateTicketNumberAsync();

            var ticket = new SupportTicket
            {
                TicketNumber = ticketNumber,
                UserId = userId,
                Subject = request.Subject,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open
            };

            var created = await _ticketRepository.CreateAsync(ticket);
            return await MapToDetailResponse(created);
        }

        public async Task<SupportTicketDetailResponse> GetTicketByIdAsync(int id, Guid userId, bool isAdmin)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Ticket with ID {id} not found.");

            if (!isAdmin && ticket.UserId != userId)
                throw new UnauthorizedAccessException("You don't have access to this ticket.");

            return await MapToDetailResponse(ticket);
        }

        public async Task<PagedResponse<SupportTicketResponse>> GetTicketsAsync(
            int page, int limit, Guid userId, bool isAdmin, TicketStatus? status = null, TicketPriority? priority = null)
        {
            var filterUserId = isAdmin ? null : (Guid?)userId;

            var (tickets, total) = await _ticketRepository.GetTicketsPagedAsync(page, limit, filterUserId, status, priority);

            return new PagedResponse<SupportTicketResponse>
            {
                Data = tickets.Select(MapToResponse).ToList(),
                Page = page,
                Limit = limit,
                Total = total
            };
        }

        public async Task<SupportTicketDetailResponse> UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Ticket with ID {id} not found.");

            ticket.Status = request.Status;
            ticket.AdminNotes = request.AdminNotes;

            if (request.Status == TicketStatus.Resolved || request.Status == TicketStatus.Closed)
                ticket.ResolvedAt = DateTime.UtcNow;

            var updated = await _ticketRepository.UpdateAsync(ticket);
            return await MapToDetailResponse(updated);
        }

        public async Task AddTicketImagesAsync(int ticketId, List<string> imageUrls)
        {
            await _imageRepository.AddImagesAsync("SupportTicket", ticketId.ToString(), imageUrls);
        }

        public async Task<List<string>> GetTicketImagesAsync(int ticketId)
        {
            var images = await _imageRepository.GetImagesAsync("SupportTicket", ticketId.ToString());
            return images.Select(i => i.ImageUrl).ToList();
        }

        private async Task<string> GenerateTicketNumberAsync()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _ticketRepository.GetTodayTicketCountAsync();
            return $"TKT-{date}-{(count + 1):D4}";
        }

        private async Task<SupportTicketDetailResponse> MapToDetailResponse(SupportTicket ticket)
        {
            var images = await GetTicketImagesAsync(ticket.Id);

            return new SupportTicketDetailResponse
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                UserId = ticket.UserId,
                UserName = $"{ticket.User?.FirstName} {ticket.User?.LastName}",
                UserEmail = ticket.User?.Email ?? string.Empty,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,
                AdminNotes = ticket.AdminNotes,
                ImageUrls = images
            };
        }

        private static SupportTicketResponse MapToResponse(SupportTicket ticket)
        {
            return new SupportTicketResponse
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,
                UserName = $"{ticket.User?.FirstName} {ticket.User?.LastName}"
            };
        }
    }
}