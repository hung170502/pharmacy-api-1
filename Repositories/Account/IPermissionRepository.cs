using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        Task<Permission?> GetByIdAsync(string id, bool? isDeep = false);
        Task<PagedDto<Permission>> GetListAsync(PermissionFilter filter);
        //put your code here
    }
}