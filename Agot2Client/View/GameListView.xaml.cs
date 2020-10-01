using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для GameListView.xaml
    /// </summary>
    public partial class GameListView : UserControl
    {
        public GameListViewModel ViewModel { get; private set; }
        public GameListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = new GameListViewModel();
            itemsControl.ItemsSource = ViewModel.Items;
        }
    }

    public class GameListViewModel
    {
        private static bool IsBusy;
        private DispatcherTimer _Timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        public ObservableCollection<GameItemViewModel> Items { get; private set; }

        public GameListViewModel()
        {
            Items = new ObservableCollection<GameItemViewModel>();
            _Timer.Interval = TimeSpan.FromSeconds(30);
            _Timer.Tick += _DispatcherTimer_Tick;
        }

        public void StartAddTask()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            _Timer.Stop();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        GameService.WCFService result = App.Service.GetGame(App.ClientVersion, MainWindow.GamePortal.User.Login);

#if !DEBUG
                        //Необходимо обновить клиент
                        if (result == null)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                App.Settings.Write();
                                MainWindow.UpdaterClient.Update();
                                try
                                {
                                    Process.Start($"http://lantsev1981.ru/{CultureInfo.CurrentUICulture.Name.Substring(0, 2)}/home/LastNews");
                                }
                                catch { }
                                Environment.Exit(777);
                            }), DispatcherPriority.ApplicationIdle).Wait();
                        }
#endif

                        App.Agot2.IsDisableNewGame = result.IsDisableNewGame;
                        MainWindow.ClientInfo.OnLineStatus = true;

                        //отрисовка
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            bool newGameMenuShow = !result.IsDisableNewGame;

                            //Удаляем устаревшие игры
                            Items.ToList().ForEach(p =>
                            {
                                if (!result.Games.Any(p1 => p1.Id == p.Model.Id))
                                    Items.Remove(p);
                            });

                            result.Games = result.Games.OrderBy(p => p.CreateTime).ToList();
                            result.Games.ForEach(p =>
                                   {
                                       if (p.Settings.Lang == null || p.Settings.Lang == App.Settings.Value.Lang
                                       || MainWindow.GamePortal.IsAdmin)
                                       {
                                           GameItemViewModel item = Items.SingleOrDefault(p1 => p1.Model.Id == p.Id);

                                           //Добавляем новые игры
                                           if (item == null)
                                           {
                                               item = new GameItemViewModel(p);
                                               Items.Insert(0, item);
                                           }
                                           //Обновляем текущие игры
                                           else
                                               item.Update(p);

                                           if (p.Settings.CreatorLogin == MainWindow.GamePortal.User.Login && p.CloseTime == null)
                                               newGameMenuShow = false;

                                           //Перемещаем по списку согласно статусу игры
                                           int index = Items.IndexOf(item);
                                           while (index < Items.Count - 1 && item.GameStatus > Items[index + 1].GameStatus)
                                           { Items.Move(index, ++index); }
                                           while (index > 0 && item.GameStatus < Items[index - 1].GameStatus)
                                           { Items.Move(index, --index); }
                                       }
                                   });

                            //отображаем меню создания игры если нет открытых игр этого пользователя
                            if (newGameMenuShow)
                            {
                                if (!App.Agot2.lobbyView.gameListView.newGameMenuView.IsMouseOver)
                                {
                                    ((Storyboard)App.Agot2.lobbyView.gameListView.Resources["Hide"]).Begin();
                                    App.Agot2.lobbyView.gameListView.newGameMenuView.IsEnabled = true;
                                }
                            }
                            //или скрываем
                            else
                            {
                                ((Storyboard)App.Agot2.lobbyView.gameListView.Resources["Collapsed"]).Begin();
                                App.Agot2.lobbyView.gameListView.newGameMenuView.IsEnabled = false;
                            }

                            //скрываем профиль игрока
                            MainWindow.GamePortal.SetLeaderBoardVisibility(result.Games.Where(p=>p.CloseTime==null).SelectMany(p=>p.GameUser)
                                .Where(p=>p.HomeType!=null && p.Login!=null && p.Login!="Вестерос").Distinct().ToList(),true);

                            IsBusy = false;
                            _Timer.Start();
                        }), DispatcherPriority.ApplicationIdle);
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Items.Clear();
                            ((Storyboard)App.Agot2.lobbyView.gameListView.Resources["Collapsed"]).Begin();
                            App.Agot2.lobbyView.gameListView.newGameMenuView.IsEnabled = false;
                        }), DispatcherPriority.ApplicationIdle);

                        IsBusy = false;
                        _Timer.Start();
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void StopAddTask()
        {
            _Timer.Stop();
        }

        private void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            StartAddTask();
        }
    }
}
