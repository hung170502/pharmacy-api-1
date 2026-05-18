using Pharmacy_API.Supports;
using System;

namespace Pharmacy_API.Filters.Account
{
    public class PolicyPermissionFilter : FilterBase
    {
        public string PolicyId { get; set; }
        public string PermissionId { get; set; }

        public void Clear()
        {
            PolicyId = null;
            PermissionId = null;

        }
    }
}