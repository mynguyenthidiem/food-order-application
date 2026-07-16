using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Order
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required, Range(0.01, double.MaxValue), Column(TypeName ="decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Required, StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;
        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public virtual Payment Payment { get; set; } = null!;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Preparing,
        Delivering,
        Completed,
        Cancelled
    }

}
