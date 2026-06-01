using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace Pharmacy_API.Services.Redis
{
    public class RedisCacheService : IDistributedCache
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(HttpClient httpClient, IConfiguration configuration, ILogger<RedisCacheService> logger)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Upstash:RestUrl"] ?? "";
            _token = configuration["Upstash:RestToken"] ?? "";
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        }

        public byte[]? Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/get/{key}", token);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(json);
                    var data = result.RootElement.GetProperty("result").GetString();
                    return data != null ? Encoding.UTF8.GetBytes(data) : null;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis GET error: {ex.Message}");
                return null;
            }
        }

        public void Refresh(string key) { }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            try
            {
                await _httpClient.DeleteAsync($"{_baseUrl}/del/{key}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis DEL error: {ex.Message}");
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            try
            {
                var stringValue = Encoding.UTF8.GetString(value);
                var expiry = options.AbsoluteExpirationRelativeToNow?.TotalSeconds ?? 300;

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("value", stringValue),
                    new KeyValuePair<string, string>("ex", ((int)expiry).ToString()),
                });

                await _httpClient.PostAsync($"{_baseUrl}/set/{key}", content, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis SET error: {ex.Message}");
            }
        }
    }
}