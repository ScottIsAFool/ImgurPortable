using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ImgurPortable.Entities;
using Newtonsoft.Json;

namespace ImgurPortable
{
    public partial class ImgurClient
    {
        private const string ImgurApiUrlBase = "https://api.imgur.com/";
        private const string ImgurAuthorisationEndPoint = ImgurApiUrlBase + "oauth2/authorize";
        private const string ImgurAuthorisationTokenEndPoint = ImgurApiUrlBase + "oauth2/token";

        internal readonly HttpClient _httpClient;

        public ImgurClient(string clientId, string clientSecret)
            : this(clientId, clientSecret, null)
        { }

        public ImgurClient(string clientId, string clientSecret, HttpMessageHandler handler)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException("clientId", "Client ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException("clientSecret", "Client secret cannot be null or empty");
            }

            _httpClient = handler == null
                ? _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
                : _httpClient = new HttpClient(handler);

            ClientId = clientId;
            ClientSecret = clientSecret;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", ClientId);
        }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; private set; }

        /// <summary>
        /// Gets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public string ClientSecret { get; private set; }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; internal set; }

        #region Authorisation
        /// <summary>
        /// Gets the authentication URL.
        /// </summary>
        /// <param name="authResponseType">Type of the authentication response.</param>
        /// <param name="state">The state.</param>
        /// <returns>The authentication URL</returns>
        public string GetAuthenticationUrl(AuthResponseType authResponseType, string state = null)
        {
            var url = string.Format("{0}?response_type={1}&client_id={2}", ImgurAuthorisationEndPoint, authResponseType.ToString().ToLower(), ClientId);

            if (!string.IsNullOrEmpty(state))
            {
                url += string.Format("&state={0}", state);
            }

            return url;
        }

        /// <summary>
        /// Gets the access token from pin asynchronous.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <returns>The access token object</returns>
        /// <exception cref="System.ArgumentNullException">pin;A pin must be provided.</exception>
        public async Task<AccessToken> GetAccessTokenFromPinAsync(string pin)
        {
            if (string.IsNullOrEmpty(pin))
            {
                throw new ArgumentNullException("pin", "A pin must be provided.");
            }

            var postData = new Dictionary<string, string>
            {
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"grant_type", "pin"},
                {"pin", pin}
            };

            var response = await GetPostResponse<AccessToken>("oauth2/token", postData);

            return response;
        }

        /// <summary>
        /// Gets the access token from code asynchronous.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The access token object</returns>
        /// <exception cref="System.ArgumentNullException">code;A code must be provided.</exception>
        public async Task<AccessToken> GetAccessTokenFromCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "A code must be provided.");
            }

            var postData = new Dictionary<string, string>
            {
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"grant_type", "code"},
                {"code", code}
            };

            var response = await GetPostResponse<AccessToken>("oauth2/token", postData);

            return response;
        }

        /// <summary>
        /// Refreshes the tokenasync.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The access token object</returns>
        public async Task<AccessToken> RefreshTokenasync(string refreshToken)
        {
            var postData = new Dictionary<string, string>
            {
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            };

            var response = await GetPostResponse<AccessToken>("oauth2/token", postData);

            return response;
        }

        #endregion

        public async Task<ImageCollection> GetUserImagesAsync(string username)
        {
            var method = string.Format("3/account/{0}/images/", username);

            var response = await GetResponse<ImgurResponse<ImageCollection>>(method);

            return response.Response;
        }

        private async Task<TResponseType> GetPostResponse<TResponseType>(string method, Dictionary<string, string> postData)
        {
            var url = string.Format("{0}{1}", ImgurApiUrlBase, method);
            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(postData));

            if (!response.IsSuccessStatusCode)
            {
                throw new NotImplementedException();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var item = JsonConvert.DeserializeObject<TResponseType>(responseString);

            return item;
        }

        private async Task<TResponseType> GetResponse<TResponseType>(string method)
        {
            var url = string.Format("{0}{1}", ImgurApiUrlBase, method);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new NotImplementedException();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<TResponseType>(responseString);
            return item;
        }
    }
}
