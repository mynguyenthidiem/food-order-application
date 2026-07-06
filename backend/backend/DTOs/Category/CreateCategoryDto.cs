using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(300)]
        public string? Image { get; set; }
    }
}
