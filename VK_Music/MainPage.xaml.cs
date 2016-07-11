using IncrementalLoading;
using IncrementalLoading.Code;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_Music.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VK_Music
{
    class TimespanToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parametr, string str)
        {
            try
            {
                var time = (TimeSpan)value;

                return System.Convert.ToDouble(time.TotalSeconds);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parametr, string str)
        {
            try
            {
                int SliderValue = System.Convert.ToInt32((double)value);
                TimeSpan ts = new TimeSpan(0, 0, 0, SliderValue, 0);
                return ts;
            }
            catch
            {
                return null;
            }
        }
    }

    public sealed partial class MainPage : Page
    {
        private static string access_token = null;
        private static int i_or_ni = 0;

        HttpClient client = new HttpClient();
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
       
        public MainPage()
        {
            this.InitializeComponent();

            bool internet_bool = IsInternet();

            Window.Current.SizeChanged += WindowSizeChanged;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_NextTrackPressed;



            if (internet_bool)
            {
                Object value = localSettings.Values["_token"];

                if (value == null)
                {
                    this.Frame.Navigate(typeof(Auth));
                }
                access_token = value.ToString(); // the error may be here
                //
                FontsCombo.DataContext = new Me_ListViewModel_Pop();
                FontsCombo.SelectedIndex = 0;
                //
                my_list.DataContext = new Me_ListViewModel();
                Player();
                Get();

            }
            else { Exit(); }


        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var WindowWidth = Window.Current.Bounds.Width;
            if (WindowWidth < 500)
            {
                Main.Visibility = Visibility.Collapsed;
                Snapped_.Visibility = Visibility.Visible;
                Bottom.Visibility = Visibility.Collapsed;
            }
            else
            {
                Bottom.Visibility = Visibility.Visible;
                Main.Visibility = Visibility.Visible;
                Snapped_.Visibility = Visibility.Collapsed;
            }
        }

        private void Player()
        {
            Binding progressBarBinding = new Binding();
            progressBarBinding.Path = new PropertyPath("Position");
            progressBarBinding.Source = media;
            progressBarBinding.Converter = new TimespanToDouble();
            progressBarBinding.Mode = BindingMode.TwoWay;
            timeSlider.SetBinding(ProgressBar.ValueProperty, progressBarBinding);

            timeSlider_s.SetBinding(ProgressBar.ValueProperty, progressBarBinding);

            Binding progr = new Binding();
            progr.Path = new PropertyPath("DownloadProgress");
            progr.Source = media;
            progr.Mode = BindingMode.TwoWay;
            timeSlider.SetBinding(ProgressBar.TagProperty, progr);
        }

        public void Get()
        {
            GetProfile();
            my_list.SelectedIndex = 0;
        }

        public async void GetProfile()
        {
            string data = await client.GetStringAsync("https://api.vkontakte.ru/method/users.get?fields=uid,%20first_name,%20last_name,%20nickname,%20screen_name,%20photo,%20photo_medium,%20photo_big&access_token=" + access_token + "&v=5.2");
            JObject o = JObject.Parse(data);
            try
            {
                int uid = (int)o["response"][0]["id"];
                string first_name = "" + o["response"][0]["first_name"] + "";
                string last_name = "" + o["response"][0]["last_name"] + "";
                string photo_medium = "" + o["response"][0]["photo_medium"] + "";
                string full_name = first_name + " " + last_name;
                photo_mediumx.Source = new BitmapImage(new Uri(photo_medium, UriKind.Absolute));
                full_namex.Text = full_name;
                prograss_u_h.Visibility = Visibility.Collapsed;
                user_header.Visibility = Visibility.Visible;
                if (!localSettings.Values.ContainsKey("_id"))
                {
                    localSettings.Values["_id"] = uid + "";
                    localSettings.Values["_name"] = first_name + " " + last_name;
                    localSettings.Values["_photo_medium"] = "" + photo_medium + "";
                }
            }
            catch
            {
                int error_code = (int)o["error"]["error_code"];
                if (error_code == 17)
                {
                    string redirect_uri = "" + o["error"]["redirect_uri"] + "";
                    localSettings.Values.Remove("_token");
                    localSettings.Values.Remove("_id");
                    localSettings.Values.Remove("_name");
                    localSettings.Values.Remove("_photo_medium");
                    this.Frame.Navigate(typeof(ErrorPage), redirect_uri);
                }
            }
        }

        private async void Exit()
        {
            var dlg = new MessageDialog("Не удается подключиться к сети!");
            dlg.Commands.Add(new UICommand("Выход",
                new UICommandInvokedHandler((args) =>
                {
                    Application.Current.Exit();
                })));
            await dlg.ShowAsync();
        }

        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            return (connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }

        private void FontsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            int selectedIndex = cmb.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    my_list.Visibility = Visibility.Visible;
                    friends_fr.Visibility = Visibility.Collapsed;
                    group_fr.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    my_list.Visibility = Visibility.Collapsed;
                    group_fr.Visibility = Visibility.Collapsed;
                    friends_fr.Visibility = Visibility.Visible;

                    GetUsers();
                    break;
                case 2:
                    my_list.Visibility = Visibility.Collapsed;
                    friends_fr.Visibility = Visibility.Collapsed;
                    group_fr.Visibility = Visibility.Visible;
                    GetGroup();
                    break;
            }
        }

        public async void GetGroup()
        {
            try
            {
                progress_links.Visibility = Visibility.Visible;
                string Audi_Parse_Text = await client.GetStringAsync("https://api.vkontakte.ru/method/groups.get?extended=1&access_token=" + access_token + "");
                JObject Audio_Parse = JObject.Parse(Audi_Parse_Text);
                IList<JToken> audio_results = Audio_Parse["response"].Children().Skip(1).ToList();
                IList<Group> audio_p_results = new List<Group>();
                foreach (JToken result in audio_results)
                {
                    Group audiores = JsonConvert.DeserializeObject<Group>(result.ToString());
                    audio_p_results.Add(audiores);
                }
                group_fr.ItemsSource = audio_p_results;
                progress_links.Visibility = Visibility.Collapsed;
            }
            catch
            {
                var dlq = new MessageDialog("Ошибка");
                dlq.ShowAsync();
            }
        }

        public async void GetUsers()
        {
            try
            {
                progress_links.Visibility = Visibility.Visible;
                string Audi_Parse_Text = await client.GetStringAsync("https://api.vkontakte.ru/method/friends.get?fields=nickname,photo_50,photo_100&access_token=" + access_token + "");
                JObject Audio_Parse = JObject.Parse(Audi_Parse_Text);
                IList<JToken> audio_results = Audio_Parse["response"].Children().ToList();
                IList<User> audio_p_results = new List<User>();
                foreach (JToken result in audio_results)
                {
                    User audiores = JsonConvert.DeserializeObject<User>(result.ToString());
                    audio_p_results.Add(audiores);
                    audiores.Photo = audiores.photo_100;
                    audiores.Name = audiores.first_name + " " + audiores.last_name;
                }
                friends_fr.ItemsSource = audio_p_results;
                progress_links.Visibility = Visibility.Collapsed;
            }
            catch
            {
                var dlq = new MessageDialog("Ошибка");
                dlq.ShowAsync();
            }
        }

        private void my_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = my_list.SelectedValue as ListItem_Me;
            switch (Convert.ToInt32(selected.id))
            {
                case 0:
                    GetAudio(0, "gid", true, "audio.get");
                    break;
                case 1:
                    GetAudio(0, "gid", true, "audio.getRecommendations");
                    break;
                case 2:
                    GetAudio(0, "gid", true, "audio.getPopular");
                    break;
            }

        }

        private void group_fr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected_g = group_fr.SelectedValue as Group;

            try { GetAudio(selected_g.Gid, "gid", true, "audio.get"); }
            catch { }
        }

        private void friends_fr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected_u = friends_fr.SelectedValue as User;
            try { GetAudio(selected_u.uid, "uid", true, "audio.get"); }
            catch (Exception ex)
            {
            }
        }

        public async void GetAudio(int gid, string g_u, bool c, string method)
        {
            progress_music.Visibility = Visibility.Visible;
            empty_audio.Visibility = Visibility.Collapsed;
            incrementalData.ItemsSource = null;
            incrementalData_s.ItemsSource = null;
            if (method == "audio.getPopular" || method == "audio.getRecommendations")
            {
                i_or_ni = 0;
                try
                {
                    string Audi_Parse_Text = await client.GetStringAsync("https://api.vkontakte.ru/method/" + method + "?user_id=" + gid + "&access_token=" + access_token + "");

                    JObject Audio_Parse = JObject.Parse(Audi_Parse_Text);
                    IList<JToken> audio_results = Audio_Parse["response"].Children().ToList();
                    IList<Item> audio_p_results = new List<Item>();
                    foreach (JToken result in audio_results)
                    {
                        Item audiores = JsonConvert.DeserializeObject<Item>(result.ToString());

                        audio_p_results.Add(audiores);
                    }
                    Audio_Count.Text = "Всего " + audio_p_results.Count + " песен";
                    incrementalData.ItemsSource = audio_p_results;
                    incrementalData_s.ItemsSource = audio_p_results;
                    progress_music.Visibility = Visibility.Collapsed;
                    incrementalData.Visibility = Visibility.Visible;
                    incrementalData_s.Visibility = Visibility.Visible;
                }
                catch
                {
                    incrementalData.Visibility = Visibility.Collapsed;
                    empty_audio.Visibility = Visibility.Visible;
                }
            }
            else
            {
                incrementalData.ItemsSource = null;
                incrementalData_s.ItemsSource = null;
                empty_audio.Visibility = Visibility.Collapsed;
                i_or_ni = 1;
                bool error = false;
                var datasourceUrl = "https://api.vkontakte.ru/method/" + method + "?" + g_u + "=" + gid + "&count=100&access_token=" + access_token + "&v=5.2";

                string Audi_Parse_Text = await client.GetStringAsync(datasourceUrl);
                RootObject account = JsonConvert.DeserializeObject<RootObject>(Audi_Parse_Text);
                try
                {
                    int count = account.response.count;
                    Audio_Count.Text = "Всего " + account.response.count + " песен";
                    if (account.response.count == 0)
                    { error = true; }
                }
                catch { error = true; }
                if (!error)
                {
                    try
                    {
                        incrementalData.ItemsSource = new IncrementalSource<RootObject, Item>(datasourceUrl, RootObjectResponse);
                        incrementalData_s.ItemsSource = new IncrementalSource<RootObject, Item>(datasourceUrl, RootObjectResponse);
                
                    }
                    catch { }
                }
                else
                {
                    empty_audio.Visibility = Visibility.Visible;
                }

            }
            progress_music.Visibility = Visibility.Collapsed;
        }

        private PagedResponse<Item> RootObjectResponse(RootObject rootObject)
        {
            return new PagedResponse<Item>(rootObject.response.items, rootObject.response.count, rootObject.response.items != null ? rootObject.response.items.Count : 0);
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (Main.Visibility == Visibility.Visible)
            {
                incrementalData.SelectedIndex = (int)media.Tag + 1;
            }
            else { incrementalData_s.SelectedIndex = (int)media.Tag + 1; }
        }

        private void incrementalData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selected_u = incrementalData.SelectedValue as Item;
                media.Tag = incrementalData.SelectedIndex;
                media.Source = new Uri(selected_u.url);
                media.Play();
                Audio_Name.Text = selected_u.title;
                Artist_Name.Text = selected_u.artist;
                Audio_Name_S.Text = selected_u.title;
                Artist_Name_S.Text = selected_u.artist;
                _Album(selected_u.artist, selected_u.title);
                //Controls_Update();
                MediaControl.TrackName = selected_u.title;
                MediaControl.ArtistName = selected_u.artist;
                
            }
            catch { }
        }

        private void incrementalData_s_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selected_u = incrementalData_s.SelectedValue as Item;
                media.Tag = incrementalData_s.SelectedIndex;
                media.Source = new Uri(selected_u.url);
                media.Play();
                Audio_Name.Text = selected_u.title;
                Artist_Name.Text = selected_u.artist;
                Audio_Name_S.Text = selected_u.title;
                Artist_Name_S.Text = selected_u.artist;
                _Album(selected_u.artist, selected_u.title);
                //Controls_Update();
                MediaControl.TrackName = selected_u.title;
                MediaControl.ArtistName = selected_u.artist;

            }
            catch { }
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            double absvalue = (int)Math.Round(media.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);
            timeSlider.Maximum = absvalue;
            timeSlider_s.Maximum = absvalue;
        }

        private void media_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                PlayButton.IsChecked = true;
                PlayButton_S.IsChecked = true;
            }
            else if (media.CurrentState == MediaElementState.Paused || media.CurrentState == MediaElementState.Stopped)
            {
                PlayButton.IsChecked = false;
                PlayButton_S.IsChecked = false;
            }
        }

        public async void _Album(string artist, string t_name)
        {
            try
            {
                string Background_R = await client.GetStringAsync("https://itunes.apple.com/search?term=" + artist + " " + t_name + "&limit=1");

                JObject Audio_Parse2 = JObject.Parse(Background_R);
                string Image = "" + Audio_Parse2["results"][0]["artworkUrl100"] + "";
                Audio_Image.Source = new BitmapImage(new Uri(Image));
                Audio_Image_S.Source = new BitmapImage(new Uri(Image));
            }
            catch
            {
                Audio_Image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.scale-100.png"));

                Audio_Image_S.Source = new BitmapImage(new Uri("ms-appx:///Assets/Logo.scale-100.png"));
            }
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Main.Visibility == Visibility.Visible)
                {
                    incrementalData.SelectedIndex = (int)media.Tag - 1;
                }
                else
                {
                    incrementalData_s.SelectedIndex = (int)media.Tag - 1;
                }
            }
            catch { }
        }

        private void SkipButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Main.Visibility == Visibility.Visible)
                {
                    incrementalData.SelectedIndex = (int)media.Tag + 1;
                }
                else
                {
                    incrementalData_s.SelectedIndex = (int)media.Tag + 1;
                }
            }
            catch { }
        }

        private async void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            progress_music.Visibility = Visibility.Visible;
            incrementalData.ItemsSource = null;
            empty_audio.Visibility = Visibility.Collapsed;
            bool error = false;
            var datasourceUrl = "https://api.vkontakte.ru/method/audio.search?q=" + sender.QueryText + "&count=20&access_token=" + access_token + "&v=5.2";

            string Audi_Parse_Text = await client.GetStringAsync(datasourceUrl);
            RootObject account = JsonConvert.DeserializeObject<RootObject>(Audi_Parse_Text);
            try
            {
                int count = account.response.count;
                Audio_Count.Text = "Всего " + account.response.count + " песен";
                if (account.response.count == 0)
                { error = true; }
            }
            catch { error = true; }
            if (!error)
            {
                try
                {
                    incrementalData_s.ItemsSource = new IncrementalSource<RootObject, Item>(datasourceUrl, RootObjectResponse);
             
                    incrementalData.ItemsSource = new IncrementalSource<RootObject, Item>(datasourceUrl, RootObjectResponse);
                }
                catch { }
            }
            else
            {
                empty_audio.Visibility = Visibility.Visible;
            }
            progress_music.Visibility = Visibility.Collapsed;
        }

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            if (media.CurrentState == MediaElementState.Paused)
            {
                media.Play();
            }
        }

        private void PlayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (media.CurrentState == MediaElementState.Playing)
            {
                media.Pause();
            }
        }

        void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    PlayMedia();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    PauseMedia();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    NextMedia();
                    break;
                    case SystemMediaTransportControlsButton.Previous:
                    PrevMedia();
                    break;
                default:
                    break;
            }
        }
        
        async void PlayMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                media.Play();
            });
        }

        async void PauseMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                media.Pause();
            });
        }

        async void NextMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                incrementalData.SelectedIndex = (int)media.Tag + 1;
            });
        }

        async void PrevMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                incrementalData.SelectedIndex = (int)media.Tag - 1;
            });
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values.Remove("_token");
            localSettings.Values.Remove("_id");
            localSettings.Values.Remove("_name");
            localSettings.Values.Remove("_photo_medium");
            this.Frame.Navigate(typeof(Auth));
        }

        private async void MediaControl_StopPressed(object sender, object e)
        {
            sender = sender ?? new object();
            e = e ?? new object();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => media.Stop());
        }
        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            sender = sender ?? new object();
            e = e ?? new object();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (media.CurrentState == MediaElementState.Paused)
                        media.Play();
                    else
                        media.Pause();
                }
                catch
                {
                }
            });
        }
        private async void MediaControl_PausePressed(object sender, object e)
        {
            sender = sender ?? new object();
            e = e ?? new object();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => media.Pause());
        }
        private async void MediaControl_PlayPressed(object sender, object e)
        {
            sender = sender ?? new object();
            e = e ?? new object();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => media.Play());
        }
        private async void MediaControl_NextTrackPressed(object sender, object e)
        {
            sender = sender ?? new object();
            e = e ?? new object();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => NextMedia());
        }
        private async void MediaControl_PrevTrackPressed(object sender, object e)
        {
            sender = sender ?? new object();
            e = e ?? new object();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PrevMedia());
        }
    }
}
