using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Review
{
    public class UpdateReviewDto
    {
        [Required, Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
