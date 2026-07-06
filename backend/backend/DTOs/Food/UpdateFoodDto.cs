using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Food
{
    public class UpdateFoodDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? Image { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available";

        [Required]
        public int CategoryId { get; set; }
    }
}
