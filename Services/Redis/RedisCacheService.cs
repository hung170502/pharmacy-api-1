using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;

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
            _baseUrl = configuration["Upstash:RestUrl"] ?? "https://honest-bat-74659.upstash.io";
            _token = configuration["Upstash:RestToken"] ?? "";
            _logger = logger;

            // ✅ Set BaseAddress
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            try
            {
                // ✅ Dùng relative URL vì đã có BaseAddress
                var response = await _httpClient.GetAsync($"/get/{key}", token);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(json);
                    var data = result.RootElement.GetProperty("result").GetString();
                    return data != null ? Encoding.UTF8.GetBytes(data) : null;
                }
                _logger.LogWarning($"Redis GET {key}: {response.StatusCode}");
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

        public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            try
            {
                // ✅ Dùng POST /del/{key} thay vì DELETE
                var response = await _httpClient.PostAsync($"/del/{key}", null, token);
                _logger.LogInformation($"Redis DEL {key}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis DEL error: {ex.Message}");
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            => SetAsync(key, value, options).GetAwaiter().GetResult();

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            try
            {
                var stringValue = Encoding.UTF8.GetString(value);
                var expiry = options.AbsoluteExpirationRelativeToNow?.TotalSeconds ?? 300;

                // ✅ Dùng API /set/{key} thay vì pipeline
                var payload = new { value = stringValue, ex = (int)expiry };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/set/{key}", content, token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Redis SET {key}: OK");
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Redis SET failed: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis SET error: {ex.Message}");
            }
        }
    }
}