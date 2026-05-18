using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IPolicyPermissionRepository : IRepository<PolicyPermission>
    {
        Task<PolicyPermission?> GetByIdAsync(string policyId, string permissionId, bool? isDeep = false);
        Task<PagedDto<PolicyPermission>> GetListAsync(PolicyPermissionFilter filter);
        Task<int> DeleteByPolicyIdAsync(string policyId);
        Task<IEnumerable<string>> GetPermissionIdsByPolicyIdAsync(string policyId);

        //put your code here
    }
}