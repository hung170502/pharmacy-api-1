using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Models.Country
{
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CountryId { get; set; }
        [StringLength(300)]
        public string CountryName { get; set; }
        public string? Sort { get; set; }
        public List<Product.Product>? Products { get; set; }
    }
}
