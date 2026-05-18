using Pharmacy_API.Supports;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Account
{
    public partial class RoleRequestDto : RequestDtoBase
    {
        [StringLength(256)]
        public string? Name { get; set; }

        //[StringLength(256)]
        //public string? NormalizedName{ get; set; }

        //[StringLength(4000)]
        //public string? ConcurrencyStamp{ get; set; }
        public ICollection<string> PolicyIds { get; set; } = new HashSet<string>();
        public RoleRequestDto()
        {
            PolicyIds = new HashSet<string>();
            Name = string.Empty;
            //NormalizedName = string.Empty;
            //ConcurrencyStamp = string.Empty;
        }
    }
}