using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cart
{
    public class AddCartDto
    {
        [Required]
        public int FoodId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}