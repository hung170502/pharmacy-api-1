using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Client.Services.Account
{
    public interface IUserClient
    {
        Task<UserDto?> InsertUserAsync(UserRequestDto requestDto);
        Task<int> UpdateUserAsync(UserRequestDto requestDto, string id);
        Task<int> DeleteUserAsync(string id);
        Task<UserDto?> GetUserAsync(string id, bool? isDeep = null);
        Task<PagedDto<UserDto>?> GetListUsersAsync(UserFilterDto filterDto);
        Task<UserDto> GetPermissionsByUserIdAsync(string userId);
    }
}