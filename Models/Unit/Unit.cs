using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Models.Unit
{
    public class Unit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UnitId { get; set; }
        [StringLength(300)]
        public string UnitName { get; set; }
        public string? Sort { get; set; }
        public List<Product.Product>? Products { get; set; }
    }
}
