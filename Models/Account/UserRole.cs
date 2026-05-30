using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    public class UserRole : IdentityUserRole<string>
    {
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }
}