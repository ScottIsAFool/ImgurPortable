using System.Net.Http.Headers;

namespace ImgurPortable.Extensions
{
    public static class ImgurClientExtensions
    {
        /// <summary>
        /// Adds the access token.
        /// </summary>
        /// <param name="imgurClient">The imgur client.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>An updated ImgurClient</returns>
        public static ImgurClient AddAccessToken(this ImgurClient imgurClient, string accessToken)
        {
            imgurClient.AccessToken = accessToken;
            imgurClient._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return imgurClient;
        }
    }
}
