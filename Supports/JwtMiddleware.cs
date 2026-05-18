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
            //logic here
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?
                .Split(" ").LastOrDefault();

            if (!string.IsNullOrWhiteSpace(token))
            {
                var username = context.User.FindFirstValue(ClaimTypes.Email);
                string? refreshTokenCache = null;
                if (!string.IsNullOrWhiteSpace(username))
                    refreshTokenCache = await _distributedCache.GetStringAsync(username);

                if (string.IsNullOrWhiteSpace(refreshTokenCache) || token != refreshTokenCache)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }

            await _next(context);
        }
    }
}