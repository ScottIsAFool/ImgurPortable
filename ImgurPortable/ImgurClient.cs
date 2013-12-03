using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ImgurPortable.Entities;
using Newtonsoft.Json;

namespace ImgurPortable
{
    public class ImgurClient
    {
        private const string ImgurApiUrlBase = "https://api.imgur.com/";
        private const string ImgurAuthorisationEndPoint = ImgurApiUrlBase + "oauth2/authorize";
        private const string ImgurAuthorisationTokenEndPoint = "oauth2/token";

        internal readonly HttpClient HttpClient;
        internal readonly HttpClient AnonHttpClient;

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

            ClientId = clientId;
            ClientSecret = clientSecret;

            HttpClient = CreateHttpClient(clientId, handler);
            AnonHttpClient = CreateHttpClient(clientId, handler);
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
        /// Gets the access token from pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The access token object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">pin;A pin must be provided.</exception>
        public async Task<AccessToken> GetAccessTokenFromPinAsync(string pin, CancellationToken cancellationToken = default(CancellationToken))
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

            var response = await PostResponse<AccessToken>(ImgurAuthorisationTokenEndPoint, string.Empty, postData, cancellationToken);

            return response;
        }

        /// <summary>
        /// Gets the access token from code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The access token object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">code;A code must be provided.</exception>
        public async Task<AccessToken> GetAccessTokenFromCodeAsync(string code, CancellationToken cancellationToken = default(CancellationToken))
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

            var response = await PostResponse<AccessToken>(ImgurAuthorisationTokenEndPoint, string.Empty, postData, cancellationToken);

