// Dtos/Brand/BrandRequestDto.cs
public class BrandRequestDto
{
    public string BrandName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public string Sort { get; set; }

    // ✅ THÊM 2 FIELD NÀY
    public string BrandImage { get; set; }      // URL ảnh từ Cloudinary
    public string ImagePublicId { get; set; }   // Public ID từ Cloudinary
}