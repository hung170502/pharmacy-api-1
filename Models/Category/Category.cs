using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Pharmacy_API.Models.Product;

namespace Pharmacy_API.Models.Category
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        //public Category? Parent { get; set; }

        [StringLength(350)]
        public string? CategoryName { get; set; }
        [StringLength(350)]
        public string? CategoryAlias { get; set; }
        public string? CategoryImage { get; set; }
        public string? Sort { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public List<Category>? Children { get; set; }
        public List<Product.Product>? Products { get; set; }
    }
}
