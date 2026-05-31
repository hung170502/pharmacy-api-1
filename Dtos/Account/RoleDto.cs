using System;

namespace Pharmacy_API.Dtos.Account
{
    public partial class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? DisplayName { get; set; }  // ✅ Thêm
        public string? Description { get; set; }   // ✅ Thêm
        public int Sort { get; set; }              // ✅ Thêm
        public int UserCount { get; set; }         // ✅ Thêm
        public ICollection<PolicyDto> Policies { get; set; } = new HashSet<PolicyDto>();
    }
}