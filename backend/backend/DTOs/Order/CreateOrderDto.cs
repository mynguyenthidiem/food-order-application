using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        [StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Required, MinLength(1, ErrorMessage = "Please select at least one item.")]
        public List<int> CartIds { get; set; } = new List<int>();
    }
}