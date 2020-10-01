using GamePortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UserProfile.xaml
    /// </summary>
    public partial class UserProfile : UserControl
    {
        private string sortBy = "";
        private GPUser _User;
        private UserGameView SelectedGameView;

        public UserProfile()
        {
            InitializeComponent();
            DataContextChanged += UserProfile_DataContextChanged;
        }

        private void UserProfile_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _User = (GPUser)e.NewValue;
        }

        private void UserGame_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedGameView = sender as UserGameView;
            UserGameViewModel userGame = SelectedGameView?.Value ?? (UserGamesList.Items.Count > 0
                ? (UserGameViewModel)UserGamesList.Items[0]
                : null);

            usersMeetList.Items.Clear();

            if (userGame != null)
            {
                MainWindow.GamePortal.GetUsers(sortBy, userGame.Model).ForEach(p =>
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        WCFUserGame usergame = p.EndedUserGames.Single(p2 => p2.GameId == userGame.Model.GameId);
                        usersMeetList.Items.Add(new GamePersonItemViewModel()
                        {
                            User = p,
                            HomeType = MainWindow.ClientInfo.WorldData.HomeType
                                .Single(p1 => p1.WCFHomeType.Name == usergame.HomeType),
                            IsLikePanelVisible = Visibility.Visible,
                            UserGame = usergame
                        });
                    }), DispatcherPriority.ApplicationIdle);
                });
            }
        }

        public void UpdatePerson_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        MainWindow.GamePortal.UpdateUserFromServer(null, () =>
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                UpdateUserGamesView();
                            }), DispatcherPriority.ApplicationIdle);
                        });
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_profileLoad") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Создаёт задачи обновления отсортированного списка игр пользователей
        /// </summary>
        /// <param name="param">параметр сортировки</param>
        public void UpdateUserGamesView()
        {
            rootGrid.IsEnabled = false;
            UserGamesProgressBar.Value = 0;
            UserGamesProgressBar.IsIndeterminate = true;
            UserGamesProgressBar.Visibility = Visibility.Visible;

            UserGamesList.Items.Clear();
            List<WCFUserGame> games = _User.UserGames.Where(p => p.EndTime != null).OrderByDescending(p => p.StartTime).ToList();
            UserGamesProgressBar.Maximum = games.Count;

            games.ForEach(p =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UserGamesProgressBar.IsIndeterminate = false;
                    UserGamesList.Items.Add(new UserGameViewModel(p));
                    UserGamesProgressBar.Value++;
                }), DispatcherPriority.ApplicationIdle);
            });

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UserGamesProgressBar.Visibility = Visibility.Hidden;
                rootGrid.IsEnabled = true;

                UserGame_MouseLeftButtonUp(null, null);
            }), DispatcherPriority.ApplicationIdle);
        }

        private void Rate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            sortBy = textBlock.Tag.ToString();
            UserGame_MouseLeftButtonUp(SelectedGameView, null);
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.Donate();
        }
    }
}
