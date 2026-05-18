using System;

namespace Pharmacy_API.Dtos.Account
{
    public partial class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public ICollection<PolicyDto> Policies { get; set; } = new HashSet<PolicyDto>();

    }
}