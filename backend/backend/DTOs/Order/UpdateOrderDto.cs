using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Order
{
    public class UpdateOrderDto
    {
        [Required]
        [StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public PaymentMethod PaymentMethod { get; set; }
    }
}