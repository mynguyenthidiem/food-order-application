using backend.Models;

namespace backend.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; } 

        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}