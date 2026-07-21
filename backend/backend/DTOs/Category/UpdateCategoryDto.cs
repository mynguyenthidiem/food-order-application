using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required]
        public int SystemCategoryId { get; set; }
    }
}
