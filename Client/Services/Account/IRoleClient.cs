using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Client.Services.Account
{
    public interface IRoleClient
    {
        Task<RoleDto?> InsertRoleAsync(RoleRequestDto requestDto);
        Task<int> UpdateRoleAsync(RoleRequestDto requestDto, string id);
        Task<int> DeleteRoleAsync(string id);
        Task<RoleDto?> GetRoleAsync(string id, bool? isDeep = null);
        Task<PagedDto<RoleDto>?> GetListRolesAsync(RoleFilterDto filterDto);
    }
}