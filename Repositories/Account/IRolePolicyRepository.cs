using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IRolePolicyRepository : IRepository<RolePolicy>
    {
        Task<PagedDto<RolePolicy>> GetListAsync(RolePolicyFilter filter);
        Task<int> DeleteByRoleIdAsync(string policyId);
        public Task<IEnumerable<string>> GetPolicyIdsForRoleAsync(string roleId);

        //put your code here
    }
}