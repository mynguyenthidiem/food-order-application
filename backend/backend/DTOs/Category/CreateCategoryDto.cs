using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public int SystemCategoryId { get; set; }
    }
}
