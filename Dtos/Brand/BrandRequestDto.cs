using Pharmacy_API.Supports;

namespace Pharmacy_API.Dtos.Brand
{
    public class BrandRequestDto : RequestDtoBase
    {
        public string? BrandName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? BrandImage { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Sort { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImagePublicId { get; set; }
    }
}