using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Cart
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [Required]
        public int FoodId { get; set; }

        [ForeignKey(nameof(FoodId))]
        public virtual Food? Food { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}