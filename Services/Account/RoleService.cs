using Microsoft.Extensions.Logging;
using AutoMapper;
using Pharmacy_API.Models;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Services.Account
{
    public class RoleService : IRoleService
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        protected readonly IRoleRepository _roleRepository;
        protected readonly IRolePolicyRepository _rolePolicyRepository;

        #endregion

        #region Constructors
        public RoleService(
            ILogger<RoleService> logger,
            IMapper mapper,
            IRoleRepository roleRepository,
            IRolePolicyRepository rolePolicyRepository
            )
        {
            _logger = logger;
            _mapper = mapper;
            _roleRepository = roleRepository;
            _rolePolicyRepository = rolePolicyRepository;
        }
        #endregion

        #region Insert Role
        public async Task<RoleDto?> InsertRoleAsync(RoleRequestDto requestDto)
        {
            _logger.LogInformation("Insert Role");

            Role role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = requestDto.Name,
                NormalizedName = requestDto.Name.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            Role? newRole = await _roleRepository.InsertAsync(role);
             
            if (newRole != null)
            {
                // Assign policies to the new role
                foreach (var policyId in requestDto.PolicyIds)
                {
                    var rolePolicy = new RolePolicy
                    {
                        RoleId = newRole.Id,
                        PolicyId = policyId
                    };

                    await _rolePolicyRepository.InsertAsync(rolePolicy);
                }

                return _mapper.Map<RoleDto>(newRole);
            }

            return null;
        }
        #endregion

        #region Update Role
        public async Task<int> UpdateRoleAsync(RoleRequestDto requestDto, string id)
        {
            _logger.LogInformation("Update Role");

            // Retrieve the existing role from the repository
            Role existingRole = await _roleRepository.GetAsync(id);

            if (existingRole != null)
            {
                // Update the existing role with new data
                existingRole.Name = requestDto.Name;
                //existingRole.NormalizedName = requestDto.NormalizedName;
                //existingRole.ConcurrencyStamp = requestDto.ConcurrencyStamp;

                // Update the role in the repository
                int updateResult = await _roleRepository.UpdateAsync(existingRole);

                if (updateResult > 0)
                {
                    // Remove existing role-policy associations
                    await _rolePolicyRepository.DeleteByRoleIdAsync(existingRole.Id);

                    // Add new role-policy associations
                    foreach (var policyId in requestDto.PolicyIds)
                    {
                        var rolePolicy = new RolePolicy
                        {
                            RoleId = existingRole.Id,
                            PolicyId = policyId
                        };

                        await _rolePolicyRepository.InsertAsync(rolePolicy);
                    }

                    // Return 1 to indicate success
                    return 1;
                }
                else
                {
                    // Handle the case where update failed
                    return 0;
                }
            }
            else
            {
                // Handle the case where role does not exist
                return 0;
            }
        }
        #endregion

        #region Delete Role
        public async Task<int> DeleteRoleAsync(string id)
        {
            _logger.LogInformation("Delete Role");


            return await _roleRepository.DeleteAsync(id);
        }
        #endregion

        #region Get Role
        public async Task<RoleDto?> GetRoleAsync(string id, bool isDeep = false)
        {
            _logger.LogInformation("Get Role");


            Role? role = await _roleRepository.GetByIdAsync(id, isDeep);
            if (role != null)
            {
                return _mapper.Map<Role, RoleDto>(role);
            }

            return null;
        }
        #endregion

        #region Get List Roles
        public async Task<PagedDto<RoleDto>> GetListRolesAsync(RoleFilterDto filterDto)
        {
            _logger.LogInformation("GetList Roles");

            PagedDto<Role> dt = await _roleRepository.GetListAsync(_mapper.Map<RoleFilterDto, RoleFilter>(filterDto));

            List<RoleDto> dtos = new List<RoleDto>();
            foreach (Role item in dt.Data)
            {
                dtos.Add(_mapper.Map<Role, RoleDto>(item));
            }

            return new PagedDto<RoleDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}