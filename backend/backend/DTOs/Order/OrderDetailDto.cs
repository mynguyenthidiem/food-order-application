namespace backend.DTOs.Order
{
    public class OrderDetailDto
    {
        public int FoodId { get; set; }

        public string FoodName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal SubTotal { get; set; }
    }
}