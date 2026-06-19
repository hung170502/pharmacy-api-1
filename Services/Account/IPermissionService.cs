using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Account
{
    public interface IPermissionService
    {
        Task<PermissionDto?> InsertPermissionAsync(PermissionRequestDto requestDto);
        Task<int> UpdatePermissionAsync(PermissionRequestDto requestDto, string id);
        Task<int> DeletePermissionAsync(string id);
        Task<PermissionDto?> GetPermissionAsync(string id, bool isDeep = false);
        Task<PagedDto<PermissionDto>> GetListPermissionsAsync(PermissionFilterDto filterDto);
    }
}