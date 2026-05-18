using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pharmacy_API.Supports;
using Pharmacy_API.Dtos.Account;


namespace Pharmacy_API.Client.Services.Account
{
    public class PermissionClient : ClientBase, IPermissionClient
    {
        #region Fields
        private readonly string _baseUrl;
        #endregion

        #region Constructors
        public PermissionClient(
            IConfiguration configuration,
            HttpClient httpClient,
            TokenProvider tokenProvider) : base(configuration, httpClient, tokenProvider)
        {
            _baseUrl = _configuration["Urls:AccountUrl"] + "/api/Account/Permissions";
        }
        #endregion

        #region Insert
        public async Task<PermissionDto?> InsertPermissionAsync(PermissionRequestDto requestDto)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseUrl))
            {
                requestMessage.Content = JsonContent.Create(requestDto);

                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.Created)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<PermissionDto>();
                    }
                }
            }

            return null;
        }
        #endregion

        #region Update
        public async Task<int> UpdatePermissionAsync(PermissionRequestDto requestDto, string id)
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
        public async Task<int> DeletePermissionAsync(string id)
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
        public async Task<PermissionDto?> GetPermissionAsync(string id, bool? isDeep = null)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, _baseUrl + "/" + id))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<PermissionDto>();
                    }
                }
            }

            return null;
        }
        #endregion

        #region Get List
        public async Task<PagedDto<PermissionDto>?> GetListPermissionsAsync(PermissionFilterDto filterDto)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, UrlUtil.BuildQueryUrl(_baseUrl, filterDto)))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await responseMessage.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<PagedDto<PermissionDto>>(result);
                    }
                }
            }

            return null;
        }
        #endregion
    }
}