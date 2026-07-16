using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; } 
    }
}