            return response;
        }

        /// <summary>
        /// Refreshes the tokenasync.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The access token object
        /// </returns>
        public async Task<AccessToken> RefreshTokenasync(string refreshToken, CancellationToken cancellationToken = default(CancellationToken))
        {
            var postData = new Dictionary<string, string>
            {
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            };

            var response = await PostResponse<AccessToken>(ImgurAuthorisationTokenEndPoint, string.Empty, postData, cancellationToken);

            return response;
        }

        #endregion

        #region Account

        /// <summary>
        /// Gets the account async.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The account details of the specified user</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<Account> GetUserAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<Account>(endPoint, string.Empty, cancellationToken);
        }

        /// <summary>
        /// Deletes the account async.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if deleted</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<bool> DeleteUserAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await DeleteResponse<bool>(endPoint, string.Empty, cancellationToken);
        }

        /// <summary>
        /// Gets the user gallery favourites async.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The image collection</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<ImageCollection> GetUserGalleryFavouritesAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<ImageCollection>(endPoint, "gallery_favorites", cancellationToken);
        }

        /// <summary>
        /// Gets the account settings.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The account settings</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<AccountSettings> GetUserSettingsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<AccountSettings>(endPoint, "settings", cancellationToken);
        }

        public async Task<bool> ChangeUserSettingsAsync(
            string username,
            string bio = null,
            bool? makeImagesPublic = null,
            bool? allowPrivateMessages = null,
            AlbumPrivacy? albumPrivacy = null,
            bool? acceptGalleryTerms = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var postData = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(bio))
            {
                postData.Add("bio", bio);
            }

            if (makeImagesPublic.HasValue)
            {
                postData.Add("public_images", makeImagesPublic.Value ? "public" : "private");
            }

            if (allowPrivateMessages.HasValue)
            {
                postData.Add("messaging_enabled", allowPrivateMessages.Value.ToString().ToLower());
            }

            if (albumPrivacy.HasValue)
            {
                postData.Add("album_privacy", albumPrivacy.Value.ToString().ToLower());
            }

            if (acceptGalleryTerms.HasValue)
            {
                postData.Add("accepted_gallery_terms", acceptGalleryTerms.Value.ToString().ToLower());
            }

            var endPoint = GetAccountEndPoint(username);

            return await PostResponse<bool>(endPoint, "settings", postData, cancellationToken);
        }

        /// <summary>
        /// Gets the account stats.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The account statistics for the specified user</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<AccountStats> GetUserStatsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<AccountStats>(endPoint, "stats", cancellationToken);
        }

        /// <summary>
        /// Gets the gallery profile.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The gallery profile for the specified user</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<GalleryProfile> GetGalleryProfileAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<GalleryProfile>(endPoint, "gallery_profile", cancellationToken);
        }

        public async Task<bool> GetUserHasVerifiedEmailAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<bool>(endPoint, "verifyemail", cancellationToken);
        }

        /// <summary>
        /// Sends the email verification.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if successful request.</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<bool> SendEmailVerificationAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);
            
            return await PostResponse<bool>(endPoint, "verifyemail", new Dictionary<string, string>(), cancellationToken);
        }

        /// <summary>
        /// Gets all user albums.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>All the user's albums</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<AlbumCollection> GetUserAlbumsAsync(string username, int? pageNumber = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var method = "albums";
            if (pageNumber.HasValue)
            {
                method += "/" + pageNumber.Value;
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<AlbumCollection>(endPoint, method, cancellationToken);
        }

        public async Task<List<string>> GetUserAlbumIdsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<List<string>>(endPoint, "albums/ids", cancellationToken);
        }

        public async Task<int> GetUserAlbumCountAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<int>(endPoint, "albums/count", cancellationToken);
        }

        public async Task<CommentCollection> GetUserCommentsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<CommentCollection>(endPoint, "comments", cancellationToken);
        }

        public async Task<List<string>> GetUserCommentIdsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<List<string>>(endPoint, "comments/ids", cancellationToken);
        }

        public async Task<int> GetUserCommentCountAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<int>(endPoint, "comments/count", cancellationToken);
        }

        public async Task<CommentCollection> GetUserImagesAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<CommentCollection>(endPoint, "images", cancellationToken);
        }

        public async Task<List<string>> GetUserImageIdsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<List<string>>(endPoint, "images/ids", cancellationToken);
        }

        public async Task<int> GetUserImageCountAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<int>(endPoint, "images/count", cancellationToken);
        }

        public async Task<Notification> GetUserRepliesAsync(string username, bool? onlyUnread = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var method = "notifications/replies";
            if (onlyUnread.HasValue && !onlyUnread.Value)
            {
                method += "?new=false";
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<Notification>(endPoint, method, cancellationToken);
        }

        #endregion
        
        private async Task<TResponseType> PostResponse<TResponseType>(string endPoint, string method, Dictionary<string, string> postData, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = string.Format("{0}{1}/{2}", ImgurApiUrlBase, endPoint, method);
            var response = await HttpClient.PostAsync(url, new FormUrlEncodedContent(postData), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new NotImplementedException();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            if (responseString.Contains("\"success\": false"))
            {
                var error = JsonConvert.DeserializeObject<ImgurResponse<Error>>(responseString);
                throw new ImgurException(error);
            }

            var item = JsonConvert.DeserializeObject<TResponseType>(responseString);

            return item;
        }

        private async Task<TResponseType> GetResponse<TResponseType>(string endPoint, string method, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = string.Format("{0}{1}/{2}", ImgurApiUrlBase, endPoint, method);

            var response = await HttpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new NotImplementedException();
            }
            
            var responseString = await response.Content.ReadAsStringAsync();

            if (responseString.Contains("\"success\": false"))
            {
                var error = JsonConvert.DeserializeObject<ImgurResponse<Error>>(responseString);
                throw new ImgurException(error);
            }

            var item = JsonConvert.DeserializeObject<TResponseType>(responseString);
            return item;
        }

        private async Task<TResponseType> DeleteResponse<TResponseType>(string endPoint, string method, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = string.Format("{0}{1}/{2}", ImgurApiUrlBase, endPoint, method);

            var response = await HttpClient.DeleteAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new NotImplementedException();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            if (responseString.Contains("\"success\": false"))
            {
                var error = JsonConvert.DeserializeObject<ImgurResponse<Error>>(responseString);
                throw new ImgurException(error);
            }

            var item = JsonConvert.DeserializeObject<TResponseType>(responseString);
            return item;
        }

        private static string GetAccountEndPoint(string username)
        {
            return string.Format("3/account/{0}", username);
        }

        private static string GetAlbumEndPoint(string albumId)
        {
            return string.Format("3/album/{0}", albumId);
        }

        private static HttpClient CreateHttpClient(string clientId, HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler ?? new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);

            return httpClient;
        }
    }
}
