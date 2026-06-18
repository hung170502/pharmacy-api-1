using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System.Threading.Tasks;
using System.Linq; // ✅ Thêm using này

namespace Pharmacy_API.Repositories.Account
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<UserRole?> GetByIdAsync(string userId, string roleId, bool? isDeep = false);
        Task<PagedDto<UserRole>> GetListAsync(UserRoleFilter filter);

        // ✅ Thêm method để lấy danh sách UserId theo RoleId
        Task<ICollection<string>> GetUserIdsByRoleIdAsync(string roleId);

        // ✅ Method hiện có
        Task<ICollection<string>> GetRolesByUserIdAsync(string userId);

        // ✅ Thêm method để lấy IQueryable (cho filter)
        IQueryable<UserRole> GetQueryable();
    }
}