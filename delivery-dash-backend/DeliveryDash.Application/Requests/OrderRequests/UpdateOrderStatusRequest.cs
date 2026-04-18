using DeliveryDash.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DeliveryDash.Application.Requests.OrderRequests
{
    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}