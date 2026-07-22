using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.SystemCategory
{
    public class UpdateSystemCategoryDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public IFormFile? Image { get; set; }
    }
}
