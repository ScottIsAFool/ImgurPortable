﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
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

        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            var url = App.ImgurClient.GetAuthenticationUrl(AuthResponseType.Token);
            new WebBrowserTask { Uri = new Uri(url, UriKind.Absolute) }.Show();
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
            var images = await App.ImgurClient.GetGalleryAsync(GallerySection.Hot, Sort.Time, dateRange: DateRange.Day);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("AccessToken"))
            {
                var token = (AccessToken)settings["AccessToken"];
                if (DateTime.Now < token.ExpiryDateTime)
                {
                    if (App.ImgurClient != null && string.IsNullOrEmpty(App.ImgurClient.AccessToken))
                    {
                        App.ImgurClient.AddAccessToken(token.Token);
                        //LoginButton.IsEnabled = false;
                    }
                }
                else
                {
                    try
                    {
                        var refreshToken = await App.ImgurClient.GetRefreshTokenAsync(token.RefreshToken);
                        settings["AccessToken"] = refreshToken;
                        settings.Save();

                        App.ImgurClient.AddAccessToken(refreshToken.Token);
                    }
                    catch { }
                }
            }
        }

        private void PhotoButton_OnClick(object sender, RoutedEventArgs e)
        {
            var chooser = new PhotoChooserTask();
            chooser.Completed += ChooserOnCompleted;
            chooser.Show();
        }

        private async void ChooserOnCompleted(object sender, PhotoResult photoResult)
        {
            if (photoResult.Error == null && photoResult.ChosenPhoto != null)
            {
                var image = await App.ImgurClient.UploadImageAsync(photoResult.ChosenPhoto, name: "halp");
            }
        }
    }
}