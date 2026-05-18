using Microsoft.Extensions.Configuration;

namespace Pharmacy_API.Supports
{
    public class ClientBase
    {
        protected readonly IConfiguration _configuration;
        protected readonly HttpClient _httpClient;
        protected readonly TokenProvider _tokenProvider;

        public ClientBase(IConfiguration configuration, HttpClient httpClient, TokenProvider tokenProvider)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;

            if (_tokenProvider != null && _tokenProvider.AccessToken != null)
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", _tokenProvider.AccessToken);
            }
        }

    }
}