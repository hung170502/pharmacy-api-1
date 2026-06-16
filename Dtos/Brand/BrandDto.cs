using Pharmacy_API.Models.Category;
using Pharmacy_API.Models.Product;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Brand
{
    public class BrandDto
    {
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? BrandImage { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Sort { get; set; }
        public string ImagePublicId { get; set; } // ✅ Thêm nếu muốn trả về

    }
}
