namespace backend.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public int SystemCategoryId { get; set; }

        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public string? Image { get; set; }
    }
}
