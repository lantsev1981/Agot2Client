using GameService;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    public static class ConnectTask
    {
        private static bool IsBusy;

        public static void AddTask(Guid gameId)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            MainWindow.ClientInfo.OnLineStatus = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        ChannelFactory<IGameService> cf = new ChannelFactory<IGameService>("AGOT2Game");
                        cf.Endpoint.Address = new EndpointAddress($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/GameHost/{gameId}");
                        IGameService service = cf.CreateChannel();

                        WCFGame result = service.Connect(
                            MainWindow.GamePortal.User.Login,
                            string.IsNullOrEmpty(MainWindow.ClientInfo.GamePassword) ? null : MainWindow.ClientInfo.GamePassword,
                            MainWindow.ClientInfo.HomeSelected);

                        if (result == null)
                        {
                            MainWindow.ClientInfo.OnLineStatus = false;
                            MainWindow.ClientInfo.ClientGameId = null;
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("notify_connectionGame"));
                        }
                        else
                        {
                            result.GameUser.Where(p => !string.IsNullOrEmpty(p.Login) && p.Login != "Вестерос").ToList().ForEach(p => MainWindow.GamePortal.GetUser(p.Login));
                            ExtGame game = null;
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                ArrowViewModel.GlobalDuration = new Duration(TimeSpan.FromSeconds(5));
                                game = new ExtGame(result, service);

                                //скрываем профиль игрока
                                if (game.WCFGame.CloseTime == null)
                                {
                                    game.ExtGameUser.Where(p => p.WCFGameUser.HomeType != null && p.WCFGameUser.Login != null && p.WCFGameUser.Login != "Вестерос")
                                        .ToList().ForEach(p =>
                                        {
                                            p.GPUser.LeaderBoardVisibility = Visibility.Collapsed;
                                            p.GPUser.OnPropertyChanged("LeaderBoardVisibility");
                                        });
                                }
                            }), DispatcherPriority.ApplicationIdle).Wait();

                            MainWindow.ClientInfo.ClientGame = game;
                            MainWindow.ClientInfo.OnLineStatus = true;
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("notify_mapRender"));
                        }
                    }
                    catch (Exception exp)
                    {
                        MainWindow.ClientInfo.OnLineStatus = false;
                        MainWindow.ClientInfo.ClientGameId = null;
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_connection") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
