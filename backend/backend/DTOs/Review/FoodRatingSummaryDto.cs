namespace backend.DTOs.Review
{
    public class FoodRatingSummaryDto
    {
        public int FoodId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewResponseDto> Reviews { get; set; } = new List<ReviewResponseDto>();
    }
}
