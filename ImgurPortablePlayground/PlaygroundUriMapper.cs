using System;
using System.Windows.Navigation;
using ImgurPortable.Extensions;

namespace ImgurPortablePlayground
{
    public class PlaygroundUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            if (uri.ToString().Contains("imgurportable"))
            {
                var token = uri.GetAccessTokenInfo();
                App.AccessToken = token;
                App.ImgurClient.AddAccessToken(token.Token);
                return new Uri("/MainPage.xaml", UriKind.Relative);
            }

            return uri;
        }
    }
}
