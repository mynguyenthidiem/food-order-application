using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace backend.DTOs.Restaurant
{
    public class UpdateRestaurantDto
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

        public DateTime OpenTime { get; set; }

        public DateTime CloseTime { get; set; }

        [Range(0, 1000000)]
        public decimal DeliveryFee { get; set; }

        public bool IsActive { get; set; }
    }
}