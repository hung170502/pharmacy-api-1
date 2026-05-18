using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public partial class LoginResponseDto : ErrorResponseDto
    {
        public UserDataDto Data { get; set; }
    }

    public partial class UserDataDto
    {
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string UserId { get; set; }

        // Thêm danh sách Roles vào DTO
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsOnline { get; set; }
        public UserDataDto()
        {
            Email = string.Empty;
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            Name = string.Empty;
            Phone = string.Empty;
            UserId = string.Empty;
            IsOnline = false;
        }
    }
}
