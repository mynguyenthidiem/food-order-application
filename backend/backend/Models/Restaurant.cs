using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Restaurant
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        [StringLength(100),EmailAddress]
        public string? Email { get; set; }
        public string? Description { get; set; }
        public DateTime OpenTime { get; set; } = new(8, 0);
        public DateTime CloseTime { get; set; } = new(22, 0);
        [Column(TypeName = "decimal(10,2)")]
        public decimal DeliveryFee { get; set; } = 0;
        public double Rating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual User Owner { get; set; } = null!;

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        public virtual ICollection<Food> Foods { get; set; } = new List<Food>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}
