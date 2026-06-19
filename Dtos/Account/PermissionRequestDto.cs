// Dtos/Account/PermissionRequestDto.cs
using Pharmacy_API.Supports;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Account
{
    public partial class PermissionRequestDto : RequestDtoBase
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [StringLength(256)]
        public string? DisplayName { get; set; }  // ✅ THÊM

        [StringLength(128)]
        public string? Group { get; set; }        // ✅ THÊM

        [StringLength(4000)]
        public string? Description { get; set; }  // ✅ THÊM

        [Required]
        public int Sort { get; set; } = 0;
    }
}