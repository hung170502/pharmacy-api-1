using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<Policy?> GetByIdAsync(string id, bool? isDeep = false);
        Task<PagedDto<Policy>> GetListAsync(PolicyFilter filter);
        //put your code here
    }
}