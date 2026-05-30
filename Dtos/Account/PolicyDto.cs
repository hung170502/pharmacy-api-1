using System;

namespace Pharmacy_API.Dtos.Account
{
    public partial class PolicyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;  // ✅ Thêm dòng này

        public string? Description { get; set; }
        public int Sort { get; set; }
        public ICollection<PermissionDto> Permissions { get; set; } = new HashSet<PermissionDto>();
        public PolicyDto()
        {
            Id = string.Empty;
            Name = string.Empty;
            DisplayName = string.Empty;  // ✅ Thêm dòng này

            Description = string.Empty;
            Sort = 0;
            Permissions = new HashSet<PermissionDto>();
        }
    }
}