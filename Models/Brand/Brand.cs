using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Pharmacy_API.Models.Product;

namespace Pharmacy_API.Models.Brand
{
    public class Brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BrandId { get; set; }
        [StringLength(300)]
        public string? BrandName { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string? Email { get; set; }
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits")]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }
        public string? BrandImage { get; set; }
        public string? ImagePublicId { get; set; }

        [StringLength(1000)]
        public string? Address { get; set; }
        [StringLength(3000)]
        public string? Description { get; set; }
        public string? Sort { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public List<Product.Product>? Products { get; set; }
    }
}