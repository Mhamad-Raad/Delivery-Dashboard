using DeliveryDash.Application.Requests.OrderRequests;
using DeliveryDash.Application.Responses.Common;
using DeliveryDash.Application.Responses.OrderResponses;
using DeliveryDash.Domain.Enums;

namespace DeliveryDash.Application.Abstracts.IService
{
    public interface IOrderService
    {
        Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request, Guid userId);
        Task<OrderDetailResponse> GetOrderByIdAsync(int id, Guid userId, Role userRole, int? vendorId = null);
        Task<OrderDetailResponse> GetOrderByOrderNumberAsync(string orderNumber, Guid userId, Role userRole, int? vendorId = null);
        Task<PagedResponse<OrderResponse>> GetOrdersAsync(
            int page,
            int limit,
            Guid userId,
            Role userRole,
            int? vendorId = null,
            OrderStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? date = null);
        Task<OrderDetailResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request, Guid userId, Role userRole, int? vendorId = null);
        Task<OrderDetailResponse> CancelOrderAsync(int id, Guid userId, Role userRole);
    }
}