using System;
using System.Windows;
using ImgurPortable.Entities;
using ImgurPortable.Extensions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace ImgurPortablePlayground
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            
        }

        //0a453bcc555c72a
        //990617fc34de37b8e4641e0b12bc9df8867dfad8
        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            var url = App.ImgurClient.GetAuthenticationUrl(AuthResponseType.Token);
            new WebBrowserTask{Uri = new Uri(url, UriKind.Absolute)}.Show();
        }

        private async void PinButton_OnClick(object sender, RoutedEventArgs e)
        {
            var pin = string.Empty;
            var token = await App.ImgurClient.GetAccessTokenFromPinAsync(pin);
            App.ImgurClient.AddAccessToken(token.Token);
        }

        private async void ImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            //var images = await App.ImgurClient.GetUserImagesAsync(App.AccessToken.AccountUsername);
            await App.ImgurClient.GetAccountAsync(App.AccessToken.AccountUsername);
        }
    }
}