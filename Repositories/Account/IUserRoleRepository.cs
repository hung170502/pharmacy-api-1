using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<UserRole?> GetByIdAsync(string userId, string roleId, bool? isDeep = false);
        Task<PagedDto<UserRole>> GetListAsync(UserRoleFilter filter);
        //put your code here

        Task<ICollection<string>> GetRolesByUserIdAsync(string userId);
    }
}