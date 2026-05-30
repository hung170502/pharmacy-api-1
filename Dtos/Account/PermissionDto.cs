namespace Pharmacy_API.Dtos.Account
{
    public class PermissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;  // ✅ Thêm
        public string Group { get; set; } = string.Empty;         // ✅ Thêm
        public string? Description { get; set; }
        public int Sort { get; set; }
    }
}