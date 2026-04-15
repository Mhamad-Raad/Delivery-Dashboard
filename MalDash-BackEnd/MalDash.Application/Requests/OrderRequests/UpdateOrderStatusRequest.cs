using MalDash.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MalDash.Application.Requests.OrderRequests
{
    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}