using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
