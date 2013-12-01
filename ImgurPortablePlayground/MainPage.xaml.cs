using System;
using System.Windows;
using ImgurPortable;
using ImgurPortable.Entities;
using ImgurPortable.Extensions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace ImgurPortablePlayground
{
    public partial class MainPage : PhoneApplicationPage
    {
        private readonly ImgurClient _client;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            _client = new ImgurClient("0a453bcc555c72a", "990617fc34de37b8e4641e0b12bc9df8867dfad8");
        }

        //0a453bcc555c72a
        //990617fc34de37b8e4641e0b12bc9df8867dfad8
        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            var url = _client.GetAuthenticationUrl(AuthResponseType.Pin);
            new WebBrowserTask{Uri = new Uri(url, UriKind.Absolute)}.Show();
        }

        private async void PinButton_OnClick(object sender, RoutedEventArgs e)
        {
            var pin = string.Empty;
            var token = await _client.GetAccessTokenFromPinAsync(pin);
            _client.AddAccessToken(token.Token);
        }
    }
}