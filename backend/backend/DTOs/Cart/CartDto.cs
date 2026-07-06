namespace backend.DTOs.Cart
{
    public class CartDto
    {
        public int Id { get; set; }

        public int FoodId { get; set; }

        public string FoodName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? Image { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime AddedAt { get; set; }
    }
}