using Pharmacy_API.Supports;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Account
{
    public partial class PolicyRequestDto : RequestDtoBase
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        public int Sort { get; set; }

        public ICollection<string> PermissionIds { get; set; } = new HashSet<string>();

        public PolicyRequestDto()
        {
            PermissionIds = new HashSet<string>();
            Name = string.Empty;
            Description = string.Empty;
            Sort = 0;
        }
    }
}