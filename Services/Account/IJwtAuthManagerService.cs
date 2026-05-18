using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public interface IJwtAuthManagerService
    {
        Task<JwtAuthResultDto> GenerateTokens(ApplicationUser user, IEnumerable<Claim> claims, DateTime now);
        Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user);
    }
}
