using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Security;
using Pharmacy_API.Models;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Services.Account
{
    public class PolicyService : IPolicyService
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        protected readonly IPolicyRepository _policyRepository;
        protected readonly IPolicyPermissionRepository _policyPermissionRepository;
        #endregion

        #region Constructors
        public PolicyService(
            ILogger<PolicyService> logger,
            IMapper mapper,
            IPolicyRepository policyRepository,
            IPolicyPermissionRepository policyPermissionRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _policyRepository = policyRepository;
            _policyPermissionRepository = policyPermissionRepository;
        }
        #endregion

        #region Insert Policy
        public async Task<PolicyDto?> InsertPolicyAsync(PolicyRequestDto requestDto)
        {
            _logger.LogInformation("Insert Policy");

            Policy policy = new Policy
            {
                Id = Guid.NewGuid().ToString(),
                Name = requestDto.Name,
                Description = requestDto.Description,
                Sort = requestDto.Sort
            };

            Policy? newPolicy = await _policyRepository.InsertAsync(policy);

            if (newPolicy != null)
            {
                foreach (var permissionId in requestDto.PermissionIds)
                {
                    // Create a new PolicyPermission for each permissionId
                    var policyPermission = new PolicyPermission
                    {
                        PolicyId = newPolicy.Id,
                        PermissionId = permissionId
                    };

                    await _policyPermissionRepository.InsertAsync(policyPermission);
                }

                // Return the mapped PolicyDto if newPolicy is not null
                // Lấy lại Policy đã insert kèm theo Permissions
                var fullPolicy = await _policyRepository.GetByIdAsync(newPolicy.Id, isDeep: true);
                return _mapper.Map<PolicyDto>(fullPolicy);

            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Update Policy
        public async Task<int> UpdatePolicyAsync(PolicyRequestDto requestDto, string id)
        {
            _logger.LogInformation("Update Policy");

            // Retrieve the existing policy from the repository
            Policy existingPolicy = await _policyRepository.GetAsync(id);

            if (existingPolicy != null)
            {
                // Update the existing policy with new data
                existingPolicy.Name = requestDto.Name;
                existingPolicy.Description = requestDto.Description;
                existingPolicy.Sort = requestDto.Sort;

                // Update the policy in the repository
                int updateResult = await _policyRepository.UpdateAsync(existingPolicy);

                if (updateResult > 0)
                {
                    // Remove existing policy permissions
                    await _policyPermissionRepository.DeleteByPolicyIdAsync(existingPolicy.Id);
                    // Add new policy permissions
                    foreach (var permissionId in requestDto.PermissionIds)
                    {
                        var policyPermission = new PolicyPermission
                        {
                            PolicyId = existingPolicy.Id,
                            PermissionId = permissionId
                        };

                        await _policyPermissionRepository.InsertAsync(policyPermission);
                    }
                    _mapper.Map<PolicyDto>(existingPolicy);
                    // Return the mapped PolicyDto
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
                // Handle the case where policy does not exist
                return 0;
            }
        }
        #endregion

        #region Delete Policy
        public async Task<int> DeletePolicyAsync(string id)
        {
            _logger.LogInformation("Delete Policy");


            return await _policyRepository.DeleteAsync(id);
        }
        #endregion

        #region Get Policy
        public async Task<PolicyDto?> GetPolicyAsync(string id, bool isDeep = false)
        {
            _logger.LogInformation("Get Policy");


            Policy? policy = await _policyRepository.GetByIdAsync(id, isDeep);
            if (policy != null)
            {
                return _mapper.Map<Policy, PolicyDto>(policy);
            }

            return null;
        }
        #endregion

        #region Get List Policies
        public async Task<PagedDto<PolicyDto>> GetListPoliciesAsync(PolicyFilterDto filterDto)
        {
            _logger.LogInformation("GetList Policies");

            PagedDto<Policy> dt = await _policyRepository.GetListAsync(_mapper.Map<PolicyFilterDto, PolicyFilter>(filterDto));

            List<PolicyDto> dtos = new List<PolicyDto>();
            foreach (Policy item in dt.Data)
            {
                dtos.Add(_mapper.Map<Policy, PolicyDto>(item));
            }

            return new PagedDto<PolicyDto>(dt.TotalRecords, dtos);
        }
        #endregion

        #region Assign Permissions to Policy
        public async Task<bool> AssignPermissionsToPolicyAsync(string policyId, List<string> permissionIds)
        {
            try
            {
                _logger.LogInformation($"Assigning {permissionIds.Count} permissions to policy {policyId}");

                // Xóa tất cả permissions cũ
                await _policyPermissionRepository.DeleteByPolicyIdAsync(policyId);

                // Thêm permissions mới
                foreach (var permissionId in permissionIds)
                {
                    var policyPermission = new PolicyPermission
                    {
                        PolicyId = policyId,
                        PermissionId = permissionId
                    };
                    await _policyPermissionRepository.InsertAsync(policyPermission);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error assigning permissions to policy: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Get Permissions of Policy
        public async Task<List<PermissionDto>> GetPermissionsByPolicyIdAsync(string policyId)
        {
            _logger.LogInformation($"Getting permissions for policy {policyId}");

            var policy = await _policyRepository.GetByIdAsync(policyId, isDeep: true);

            if (policy?.PolicyPermissions == null)
                return new List<PermissionDto>();

            var permissions = policy.PolicyPermissions
                .Select(pp => new PermissionDto
                {
                    Id = pp.Permission.Id,
                    Name = pp.Permission.Name,
                    DisplayName = pp.Permission.DisplayName,
                    Group = pp.Permission.Group,
                    Sort = pp.Permission.Sort
                })
                .ToList();

            return permissions;
        }
        #endregion
    }
}