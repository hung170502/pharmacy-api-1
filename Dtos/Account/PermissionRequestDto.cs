using Pharmacy_API.Supports;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Account
{
    public partial class PermissionRequestDto : RequestDtoBase
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Sort { get; set; }
    }
}