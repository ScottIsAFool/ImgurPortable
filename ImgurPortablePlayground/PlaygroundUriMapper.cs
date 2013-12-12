using System;
using System.IO.IsolatedStorage;
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

                var settings = IsolatedStorageSettings.ApplicationSettings;
                settings["AccessToken"] = token;
                settings.Save();

                App.ImgurClient.AddAccessToken(token.Token);
                return new Uri("/MainPage.xaml", UriKind.Relative);
            }

            return uri;
        }
    }
}
