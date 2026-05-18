using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByIdAsync(string id, bool? isDeep = false);
        Task<PagedDto<Role>> GetListAsync(RoleFilter filter);
        //put your code here


    }
}