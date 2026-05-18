using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken>
    {
        Task<UserRefreshToken?> GetByIdAsync(string id, bool? isDeep = false);
        Task<PagedDto<UserRefreshToken>> GetListAsync(UserRefreshTokenFilter filter);
        //put your code here
    }
}