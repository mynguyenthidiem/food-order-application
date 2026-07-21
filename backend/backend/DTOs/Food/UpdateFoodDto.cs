using backend.Models;
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

        public IFormFile? Image { get; set; }

        public FoodStatus Status { get; set; }

        [Required]
        public int? CategoryId { get; set; }
    }
}
