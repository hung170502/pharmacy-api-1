using Pharmacy_API.Supports;
using System;

namespace Pharmacy_API.Filters.Account
{
    public class UserRefreshTokenFilter : FilterBase
    {
        public string UserId { get; set; }

        public void Clear()
        {
            UserId = null;

        }
    }
}