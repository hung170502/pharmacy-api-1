using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pharmacy_API.Supports;
using Pharmacy_API.Dtos.Account;

namespace Pharmacy_API.Client.Services.Account
{
    public class PolicyClient : ClientBase, IPolicyClient
    {
        #region Fields
        private readonly string _baseUrl;
        #endregion

        #region Constructors
        public PolicyClient(
            IConfiguration configuration,
            HttpClient httpClient,
            TokenProvider tokenProvider) : base(configuration, httpClient, tokenProvider)
        {
            _baseUrl = _configuration["Urls:AccountUrl"] + "/api/Account/Policies";
        }
        #endregion

        #region Insert
        public async Task<PolicyDto?> InsertPolicyAsync(PolicyRequestDto requestDto)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseUrl))
            {
                requestMessage.Content = JsonContent.Create(requestDto);

                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.Created)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<PolicyDto>();
                    }
                }
            }

            return null;
        }
        #endregion

        #region Update
        public async Task<int> UpdatePolicyAsync(PolicyRequestDto requestDto, string id)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, _baseUrl + "/" + id))
            {
                requestMessage.Content = JsonContent.Create(requestDto);

                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<int>();
                    }
                }
            }

            return 0;
        }
        #endregion

        #region Delete
        public async Task<int> DeletePolicyAsync(string id)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, _baseUrl + "/" + id))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<int>();
                    }
                }
            }

            return 0;
        }
        #endregion

        #region Get
        public async Task<PolicyDto?> GetPolicyAsync(string id, bool? isDeep = null)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, _baseUrl + "/" + id))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<PolicyDto>();
                    }
                }
            }

            return null;
        }
        #endregion

        #region Get List
        public async Task<PagedDto<PolicyDto>?> GetListPoliciesAsync(PolicyFilterDto filterDto)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, UrlUtil.BuildQueryUrl(_baseUrl, filterDto)))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await responseMessage.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<PagedDto<PolicyDto>>(result);
                    }
                }
            }

            return null;
        }
        #endregion
    }
}