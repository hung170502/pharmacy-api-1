using Pharmacy_API.Supports;
using System;

namespace Pharmacy_API.Filters.Account
{
    public class RolePolicyFilter : FilterBase
    {
        public string RoleId { get; set; }
        public string PolicyId { get; set; }

        public void Clear()
        {
            RoleId = null;
            PolicyId = null;

        }
    }
}