using Pharmacy_API.Supports;
using System;

namespace Pharmacy_API.Filters.Account
{
    public class UserRoleFilter : FilterBase
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public void Clear()
        {
            UserId = null;
            RoleId = null;

        }
    }
}