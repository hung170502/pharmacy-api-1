using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Client.Services.Account
{
    public interface IPolicyClient
    {
        Task<PolicyDto?> InsertPolicyAsync(PolicyRequestDto requestDto);
        Task<int> UpdatePolicyAsync(PolicyRequestDto requestDto, string id);
        Task<int> DeletePolicyAsync(string id);
        Task<PolicyDto?> GetPolicyAsync(string id, bool? isDeep = null);
        Task<PagedDto<PolicyDto>?> GetListPoliciesAsync(PolicyFilterDto filterDto);
    }
}