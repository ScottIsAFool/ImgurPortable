using System;
using System.Collections.Generic;
using ImgurPortable.Entities;

namespace ImgurPortable.Extensions
{
    public static class UriExtensions
    {
        public static bool IsFromImgur(this Uri uri)
        {
            var url = uri.CallbackUrl();
            return url.Contains("#access_token=") || url.Contains("error=access_denied") || url.Contains("code=");
        }

        public static bool IsError(this Uri uri)
        {
            return uri.ToString().Contains("error=access_denied");
        }

        public static string GetAuthCode(this Uri uri)
        {
            var url = uri.CallbackUrl();
            var dict = new Uri(url, UriKind.Absolute).Query.ParseQueryString();

            return dict["code"];
        }

        public static AccessToken GetAccessTokenInfo(this Uri uri)
        {
            var url = uri.CallbackUrl();
            // imgurportable:///#access_token=d13f9247852aa21da390e2577d0089de93ad6532&expires_in=3600&token_type=bearer&refresh_token=08c8e8b3947d32928b822781c54cfb8192e97860&account_username=scottisafool
            var dict = new Uri(url.Replace("#", "?"), UriKind.Absolute).Query.ParseQueryString();

            var token = new AccessToken
            {
                AccountUsername = dict["account_username"],
                ExpiresIn = int.Parse(dict["expires_in"]),
                RefreshToken = dict["refresh_token"],
                Token = dict["access_token"],
                TokenType = dict["token_type"],
                ExpiryDateTime = DateTime.Now.AddHours(1)
            };

            return token;
        }

        private static string CallbackUrl(this Uri uri)
        {
            var url = uri.ToString();
            url = url.Replace("/Protocol?encodedLaunchUri=", string.Empty);
            url = Uri.UnescapeDataString(url);

            return url;
        }

        private static Dictionary<string, string> ParseQueryString(this string query)
        {
            var queryDict = new Dictionary<string, string>();
            foreach (String token in query.TrimStart(new char[] { '?' }).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    queryDict[parts[0].Trim()] = Uri.UnescapeDataString(parts[1]).Trim();
                else
                    queryDict[parts[0].Trim()] = "";
            }
            return queryDict;
        }
    }
}
