using Pharmacy_API.Supports;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Account
{
    public partial class UserRequestDto : RequestDtoBase
    {
        [StringLength(256)]
        public string UserName { get; set; }

        [StringLength(256)]
        public string? Password { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(4000)]
        public string PhoneNumber { get; set; }

        public List<string> RoleIds { get; set; }

        public UserRequestDto()
        {
            UserName = "";
            Password = "";
            Email = "";
            PhoneNumber = "";
            RoleIds = new List<string>();

        }
    }
}