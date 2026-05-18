using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;

namespace Pharmacy_API.Repositories.Account
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByIdAsync(string id, bool? isDeep = false);
        Task<PagedDto<ApplicationUser>> GetListAsync(UserFilter filter);
        //put your code here
        string GenerateRandomCode();
    }
}