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

        [StringLength(350)]
        public string? CategoryName { get; set; }
        [StringLength(350)]
        public string? CategoryAlias { get; set; }
        public string? CategoryImage { get; set; }

        // THÊM: Để xóa ảnh trên Cloudinary
        public string? ImagePublicId { get; set; }

        public string? Sort { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public List<Category>? Children { get; set; }
        public List<Product.Product>? Products { get; set; }
    }
}