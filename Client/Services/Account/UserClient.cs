using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using Pharmacy_API.Supports;
using Pharmacy_API.Dtos.Account;

namespace Pharmacy_API.Client.Services.Account
{
    public class UserClient : ClientBase, IUserClient
    {
        #region Fields
        private readonly string _baseUrl;
        #endregion

        #region Constructors
        public UserClient(
            IConfiguration configuration,
            HttpClient httpClient,
            TokenProvider tokenProvider) : base(configuration, httpClient, tokenProvider)
        {
            _baseUrl = _configuration["Urls:AccountUrl"] + "/api/Account/Users";
        }
        #endregion

        #region Insert
        public async Task<UserDto?> InsertUserAsync(UserRequestDto requestDto)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _baseUrl))
            {
                requestMessage.Content = JsonContent.Create(requestDto);

                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.Created)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<UserDto>();
                    }
                }
            }

            return null;
        }
        #endregion

        #region Update
        public async Task<int> UpdateUserAsync(UserRequestDto requestDto, string id)
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
        public async Task<int> DeleteUserAsync(string id)
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
        public async Task<UserDto?> GetUserAsync(string id, bool? isDeep = null)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, _baseUrl + "/" + id))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        return await responseMessage.Content.ReadFromJsonAsync<UserDto>();
                    }
                }
            }

            return null;
        }
        #endregion

        #region Get List
        public async Task<PagedDto<UserDto>?> GetListUsersAsync(UserFilterDto filterDto)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, UrlUtil.BuildQueryUrl(_baseUrl, filterDto)))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await responseMessage.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<PagedDto<UserDto>>(result);
                    }
                }
            }

            return null;
        }
        #endregion

        #region Get Permissions By UserId
        public async Task<UserDto?> GetPermissionsByUserIdAsync(string userId)
        {
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, _baseUrl + "/GetAllPermissions/" + userId))
            {
                using (HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage))
                {
                    responseMessage.EnsureSuccessStatusCode();

                    if (responseMessage.StatusCode != HttpStatusCode.NoContent)
                    {
                        HashSet<string> permissions = await responseMessage.Content.ReadFromJsonAsync<HashSet<string>>();

                        UserDto userCheck = new UserDto
                        {
                            Permissions = permissions
                        };

                        return userCheck;
                    }
                }
            }

            return null;
        }
        #endregion

    }
}