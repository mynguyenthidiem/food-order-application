using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}