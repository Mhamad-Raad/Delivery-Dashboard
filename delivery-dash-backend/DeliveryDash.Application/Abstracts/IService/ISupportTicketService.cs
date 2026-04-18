using DeliveryDash.Application.Requests.SupportTicketRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.SupportTicketResponses;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface ISupportTicketService
    {
        Task<SupportTicketDetailResponse> CreateTicketAsync(CreateSupportTicketRequest request, Guid userId);
        Task<SupportTicketDetailResponse> GetTicketByIdAsync(int id, Guid userId, bool isAdmin);
        Task<PagedResponse<SupportTicketResponse>> GetTicketsAsync(
            int page, int limit, Guid userId, bool isAdmin, TicketStatus? status = null, TicketPriority? priority = null);
        Task<SupportTicketDetailResponse> UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request);
        Task AddTicketImagesAsync(int ticketId, List<string> imageUrls);
        Task<List<string>> GetTicketImagesAsync(int ticketId);
    }
}