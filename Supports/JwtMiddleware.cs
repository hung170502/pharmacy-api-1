using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Pharmacy_API.Supports
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _distributedCache;

        public JwtMiddleware(RequestDelegate next, IDistributedCache distributedCache)
        {
            _next = next;
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Middleware for handling JWT tokens.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context, ILogger<JwtMiddleware> logger)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?
                .Split(" ").LastOrDefault();

            if (!string.IsNullOrWhiteSpace(token))
            {
                var username = context.User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrWhiteSpace(username))
                {
                    var refreshTokenCache = await _distributedCache.GetStringAsync(username);

                    // ✅ Parse JSON từ Redis nếu cần
                    if (!string.IsNullOrWhiteSpace(refreshTokenCache))
                    {
                        try
                        {
                            // Thử parse JSON {"value":"..."}
                            var json = System.Text.Json.JsonDocument.Parse(refreshTokenCache);
                            if (json.RootElement.TryGetProperty("value", out var valueProp))
                            {
                                refreshTokenCache = valueProp.GetString();
                            }
                        }
                        catch { /* Không phải JSON, dùng giá trị gốc */ }
                    }

                    // So sánh token
                    if (string.IsNullOrWhiteSpace(refreshTokenCache) || token != refreshTokenCache)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}