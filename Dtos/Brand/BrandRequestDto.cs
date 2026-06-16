using Microsoft.AspNetCore.Http; // ✅ THÊM DÒNG NÀY
using Pharmacy_API.Supports;

namespace Pharmacy_API.Dtos.Brand
{
    public class BrandRequestDto : RequestDtoBase
    {
        public string BrandName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Sort { get; set; }

        // URL ảnh từ Cloudinary (khi frontend upload trước)
        public string BrandImage { get; set; }

        // Public ID từ Cloudinary
        public string ImagePublicId { get; set; }

        // ✅ File upload (nếu upload trực tiếp) - CẦN using Microsoft.AspNetCore.Http
        public IFormFile? Image { get; set; }
    }
}