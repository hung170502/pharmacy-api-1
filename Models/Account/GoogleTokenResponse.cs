using Newtonsoft.Json;

namespace Pharmacy_API.Models.Account
{
    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public long? ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }
    }
}
