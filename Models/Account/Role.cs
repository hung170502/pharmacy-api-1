using Microsoft.AspNetCore.Identity;

namespace Pharmacy_API.Models.Account
{
    public class Role : IdentityRole
    {
        public virtual ICollection<RolePolicy> RolePolicies { get; set; }
        public Role()
        {
            RolePolicies = new HashSet<RolePolicy>();
        }
    }
}