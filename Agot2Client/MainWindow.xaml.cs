using GamePortal;
using GameService;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Updater;

namespace Agot2Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public static UpdaterClient UpdaterClient;
        private static ClientInfo _ClientInfo = new ClientInfo();
        public static ClientInfo ClientInfo => _ClientInfo;

        public static GamePortal GamePortal { get; set; }
        public static GameTypes GameTypes { get; set; }


        #region viewModel
        public List<Vol> Vols => App.Settings.Value.Vols;

        private string _LastDonateInfo;
        public string LastDonateInfo
        {
            get => _LastDonateInfo;
            set
            {
                _LastDonateInfo = value;
                OnPropertyChanged("LastDonateInfo");
            }
        }
        #endregion

        public MainWindow()
        {
            Title = App.Title;
            GameTypes = new GameTypes();
            foreach (GameTypeItem item in GameTypes)
                item.Name = string.Format(App.GetResources(item.Name), item.PlayerCount);

            Window_Initialized(null, null);

            DataContext = this;
            App.Agot2 = this;

            InitializeComponent();
            //mainTitle.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media/main_title.mp3"), UriKind.Absolute);
            leftPanelView.leaderBoardView.UsersUpdate();

            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
#if !DEBUG
            try
            {
                UpdaterClient = new UpdaterClient("lantsev1981.ru:6999", "Agot2Client.exe", "AGOT: Online BG");
                bool? result = UpdaterClient.Update();
                if (result != true)
                {
                    try
                    {
                        Process.Start($"http://lantsev1981.ru/{CultureInfo.CurrentUICulture.Name.Substring(0, 2)}/home/LastNews");
                    }
                    catch { }
                    Environment.Exit(777);
                }
#endif

                if (new ConfirmWindow().ShowDialog() != true)
                    Environment.Exit(555);

                CryptoFileJson<WCFStaticData> staticData = new CryptoFileJson<WCFStaticData>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "staticData"), "W@NtUz81");
                MainWindow.ClientInfo.WorldData = new ExtStaticData(staticData.Read());

                MainWindow.GamePortal = new GamePortal();
                GamePortal.PropertyChanged += GamePortal_PropertyChanged;
                MainWindow.GamePortal.Load();

#if !DEBUG
                if (!MainWindow.GamePortal.IsAdmin
                    && Process.GetProcesses(".").Count(p => p.ProcessName == "Agot2Client") > 1
                    && (MainWindow.GamePortal.User.AllPower < 400 || MainWindow.GamePortal.User.RateValues["DurationHours"] < 24))
                {
                    MessageBox.Show(App.GetResources("text_accessDenied3"), App.GetResources("text_error"), MessageBoxButton.OK, MessageBoxImage.Stop);
                    Environment.Exit(555);
                }
            }
            catch (Exception exp)
            {
                App.SendException(exp);
                Environment.Exit(666);
            }
#endif
        }

        private void GamePortal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MasterOfDonate":
                    if (GamePortal.MasterOfDonate != null)
                        LastDonateInfo = string.Format(App.GetResources("text_lastDonateInfo"),
                            GamePortal.MasterOfDonate.LastPayment.Power,
                            GamePortal.MasterOfDonate.LastPayment.Time.LocalDateTime);
                    break;
                default:
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetControlVisability(cloudsView);
            SetControlVisability(lobbyView);
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (MainWindow.ClientInfo.ClientGame != null)
            {
                MainWindow.ClientInfo.ClientGame.ClientStepCganged -= ExtGame_ClientStepCgange;
            }
            if (game != null)
            {
                MainWindow.ClientInfo.ClientGame.ClientStepCganged += ExtGame_ClientStepCgange;
                ToGameView();
            }
            else
            {
                SetControlVisability(gameView, Visibility.Collapsed);
                SetControlVisability(cloudsView);
                SetControlVisability(lobbyView);
            }

            for (int i = App.Current.Windows.Count - 1; i >= 0; i--)
            {
                if (App.Current.Windows[i] != this)
                    App.Current.Windows[i].Close();
            }
        }

        private void SetControlVisability(UserControl userControl, Visibility visibility = Visibility.Visible)
        {
            DoubleAnimation animation = new DoubleAnimation(userControl.Opacity, visibility == Visibility.Visible ? 1 : 0, new Duration(new TimeSpan(0, 0, 2)));
            userControl.BeginAnimation(UserControl.OpacityProperty, animation);

            userControl.Visibility = visibility;
        }

        private void ToGameView()
        {
            SetControlVisability(lobbyView, Visibility.Collapsed);
            SetControlVisability(cloudsView, Visibility.Collapsed);
            SetControlVisability(gameView);
        }

        private void ExtGame_ClientStepCgange(ExtStep newValue)
        {
            if (newValue != null)
            {
                if (this.WindowState == WindowState.Minimized)
                    this.WindowState = WindowState.Maximized;
                this.Topmost = true;
                this.Topmost = false;

                stepTimer.Position = TimeSpan.Zero;
                stepTimer.Play();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //GamePortal.SettingsSave();
            MainWindow.ClientInfo.DisConnect(true);
            e.Cancel = true;
        }

        public void UpdateUsersView(string sortBy, ItemsControl list, ProgressBar progressBar, Grid relatedGrid, WCFUserGame userGame = null)
        {
            relatedGrid.IsEnabled = false;
            progressBar.Value = 0;
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;

            list.Items.Clear();
            List<GPUser> users = GamePortal.GetUsers(sortBy, userGame);
            progressBar.Maximum = users.Count;

            users.ForEach(p =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressBar.IsIndeterminate = false;
                    list.Items.Add(p);
                    progressBar.Value++;
                    if (userGame == null)
                        p.Position = progressBar.Value.ToString();
                }), DispatcherPriority.ApplicationIdle);
            });

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GamePortal.User.OnPropertyChanged("Position");

                progressBar.Visibility = Visibility.Hidden;
                relatedGrid.IsEnabled = true;
            }), DispatcherPriority.ApplicationIdle);
        }

        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            GamePortal.Settings();
        }

        private bool _IsDisableNewGame;
        public bool IsDisableNewGame
        {
            get => _IsDisableNewGame;
            set
            {
                _IsDisableNewGame = value;
                OnPropertyChanged("IsDisableNewGame");
            }
        }
        private void DisableNewGame(object sender, RoutedEventArgs e)
        {
            App.TaskFactory.StartNew(() =>
            {
                try
                {
                    App.Service.DisableNewGame(App.ClientVersion, GamePortal.User.Login, !IsDisableNewGame);
                    IsDisableNewGame = !IsDisableNewGame;
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ((Storyboard)App.Agot2.lobbyView.gameListView.Resources[IsDisableNewGame ? "Collapsed" : "Hide"]).Begin();
                        App.Agot2.lobbyView.gameListView.newGameMenuView.IsEnabled = !IsDisableNewGame;
                    }));
                }
                catch (Exception exp)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                }
            });
        }
    }
}
