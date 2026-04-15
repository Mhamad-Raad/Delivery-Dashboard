using MalDash.Domain.Entities;
using MalDash.Domain.Enums;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<(IEnumerable<Order> Orders, int Total)> GetOrdersPagedAsync(
            int page,
            int limit,
            Guid? userId = null,
            int? vendorId = null,
            OrderStatus? status = null,
            IEnumerable<int>? orderIds = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? date = null);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<int> GetTodayOrderCountAsync();
    }
}