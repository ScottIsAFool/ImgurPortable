using Newtonsoft.Json;

namespace ImgurPortable.Entities
{
    public class AccessToken
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("account_username")]
        public string AccountUsername { get; set; }
    }
}
