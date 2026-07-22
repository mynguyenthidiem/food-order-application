using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        public int RestaurantId { get; set; }

        [ForeignKey(nameof(RestaurantId))]
        public virtual Restaurant Restaurant { get; set; } = null!;

        public int SystemCategoryId { get; set; }

        [ForeignKey(nameof(SystemCategoryId))]
        public virtual SystemCategory SystemCategory { get; set; } = null!;

        public virtual ICollection<Food> Foods { get; set; } = new List<Food>();
    }
}
