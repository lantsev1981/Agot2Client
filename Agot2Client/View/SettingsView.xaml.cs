using MyLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        class LikeProfileViewModel
        {
            public GPUser User { get; set; }
            public string Password { get; set; }

            public LikeProfileViewModel(GPUser user)
            {
                User = user;
                Password = string.Empty;
            }
        }

        public SettingsView()
        {
            InitializeComponent();
        }

        public void Show(AppSettings settings)
        {
            this.DataContext = settings;
            this.Visibility = Visibility.Visible;
            profileGrid.DataContext = MainWindow.GamePortal.User;

            GetLikeProfile();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            App.Settings.Write();
        }

        private void LangButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;

            var oldLang = App.Settings.Value.Lang;
            var newLang = btn.Tag.ToString();
            if (oldLang == newLang)
                return;

            App.Settings.Value.Lang = btn.Tag.ToString();
            App.Settings.Write();
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Agot2Client.exe"));
            Environment.Exit(555);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Value.User = null; App.Settings.Value.access_token = null;
            App.Settings.Write();
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Agot2Client.exe"));
            Environment.Exit(555);
        }

        private void GetLikeProfile()
        {
            MainWindow.GamePortal.GetLikeProfileAsync((accounts) =>
            {
                var result = accounts.Select(p =>
                {
                    var user = MainWindow.GamePortal.GetUser(p);
                    return new LikeProfileViewModel(user);
                });

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LikeProfileItems.DataContext = result;
                }), DispatcherPriority.ApplicationIdle);
            });
        }

        private void linkProfile_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var view = sender as FrameworkElement;
            if (view == null)
                return;

            var model = view.DataContext as LikeProfileViewModel;
            if (model == null)
                return;

            this.LikeProfileItems.DataContext = null;

            MainWindow.GamePortal.LinkAccountsAsync(model.User.Login, Crypto.SHA1Hex(model.Password), () =>
            {
                GetLikeProfile();
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_linkAccountCompleted"));
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    App.Agot2.leftPanelView.leaderBoardView.UsersUpdate();
                }), DispatcherPriority.ApplicationIdle);
            });
        }

        private void clear_Button_Click(object sender, RoutedEventArgs e)
        {
            clearBtn.Visibility = Visibility.Collapsed;

            MainWindow.GamePortal.ClearProfileAsync(() =>
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_linkAccountCompleted"));
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    App.Agot2.leftPanelView.leaderBoardView.UsersUpdate();
                }), DispatcherPriority.ApplicationIdle);
            });
        }
    }
}
