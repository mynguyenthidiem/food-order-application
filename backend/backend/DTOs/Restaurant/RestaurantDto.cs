namespace backend.DTOs.Restaurant
{
    public class RestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public decimal DeliveryFee { get; set; }
        public double Rating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OwnerId { get; set; }
    }
}