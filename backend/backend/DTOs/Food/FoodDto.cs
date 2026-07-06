namespace backend.DTOs.Food
{
    public class FoodDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? Image { get; set; }

        public string Status { get; set; } = "";

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
