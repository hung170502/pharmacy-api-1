using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Services.Account
{
    public class UserService : IUserService
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        protected readonly IUserRepository _userRepository;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<Role> _roleManager;
        protected readonly IUserRoleRepository _userRoleRepository;
        protected readonly IRolePolicyRepository _rolePolicyRepository;
        protected readonly IPolicyPermissionRepository _policyPermissionRepository;
        protected readonly IPermissionRepository _permissionRepository;
        private readonly AppSettings _appSettings;
        private readonly IEmailSenderService _emailSender;  // ✅ Thêm
        #endregion

        #region Constructors
        public UserService(
            ILogger<UserService> logger,
            IMapper mapper,
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<Role> roleManager,
            IUserRoleRepository userRoleRepository,
            IRolePolicyRepository rolePolicyRepository,
            IPolicyPermissionRepository policyPermissionRepository,
            IPermissionRepository permissionRepository,
            IOptions<AppSettings> appSettings,
            IEmailSenderService emailSender)
        {
            _logger = logger;
            _mapper = mapper;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
            _rolePolicyRepository = rolePolicyRepository;
            _policyPermissionRepository = policyPermissionRepository;
            _permissionRepository = permissionRepository;
            _appSettings = appSettings.Value;
            _emailSender = emailSender;  // ✅ Thêm
        }
        #endregion

        #region Insert User
        public async Task<UserDto?> InsertUserAsync(UserRequestDto requestDto)
        {
            _logger.LogInformation("Insert User");

            // ✅ Tự động generate mật khẩu ngẫu nhiên
            var generatedPassword = GenerateRandomPassword();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = requestDto.UserName,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                LastLogin = DateTime.UtcNow,
                EmailConfirmed = true,
            };

            // ✅ Dùng mật khẩu tự generate
            var newUser = await _userManager.CreateAsync(user, generatedPassword);

            if (!newUser.Succeeded)
            {
                _logger.LogError("Failed to create user: {Errors}", string.Join(", ", newUser.Errors.Select(e => e.Description)));
                return null;
            }

            if (requestDto.RoleIds.Count > 0)
            {
                var query = _roleManager.Roles.Where(role => requestDto.RoleIds.Contains(role.Id));
                var roles = query.ToList();

                if (roles.Count != requestDto.RoleIds.Count)
                {
                    _logger.LogError("Failed to create user: Role Id is not exist");
                    return null;
                }

                if (roles.Count > 0)
                {
                    List<string> roleNames = roles.Select(role => role.Name).ToList();
                    var result = await _userManager.AddToRolesAsync(user, roleNames);
                    if (!result.Succeeded)
                    {
                        return null;
                    }
                }
            }

            _logger.LogInformation("User created successfully");

            // ✅ Gửi email với mật khẩu tự generate
            try
            {
                string emailContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 20px;'>
                <h2 style='color: #2563eb;'>Nhà thuốc An Tâm Việt</h2>
                <p>Xin chào <strong>{requestDto.UserName}</strong>,</p>
                <p>Tài khoản nhân viên của bạn đã được tạo thành công.</p>
                <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                    <p><strong>Email đăng nhập:</strong> {requestDto.Email}</p>
                    <p><strong>Mật khẩu:</strong> {generatedPassword}</p>
                </div>
                <p style='color: #dc2626; font-size: 13px;'>⚠️ Vui lòng đăng nhập và đổi mật khẩu ngay sau lần đầu sử dụng.</p>
                <a href='http://localhost:3000/login' style='display: inline-block; background: #2563eb; color: white; padding: 10px 20px; border-radius: 6px; text-decoration: none;'>Đăng nhập ngay</a>
            </div>";

                await _emailSender.SendEmailAsync(requestDto.Email, "Tài khoản Nhà thuốc An Tâm Việt", emailContent);
                _logger.LogInformation($"Email sent to {requestDto.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
            }


            var roleNamesOfNewUser = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleNames = roleNamesOfNewUser,
                LastLogin = user.LastLogin,
                IsOnline = user.IsOnline,
            };

            return userDto;
        }
        #endregion

        #region Update User
        public async Task<int> UpdateUserAsync(UserRequestDto requestDto, string id)
        {
            _logger.LogInformation("Updating User with ID: {UserId}", id);

            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", id);
                return 0;
            }

            user.UserName = requestDto.UserName;
            user.Email = requestDto.Email;
            user.PhoneNumber = requestDto.PhoneNumber;
            user.LastLogin = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return 0;
            }

            var currentRoleNames = await _userManager.GetRolesAsync(user);
            var currentRoles = _roleManager.Roles.Where(r => currentRoleNames.Contains(r.Name)).ToList();
            var newRoles = _roleManager.Roles.Where(r => requestDto.RoleIds.Contains(r.Id)).ToList();
            var newRoleNames = newRoles.Select(r => r.Name).ToList();
            var rolesToRemove = currentRoles.Select(r => r.Name).Except(newRoleNames).ToList();
            var rolesToAdd = newRoleNames.Except(currentRoles.Select(r => r.Name)).ToList();

            if (rolesToRemove.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeRolesResult.Succeeded)
                {
                    throw new Exception("Failed to remove roles");
                }
            }

            if (rolesToAdd.Any())
            {
                var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addRolesResult.Succeeded)
                {
                    throw new Exception("Failed to add roles");
                }
            }

            _logger.LogInformation("User with ID: {UserId} updated successfully", id);
            return 1;
        }
        #endregion

        #region Delete User
        public async Task<int> DeleteUserAsync(string id)
        {
            _logger.LogInformation("Delete User");

            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null) return 0;

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return 0;
            }

            return 1;
        }
        #endregion

        #region Get User
        public async Task<UserDto?> GetUserAsync(string id, bool isDeep = false)
        {
            _logger.LogInformation("Get User");

            var user = await _userManager.FindByIdAsync(id);
            string storagePath = _appSettings.ImageStoragePath;
            string avatarUrl;

            if (user != null)
            {
                avatarUrl = Path.Combine(storagePath, user.Id, user.AvatarUrl ?? string.Empty);
                if (File.Exists(avatarUrl))
                {
                    byte[] imageArray = File.ReadAllBytes(avatarUrl);
                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                    user.AvatarUrl = base64ImageRepresentation;
                }
            }

            var result = _mapper.Map<ApplicationUser, UserDto>(user);

            if (user != null)
            {
                // ✅ Luôn load roleNames
                var roleNames = await _userManager.GetRolesAsync(user);
                result.RoleNames = roleNames.ToList();

                if (isDeep)
                {
                    var roles = await _roleManager.Roles
                        .Where(role => roleNames.Contains(role.Name))
                        .Select(role => new RoleDto
                        {
                            Id = role.Id,
                            Name = role.Name
                        }).ToListAsync();
                    result.Roles = roles;
                }
            }

            return result;
        }
        #endregion

        #region Get List Users
        public async Task<PagedDto<UserDto>> GetListUsersAsync(UserFilterDto filterDto)
        {
            _logger.LogInformation("GetList Users");

            // ✅ ExcludeAdmins là bool, dùng trực tiếp
            var excludeAdmins = filterDto.ExcludeAdmins; // Không cần ?? hay GetValueOrDefault

            // Lấy danh sách user từ repository
            var userFilter = _mapper.Map<UserFilterDto, UserFilter>(filterDto);
            PagedDto<ApplicationUser> applicationUsers = await _userRepository.GetListAsync(userFilter);

            // Lấy danh sách Admin UserIds để loại bỏ
            ICollection<string>? adminUserIds = null;
            if (excludeAdmins) // ✅ Dùng trực tiếp
            {
                var adminRole = await _roleManager.Roles
                    .FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");

                if (adminRole != null)
                {
                    adminUserIds = await _userRoleRepository.GetUserIdsByRoleIdAsync(adminRole.Id);
                    _logger.LogInformation($"Found {adminUserIds?.Count ?? 0} admin users to exclude");
                }
            }

            // Lọc bỏ Admin
            var filteredUsers = applicationUsers.Data;
            if (adminUserIds != null && adminUserIds.Any())
            {
                filteredUsers = filteredUsers.Where(u => !adminUserIds.Contains(u.Id)).ToList();
                _logger.LogInformation($"Filtered out {applicationUsers.Data.Count - filteredUsers.Count} admin users");
            }

            // Map to DTO
            List<UserDto> userDtos = filteredUsers
                .Select(user => _mapper.Map<ApplicationUser, UserDto>(user))
                .ToList();

            // Load roles
            foreach (var userDto in userDtos)
            {
                var user = await _userManager.FindByIdAsync(userDto.Id);
                if (user != null)
                {
                    var roleNames = await _userManager.GetRolesAsync(user);
                    userDto.RoleNames = roleNames.ToList();
                }

                if (filterDto.IsDeep)
                {
                    var roleIds = await _userRoleRepository.GetRolesByUserIdAsync(userDto.Id);
                    List<RoleDto> roles = new List<RoleDto>();
                    foreach (var roleId in roleIds)
                    {
                        var role = _roleManager.Roles
                            .Where(r => r.Id == roleId)
                            .Select(r => new RoleDto
                            {
                                Id = r.Id,
                                Name = r.Name,
                                DisplayName = r.DisplayName,
                                Description = r.Description
                            }).FirstOrDefault();
                        if (role != null)
                        {
                            roles.Add(role);
                        }
                    }
                    userDto.Roles = roles;
                }
            }

            // Cập nhật total records
            var totalRecords = applicationUsers.TotalRecords;
            if (adminUserIds != null && adminUserIds.Any())
            {
                totalRecords = userDtos.Count;
            }

            _logger.LogInformation($"GetList Users: {totalRecords} total, {userDtos.Count} returned, ExcludeAdmins={excludeAdmins}");

            return new PagedDto<UserDto>(totalRecords, userDtos);
        }
        #endregion

        #region Delete Many Users
        public async Task<int> DeleteManyUsersAsync(ICollection<string> ids)
        {
            _logger.LogInformation("Delete Many Users");

            var usersToDelete = await _userManager.Users
                .Where(user => ids.Contains(user.Id))
                .ToListAsync();

            int deleteCount = 0;

            foreach (var user in usersToDelete)
            {
                _userRepository.Remove(user);
                deleteCount++;
            }
            deleteCount = await _userRepository.UnitOfWork.SaveChangesAsync();

            if (deleteCount != ids.Count)
            {
                _logger.LogInformation("There was an error while saving to the database");
                return 0;
            }

            return ids.Count;
        }
        #endregion

        #region Get Permissions By User ID
        public async Task<HashSet<string>> GetPermissionsByUserIdAsync(string userId)
        {
            var roleIds = await _userRoleRepository.GetRolesByUserIdAsync(userId);
            HashSet<string>? permissions = new HashSet<string>();

            if (roleIds.Any())
            {
                foreach (var roleId in roleIds)
                {
                    var permissionSetIds = await _rolePolicyRepository.GetPolicyIdsForRoleAsync(roleId);
                    if (permissionSetIds.Any())
                    {
                        foreach (var permissionSetId in permissionSetIds)
                        {
                            var permissionIds = await _policyPermissionRepository.GetPermissionIdsByPolicyIdAsync(permissionSetId);
                            if (permissionIds.Any())
                            {
                                foreach (var permissionId in permissionIds)
                                {
                                    var permission = await _permissionRepository.GetByIdAsync(permissionId);
                                    if (permission != null)
                                    {
                                        permissions.Add(permission.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return permissions;
        }
        #endregion
        #region Helper
        private string GenerateRandomPassword(int length = 10)
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%";

            var random = new Random();
            var password = new char[length];

            // Đảm bảo có ít nhất 1 ký tự mỗi loại
            password[0] = uppercase[random.Next(uppercase.Length)];
            password[1] = lowercase[random.Next(lowercase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];

            // Phần còn lại random
            string allChars = uppercase + lowercase + digits + special;
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // Xáo trộn
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
        #endregion
    }
}