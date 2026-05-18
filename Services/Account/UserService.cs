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
            IOptions<AppSettings> appSettings
           )
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
        }
        #endregion

        #region Insert User
        public async Task<UserDto?> InsertUserAsync(UserRequestDto requestDto)
        {
            _logger.LogInformation("Insert User");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = requestDto.UserName,
                Email = requestDto.Email,
                PhoneNumber = requestDto.PhoneNumber,
                LastLogin = DateTime.UtcNow,
                EmailConfirmed = true,
            };

            var newUser = await _userManager.CreateAsync(user, requestDto.Password);

            if (!newUser.Succeeded)
            {
                _logger.LogError("Failed to create user: {Errors}", string.Join(", ", newUser.Errors.Select(e => e.Description)));
                return null;
            }

            if (requestDto.RoleIds.Count > 0)
            {
                //var roles = await _roleRepository.GetByIdsAsync(requestDto.RoleIds);

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

            // Get names of current roles 
            var currentRoleNames = await _userManager.GetRolesAsync(user);
            var currentRoles = _roleManager.Roles.Where(r => currentRoleNames.Contains(r.Name))
                                                 .ToList();

            var newRoles = _roleManager.Roles.Where(r => requestDto.RoleIds.Contains(r.Id))
                                 .ToList();

            var newRoleNames = newRoles.Select(r => r.Name).ToList();

            // define names of roles to remove
            var rolesToRemove = currentRoles.Select(r => r.Name).Except(newRoleNames).ToList();

            // define names of roles to add
            var rolesToAdd = newRoleNames.Except(currentRoles.Select(r => r.Name)).ToList();

            // remove
            if (rolesToRemove.Any())
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeRolesResult.Succeeded)
                {
                    throw new Exception("Failed to remove roles");
                }
            }

            // add
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

            //ApplicationUser? user = await _userRepository.GetByIdAsync(id, isDeep);
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

            if (isDeep)
            {
                var roleNames = await _userManager.GetRolesAsync(user);

                var roles = await _roleManager.Roles
                                               .Where(role => roleNames.Contains(role.Name))
                                               .Select(role => new RoleDto
                                               {
                                                   Id = role.Id,
                                                   Name = role.Name
                                               }).ToListAsync();
                result.Roles = roles;
            }

            return result;
        }
        #endregion

        #region Get List Users
        public async Task<PagedDto<UserDto>> GetListUsersAsync(UserFilterDto filterDto)
        {
            _logger.LogInformation("GetList Users");

            PagedDto<ApplicationUser> applicationUsers = await _userRepository.GetListAsync(_mapper.Map<UserFilterDto, UserFilter>(filterDto));

            List<UserDto> userDtos = applicationUsers.Data.Select(user => _mapper.Map<ApplicationUser, UserDto>(user)).ToList();

            if (filterDto.IsDeep)
            {
                foreach (var userDto in userDtos)
                {
                    var roleIds = await _userRoleRepository.GetRolesByUserIdAsync(userDto.Id);

                    List<RoleDto> roles = new List<RoleDto>();
                    foreach (var roleId in roleIds)
                    {
                        var role = _roleManager.Roles.Where(r => r.Id == roleId)
                                                          .Select(role => new RoleDto
                                                          {
                                                              Id = role.Id,
                                                              Name = role.Name
                                                          }).FirstOrDefault();
                        if (role != null)
                        {
                            roles.Add(role);
                        }
                    }

                    userDto.Roles = roles;

                }
            }
            return new PagedDto<UserDto>(applicationUsers.TotalRecords, userDtos);
        }
        #endregion

        #region Delete Many Users
        public async Task<int> DeleteManyUsersAsync(ICollection<string> ids)
        {
            _logger.LogInformation("Delete Many Users");

            var usersToDelete = await _userManager.Users.Where(user => ids.Contains(user.Id)).ToListAsync();

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
    }
}