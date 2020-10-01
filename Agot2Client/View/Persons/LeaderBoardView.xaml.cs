using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using System.Windows.Shapes;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UsersPanelView.xaml
    /// </summary>
    public partial class LeaderBoardView : UserControl
    {
        private string sortBy = "";

        public LeaderBoardView()
        {
            InitializeComponent();
        }

        public static bool LoadingData { get; private set; }
        public void UsersUpdate()
        {
            LoadingData = true;
            UsersGrid.IsEnabled = false;
            App.Agot2.updatingProfile.updatingProfileProgress.Value = UsersProgressBar.Value = 0;
            App.Agot2.updatingProfile.updatingProfileProgress.IsIndeterminate = UsersProgressBar.IsIndeterminate = true;

            App.Agot2.updatingProfile.Visibility = UsersProgressBar.Visibility = System.Windows.Visibility.Visible;

            MainWindow.GamePortal.UpdateUsersAsync((progress, count) =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (progress == -1)
                    {
                        App.Agot2.updatingProfile.updatingProfileProgress.IsIndeterminate = UsersProgressBar.IsIndeterminate = false;
                        App.Agot2.updatingProfile.updatingProfileProgress.Maximum = UsersProgressBar.Maximum = count;
                    }
                    else
                        App.Agot2.updatingProfile.updatingProfileProgress.Value = UsersProgressBar.Value = progress;
                }), DispatcherPriority.ApplicationIdle);
            }, (exp) =>
            {
                if (exp != null)
                {
                    Thread.Sleep(10000);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        UsersUpdate();
                    }), DispatcherPriority.ApplicationIdle);//повторяем до загрузки всех профилей
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        LoadingData = false;
                        MainWindow.ClientInfo.GameSettings.CreatorLogin = MainWindow.GamePortal.User.Login;
                        if (GPUser.MinRateValues.ContainsKey("LikeRate"))
                        {
                            App.Agot2.lobbyView.gameListView.newGameMenuView.LikeSlider.Minimum = GPUser.MinRateValues["LikeRate"];
                            App.Agot2.lobbyView.gameListView.newGameMenuView.LikeSlider.Value = GPUser.MinRateValues["LikeRate"];
                        }

                        if (!UpdatingProfile.PlayIntro)
                            App.Agot2.updatingProfile.Hide();

                    }), DispatcherPriority.ApplicationIdle);//нужно дождаться биндинга профиля  
                }
            });
        }

        public void FinishUpdate()
        {
            App.Agot2.updatingProfile.Visibility = Visibility.Collapsed;
            App.Agot2.leftPanelView.userProfile.UpdateUserGamesView();
            App.Agot2.UpdateUsersView(sortBy, UsersList, UsersProgressBar, UsersGrid);
            App.Agot2.lobbyView.gameListView.ViewModel.StartAddTask();

            //возврат в игру после автоматического обновления
            if (!string.IsNullOrWhiteSpace(App.Settings.Value.LastGameId))
            {
                if (Guid.TryParse(App.Settings.Value.LastGameId, out Guid lastGameId))
                    MainWindow.ClientInfo.ClientGameId = lastGameId;

                App.Settings.Value.LastGameId = null;
                App.Settings.Write();
            }
        }

        private void Rate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            sortBy = textBlock.Tag.ToString();
            //MainWindow.GamePortal.UsersSortedBy(textBlock.Tag.ToString());
            App.Agot2.UpdateUsersView(sortBy, UsersList, UsersProgressBar, UsersGrid);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                App.Agot2.UpdateUsersView(sortBy, UsersList, UsersProgressBar, UsersGrid);

            e.Handled = true;
        }

        private void OnlineOnly_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            App.Agot2.UpdateUsersView(sortBy, UsersList, UsersProgressBar, UsersGrid);
        }

        private void GetPersons_Button_Click(object sender, RoutedEventArgs e)
        {
            UsersUpdate();
        }
    }
}
