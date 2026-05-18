using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Account
{
    public interface IRoleService
    {
        Task<RoleDto?> InsertRoleAsync(RoleRequestDto requestDto);
        Task<int> UpdateRoleAsync(RoleRequestDto requestDto, string id);
        Task<int> DeleteRoleAsync(string id);
        Task<RoleDto?> GetRoleAsync(string id, bool isDeep = false);
        Task<PagedDto<RoleDto>> GetListRolesAsync(RoleFilterDto filterDto);
    }
}