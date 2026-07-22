using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace backend.DTOs.Restaurant
{
    public class CreateRestaurantDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Address { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }

        [StringLength(20), Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(100), EmailAddress]
        public string? Email { get; set; }

        public string? Description { get; set; }

        public TimeOnly OpenTime { get; set; } = new(8, 0);

        public TimeOnly CloseTime { get; set; } = new(22, 0);

        [Range(0, 1000000, ErrorMessage = "Phí giao hàng không hợp lệ.")]
        public decimal DeliveryFee { get; set; } = 0;
    }
}