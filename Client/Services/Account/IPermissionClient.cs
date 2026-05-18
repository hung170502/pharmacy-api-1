using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Client.Services.Account
{
    public interface IPermissionClient
    {
        Task<PermissionDto?> InsertPermissionAsync(PermissionRequestDto requestDto);
        Task<int> UpdatePermissionAsync(PermissionRequestDto requestDto, string id);
        Task<int> DeletePermissionAsync(string id);
        Task<PermissionDto?> GetPermissionAsync(string id, bool? isDeep = null);
        Task<PagedDto<PermissionDto>?> GetListPermissionsAsync(PermissionFilterDto filterDto);
    }
}