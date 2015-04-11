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


namespace VK_Music
{
    public sealed partial class Auth : Page
    {
        #region Parametrs
        int client_id = 3870627;
        int scope = 327690;
        #endregion
        public Auth()
        {
            this.InitializeComponent();
            auth();
        }

        async private void auth()
        {
            var VkUrl = "https://oauth.vk.com/authorize?client_id=" + client_id + "&scope=" + scope + "&redirect_uri=http://oauth.vk.com/blank.html&display=touch&response_type=token";
            var requestUri = new Uri(VkUrl);
            var callbackUri = new Uri("http://oauth.vk.com/blank.html");

            WebAuthenticationResult webAuthResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                    WebAuthenticationOptions.None,
                                                    requestUri,
                                                    callbackUri);
            if (webAuthResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var responseString = webAuthResult.ResponseData.ToString();
                char[] separators = { '=', '&' };
                string[] responseContent = responseString.Split(separators);
                string AccessToken = responseContent[1];
                int UserId = Int32.Parse(responseContent[5]);
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["_token"] = "" + AccessToken + "";
                this.Frame.Navigate(typeof(MainPage), AccessToken);

            }

        }
    }
}
