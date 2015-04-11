using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace VK_Music
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ErrorPage : Page
    {
        public ErrorPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string parameter = e.Parameter as string;
            base.OnNavigatedTo(e);
            Repeat_Validation(parameter);
        }
        async private void Repeat_Validation(string redirect_uri)
        {
            var requestUri = new Uri(redirect_uri);
            var callbackUri = new Uri("https://oauth.vk.com/blank.html");

            WebAuthenticationResult webAuthResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                    WebAuthenticationOptions.None,
                                                    requestUri,
                                                    callbackUri);

            if (webAuthResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var responseString = webAuthResult.ResponseData.ToString();
                char[] separators = { '=', '&' };
                string[] responseContent = responseString.Split(separators);
                string AccessToken = responseContent[3];
                int UserId = Int32.Parse(responseContent[5]);
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["_token"] = "" + AccessToken + "";
                this.Frame.Navigate(typeof(MainPage), AccessToken);

            }

        }
    }
}
