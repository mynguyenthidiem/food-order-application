using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required]
        public int FoodId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
