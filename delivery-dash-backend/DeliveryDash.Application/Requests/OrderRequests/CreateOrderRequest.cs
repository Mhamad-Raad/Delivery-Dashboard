using System.ComponentModel.DataAnnotations;

namespace DeliveryDash.Application.Requests.OrderRequests
{
    public class CreateOrderRequest
    {
        [Required]
        public int VendorId { get; set; }

        public int? AddressId { get; set; }

        [Required]
        public decimal DeliveryFee { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}