using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cart
{
    public class UpdateCartDto
    {
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}