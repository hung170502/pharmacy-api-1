using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Account
{
    public interface IUserService
    {
        Task<UserDto?> InsertUserAsync(UserRequestDto requestDto);
        Task<int> UpdateUserAsync(UserRequestDto requestDto, string id);
        Task<int> DeleteUserAsync(string id);
        Task<UserDto?> GetUserAsync(string id, bool isDeep = false);
        Task<PagedDto<UserDto>> GetListUsersAsync(UserFilterDto filterDto);
        Task<int> DeleteManyUsersAsync(ICollection<string> Ids);
        Task<HashSet<string>> GetPermissionsByUserIdAsync(string userId);


    }
}