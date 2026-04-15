using MalDash.Application.Requests.SupportTicketRequests;
using MalDash.Application.Responses.Common;
using MalDash.Application.Responses.SupportTicketResponses;
using MalDash.Domain.Enums;

namespace MalDash.Application.Abstracts.IService
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