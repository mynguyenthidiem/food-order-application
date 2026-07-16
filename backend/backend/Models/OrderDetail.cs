using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class OrderDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!; 
        [Required]
        public int FoodId { get; set; }
        [ForeignKey(nameof(FoodId))]
        public virtual Food Food { get; set; } = null!;

        [Column(TypeName ="decimal(10,2)"), Range(0.01, double.MaxValue)]
        public decimal SubTotal { get; set; }
    }
}
