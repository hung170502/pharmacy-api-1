using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Account
{
    public interface IPolicyService
    {
        Task<PolicyDto?> InsertPolicyAsync(PolicyRequestDto requestDto);
        Task<int> UpdatePolicyAsync(PolicyRequestDto requestDto, string id);
        Task<int> DeletePolicyAsync(string id);
        Task<PolicyDto?> GetPolicyAsync(string id, bool isDeep = false);
        Task<PagedDto<PolicyDto>> GetListPoliciesAsync(PolicyFilterDto filterDto);
        Task<bool> AssignPermissionsToPolicyAsync(string policyId, List<string> permissionIds);
        Task<List<PermissionDto>> GetPermissionsByPolicyIdAsync(string policyId);
    }
}