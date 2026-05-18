using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Dtos.Account;

namespace Pharmacy_API.Services.Account
{
    public class JwtAuthManagerService : IJwtAuthManagerService
    {
        #region properties
        public UserManager<ApplicationUser> _userManager { get; }
        private readonly byte[] _secret;
        private readonly AppSettings _appSettings;
        private readonly IUpdateUserService _userService;
        #endregion

        public JwtAuthManagerService(IOptions<AppSettings> appSettings,
            UserManager<ApplicationUser> userManager,
            IUpdateUserService userService)
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _secret = Encoding.ASCII.GetBytes(_appSettings.Jwt.Key);
            _userService = userService;
        }

        #region Functions
        /// <summary>
        /// Generates access and refresh tokens for a given user.
        /// </summary>
        /// <param name="user">The user for whom to generate tokens.</param>
        /// <param name="claims">The claims for the generated tokens.</param>
        /// <param name="now">The current date and time.</param>
        /// <returns>An instance of <see cref="JwtAuthResultDto"/> containing the generated tokens.</returns>
        public async Task<JwtAuthResultDto> GenerateTokens(ApplicationUser user, IEnumerable<Claim> claims, DateTime now)
        {
            var shouldAddAudienceClaim = string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);
            var jwtToken = new JwtSecurityToken(
                issuer: _appSettings.Jwt.Issuer,
                audience: shouldAddAudienceClaim ? _appSettings.Jwt.Audience : string.Empty,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_appSettings.Jwt.AccessTokenExpiryInMinutes),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256Signature));

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = await _userManager.GenerateUserTokenAsync(user, _appSettings.Jwt.AppName, _appSettings.Jwt.RefreshTokenName);

            return new JwtAuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Retrieves the claims associated with a given user.
        /// </summary>
        /// <param name="user">The user for whom to retrieve claims.</param>
        /// <returns>An enumerable collection of <see cref="Claim"/> objects representing the user's claims.</returns>
        public async Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user)
        {
            var scopes = await _userService.GetUserPermissionsAsync(user.Id);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName?.Trim() ?? string.Empty, ClaimValueTypes.String),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty, ClaimValueTypes.String),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String),
                new Claim("scope", string.Join(" ", scopes))
            };

            return claims;
        }
        #endregion

    }
}

