using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ImgurPortable.Entities;
using ImgurPortable.Extensions;
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
            var url = string.Format("{0}?response_type={1}&client_id={2}", ImgurAuthorisationEndPoint, authResponseType.ToLower(), ClientId);

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

            var response = await PostResponse<AccessToken>(ImgurAuthorisationTokenEndPoint, string.Empty, postData, HttpClient, cancellationToken);

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

            var response = await PostResponse<AccessToken>(ImgurAuthorisationTokenEndPoint, string.Empty, postData, HttpClient, cancellationToken);

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

            var response = await PostResponse<AccessToken>(ImgurAuthorisationTokenEndPoint, string.Empty, postData, HttpClient, cancellationToken);

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

            return await GetResponse<Account>(endPoint, string.Empty, HttpClient, cancellationToken);
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

            return await DeleteResponse<bool>(endPoint, string.Empty, HttpClient, cancellationToken);
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

            return await GetResponse<ImageCollection>(endPoint, "gallery_favorites", HttpClient, cancellationToken);
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

            return await GetResponse<AccountSettings>(endPoint, "settings", HttpClient, cancellationToken);
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
                postData.Add("messaging_enabled", allowPrivateMessages.Value.ToLower());
            }

            if (albumPrivacy.HasValue)
            {
                postData.Add("album_privacy", albumPrivacy.Value.ToLower());
            }

            if (acceptGalleryTerms.HasValue)
            {
                postData.Add("accepted_gallery_terms", acceptGalleryTerms.Value.ToLower());
            }

            var endPoint = GetAccountEndPoint(username);

            return await PostResponse<bool>(endPoint, "settings", postData, HttpClient, cancellationToken);
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

            return await GetResponse<AccountStats>(endPoint, "stats", HttpClient, cancellationToken);
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

            return await GetResponse<GalleryProfile>(endPoint, "gallery_profile", HttpClient, cancellationToken);
        }

        public async Task<bool> GetUserHasVerifiedEmailAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<bool>(endPoint, "verifyemail", HttpClient, cancellationToken);
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

            return await PostResponse<bool>(endPoint, "verifyemail", new Dictionary<string, string>(), HttpClient, cancellationToken);
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

            return await GetResponse<AlbumCollection>(endPoint, method, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user album ids.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The album ids</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<List<string>> GetUserAlbumIdsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<List<string>>(endPoint, "albums/ids", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user album count.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of albums</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<int> GetUserAlbumCountAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<int>(endPoint, "albums/count", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user comments.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The comments that user has made</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<CommentCollection> GetUserCommentsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<CommentCollection>(endPoint, "comments", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user comment ids.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The commend Ids for that user</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<List<string>> GetUserCommentIdsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<List<string>>(endPoint, "comments/ids", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user comment count.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of comments that user has made</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<int> GetUserCommentCountAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<int>(endPoint, "comments/count", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user images.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The images that user has uploaded</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<CommentCollection> GetUserImagesAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<CommentCollection>(endPoint, "images", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user image ids.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The image ids for that user</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<List<string>> GetUserImageIdsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<List<string>>(endPoint, "images/ids", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user image count.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of images for that user</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
        public async Task<int> GetUserImageCountAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username", "Username cannot be null or empty");
            }

            var endPoint = GetAccountEndPoint(username);

            return await GetResponse<int>(endPoint, "images/count", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the user replies.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="onlyUnread">The only unread.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The replies that user has submitted</returns>
        /// <exception cref="System.ArgumentNullException">username;Username cannot be null or empty</exception>
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

            return await GetResponse<Notification>(endPoint, method, HttpClient, cancellationToken);
        }

        #endregion

        #region Album

        /// <summary>
        /// Gets the album.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The specified album</returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<Album> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var endPoint = GetAlbumEndPoint(albumId);

            return await GetResponse<Album>(endPoint, string.Empty, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the album images.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<ImageCollection> GetAlbumImagesAsync(string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var endPoint = GetAlbumEndPoint(albumId);

            return await GetResponse<ImageCollection>(endPoint, "images", HttpClient, cancellationToken);
        }

        /// <summary>
        /// Gets the album image.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// albumId;Album ID cannot be null or empty
        /// or
        /// imageId;Image ID cannot be null or empty
        /// </exception>
        public async Task<ImageCollection> GetAlbumImageAsync(string albumId, string imageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentNullException("imageId", "Image ID cannot be null or empty");
            }

            var endPoint = GetAlbumEndPoint(albumId);
            var method = string.Format("image/{0}", imageId);

            return await GetResponse<ImageCollection>(endPoint, method, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Creates the new album.
        /// </summary>
        /// <param name="images">The images.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="privacy">The privacy.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="coverImageId">The cover image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Album> CreateNewAlbumAsync(
            ImageCollection images = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CreateNewAlbumAsync(images == null ? null : images.Select(x => x.Id), title, description, privacy, layout, coverImageId, cancellationToken);
        }

        /// <summary>
        /// Creates the new album.
        /// </summary>
        /// <param name="imageIds">The image ids.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="privacy">The privacy.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="coverImageId">The cover image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Album> CreateNewAlbumAsync(
            IEnumerable<string> imageIds = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CreateAlbumInternal(string.Empty, imageIds, title, description, privacy, layout, coverImageId, HttpClient, cancellationToken);
        }

        public async Task<Album> CreateNewAnonymousAlbumAsync(
            ImageCollection images,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CreateNewAnonymousAlbumAsync(images == null ? null : images.Select(x => x.Id), title, description, privacy, layout, coverImageId, cancellationToken);
        }

        public async Task<Album> CreateNewAnonymousAlbumAsync(
            IEnumerable<string> imageIds = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CreateAlbumInternal(string.Empty, imageIds, title, description, privacy, layout, coverImageId, AnonHttpClient, cancellationToken);
        }

        /// <summary>
        /// Updates the album.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="images">The images.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="privacy">The privacy.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="coverImageId">The cover image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Album> UpdateAlbumAsync(
            string albumId,
            ImageCollection images = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UpdateAlbumAsync(albumId, images == null ? null : images.Select(x => x.Id), title, description, privacy, layout, coverImageId, cancellationToken);
        }

        /// <summary>
        /// Updates the album.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="imageIds">The image ids.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="privacy">The privacy.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="coverImageId">The cover image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<Album> UpdateAlbumAsync(
            string albumId,
            IEnumerable<string> imageIds = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            return await CreateAlbumInternal(albumId, imageIds, title, description, privacy, layout, coverImageId, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Updates the album.
        /// </summary>
        /// <param name="deleteHash">The delete hash.</param>
        /// <param name="images">The images.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="privacy">The privacy.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="coverImageId">The cover image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Album> UpdateAnonymousAlbumAsync(
            string deleteHash,
            ImageCollection images = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UpdateAlbumAsync(deleteHash, images == null ? null : images.Select(x => x.Id), title, description, privacy, layout, coverImageId, cancellationToken);
        }

        /// <summary>
        /// Updates the album.
        /// </summary>
        /// <param name="deleteHash">The delete hash.</param>
        /// <param name="imageIds">The image ids.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="privacy">The privacy.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="coverImageId">The cover image identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Album> UpdateAnonymousAlbumAsync(
            string deleteHash,
            IEnumerable<string> imageIds = null,
            string title = null,
            string description = null,
            AlbumPrivacy? privacy = null,
            Layout? layout = null,
            string coverImageId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await CreateAlbumInternal(deleteHash, imageIds, title, description, privacy, layout, coverImageId, AnonHttpClient, cancellationToken);
        }

        /// <summary>
        /// Deletes the album.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> DeleteAlbumAsync(string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var endPoint = GetAlbumEndPoint(albumId);

            return await DeleteResponse<bool>(endPoint, string.Empty, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Favourites the album.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the album is favourited, false if unfavourited</returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> FavouriteAlbumAsync(string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var endPoint = GetAlbumEndPoint(albumId);

            var response = await PostResponse<string>(endPoint, "favorite", new Dictionary<string, string>(), HttpClient, cancellationToken);

            return response != "unfavorited";
        }

        /// <summary>
        /// Sets the album images.
        /// NOTE: Sets the images for an album, removes all other images and only uses the images in this request
        /// </summary>
        /// <param name="images">The images.</param>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if images set successfully</returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> SetAlbumImagesAsync(ImageCollection images, string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            return await SetAlbumImagesAsync(images.Select(x => x.Id), albumId, cancellationToken);
        }

        /// <summary>
        /// Sets the album images.
        /// NOTE: Sets the images for an album, removes all other images and only uses the images in this request
        /// </summary>
        /// <param name="imageIds">The image ids.</param>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if images set successfully</returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> SetAlbumImagesAsync(IEnumerable<string> imageIds, string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var ids = imageIds.ToCommaSeparated();
            var postData = new Dictionary<string, string>
            {
                {"ids", ids}
            };

            var endPoint = GetAlbumEndPoint(albumId);

            return await PostResponse<bool>(endPoint, string.Empty, postData, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Adds the images to album.
        /// </summary>
        /// <param name="images">The images.</param>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if images added successfully</returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> AddImagesToAlbumAsync(ImageCollection images, string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            return await AddImagesToAlbumAsync(images.Select(x => x.Id), albumId, cancellationToken);
        }

        /// <summary>
        /// Adds the images to the album.
        /// </summary>
        /// <param name="imageIds">The image ids.</param>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if images added successfully</returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> AddImagesToAlbumAsync(IEnumerable<string> imageIds, string albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var ids = imageIds.ToCommaSeparated();
            var postData = new Dictionary<string, string>
            {
                {"ids", ids}
            };

            var endPoint = GetAlbumEndPoint(albumId);

            return await PostResponse<bool>(endPoint, "add", postData, HttpClient, cancellationToken);
        }

        /// <summary>
        /// Removes the images from album.
        /// </summary>
        /// <param name="images">The images.</param>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> RemoveImagesFromAlbumAsync(ImageCollection images, string albumId, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            return await RemoveImagesFromAlbumAsync(images == null ? null : images.Select(x => x.Id), albumId, cancellationToken);
        }

        /// <summary>
        /// Removes the images from album.
        /// </summary>
        /// <param name="imageIds">The image ids.</param>
        /// <param name="albumId">The album identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">albumId;Album ID cannot be null or empty</exception>
        public async Task<bool> RemoveImagesFromAlbumAsync(IEnumerable<string> imageIds, string albumId, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (string.IsNullOrEmpty(albumId))
            {
                throw new ArgumentNullException("albumId", "Album ID cannot be null or empty");
            }

            var ids = imageIds.ToCommaSeparated();
            var endPoint = GetAlbumEndPoint(albumId);

            var method = string.Format("remove_images?ids={0}", ids);

            return await DeleteResponse<bool>(endPoint, method, HttpClient, cancellationToken);
        }

        #endregion

        #region Comments
        public async Task<Comment> GetCommentAsync(string commentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(commentId))
            {
                throw new ArgumentNullException("commentId", "Comment ID cannot be null or empty");
            }

            var endPoint = GetCommentEndPoint(commentId);

            return await GetResponse<Comment>(endPoint, String.Empty, HttpClient, cancellationToken);
        }

        public async Task<Comment> CreateCommentAsync(string imageId, string comment, string parentId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentNullException("imageId", "Image ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(comment))
            {
                throw new ArgumentNullException("comment", "Comment cannot be null or empty");
            }

            var postData = new Dictionary<string, string>
            {
                {"image_id", imageId},
                {"comment", comment}
            };

            if (!string.IsNullOrEmpty(parentId))
            {
                postData.Add("parent_id", parentId);
            }

            var endPoint = GetCommentEndPoint(string.Empty);

            var response = await PostResponse<Comment>(endPoint, string.Empty, postData, HttpClient, cancellationToken);

            return await GetCommentAsync(response.Id, cancellationToken);
        }

        public async Task<bool> DeleteCommentAsync(string commentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(commentId))
            {
                throw new ArgumentNullException("commentId", "Comment ID cannot be null or empty");
            }

            var endPoint = GetCommentEndPoint(commentId);

            return await DeleteResponse<bool>(endPoint, string.Empty, HttpClient, cancellationToken);
        }

        public async Task<CommentCollection> GetCommentRepliesAsync(string commentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(commentId))
            {
                throw new ArgumentNullException("commentId", "Comment ID cannot be null or empty");
            }

            var endPoint = GetCommentEndPoint(commentId);

            return await GetResponse<CommentCollection>(endPoint, "replies", HttpClient, cancellationToken);
        }

        public async Task<bool> VoteOnCommentAsync(string commentId, Vote vote, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(commentId))
            {
                throw new ArgumentNullException("commentId", "Comment ID cannot be null or empty");
            }

            var endPoint = GetCommentEndPoint(commentId);
            var method = string.Format("vote/{0}", vote.ToLower());

            return await PostResponse<bool>(endPoint, method, new Dictionary<string, string>(), HttpClient, cancellationToken);
        }

        public async Task<bool> ReportCommentAsync(string commentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(commentId))
            {
                throw new ArgumentNullException("commentId", "Comment ID cannot be null or empty");
            }

            var endPoint = GetCommentEndPoint(commentId);

            return await PostResponse<bool>(endPoint, "report", new Dictionary<string, string>(), HttpClient, cancellationToken);
        }

        public async Task<bool> AddReplyToCommentAsync(string commentId, string imageId, string comment, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(commentId))
            {
                throw new ArgumentNullException("commentId", "Comment ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentNullException("imageId", "Image ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(comment))
            {
                throw new ArgumentNullException("comment", "Comment cannot be null or empty");
            }

            var endPoint = GetCommentEndPoint(commentId);
            var postData = new Dictionary<string, string>
            {
                { "image_id", imageId },
                { "comment", comment }
            };

            return await PostResponse<bool>(endPoint, string.Empty, postData, HttpClient, cancellationToken);
        }
        #endregion

        #region Internal Common Methods
        internal async Task<Album> CreateAlbumInternal(string albumId, IEnumerable<string> imageIds, string title, string description, AlbumPrivacy? privacy, Layout? layout, string coverImageId, HttpClient httpClient, CancellationToken cancellationToken)
        {
            var postData = new Dictionary<string, string>();

            if (imageIds != null)
            {
                var ids = imageIds.ToCommaSeparated();
                postData.Add("ids", ids);
            }

            if (!string.IsNullOrEmpty(title))
            {
                postData.Add("title", title);
            }

            if (!string.IsNullOrEmpty(description))
            {
                postData.Add("description", description);
            }

            if (privacy.HasValue)
            {
                postData.Add("privacy", privacy.Value.ToLower());
            }

            if (layout.HasValue)
            {
                postData.Add("layout", layout.Value.ToLower());
            }

            if (!string.IsNullOrEmpty(coverImageId))
            {
                postData.Add("cover", coverImageId);
            }

            var endPoint = GetAlbumEndPoint(albumId);

            var album = await PostResponse<Album>(endPoint, string.Empty, postData, httpClient, cancellationToken);

            return await GetAlbumAsync(album.Id, cancellationToken);
        }
        #endregion

        private static async Task<TResponseType> PostResponse<TResponseType>(string endPoint, string method, Dictionary<string, string> postData, HttpClient httpClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = string.Format("{0}{1}/{2}", ImgurApiUrlBase, endPoint, method);
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(postData), cancellationToken);

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

            var item = JsonConvert.DeserializeObject<ImgurResponse<TResponseType>>(responseString);

            return item.Response;
        }

        private static async Task<TResponseType> GetResponse<TResponseType>(string endPoint, string method, HttpClient httpClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = string.Format("{0}{1}/{2}", ImgurApiUrlBase, endPoint, method);

            var response = await httpClient.GetAsync(url, cancellationToken);

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

            var item = JsonConvert.DeserializeObject<ImgurResponse<TResponseType>>(responseString);
            return item.Response;
        }

        private static async Task<TResponseType> DeleteResponse<TResponseType>(string endPoint, string method, HttpClient httpClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = string.Format("{0}{1}/{2}", ImgurApiUrlBase, endPoint, method);

            var response = await httpClient.DeleteAsync(url, cancellationToken);

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

            var item = JsonConvert.DeserializeObject<ImgurResponse<TResponseType>>(responseString);
            return item.Response;
        }

        private static string GetAccountEndPoint(string username)
        {
            return string.Format("3/account/{0}", username);
        }

        private static string GetAlbumEndPoint(string albumId)
        {
            return string.Format("3/album/{0}", albumId);
        }

        private static string GetCommentEndPoint(string commentId)
        {
            return string.Format("3/comment/{0}", commentId);
        }

        private static string GetGalleryEndPoint(string section)
        {
            return string.Format("3/gallery/{0}", section);
        }

        private static HttpClient CreateHttpClient(string clientId, HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler ?? new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);

            return httpClient;
        }
    }
}
