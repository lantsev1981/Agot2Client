using GameService;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    static class CheckBlackListTask
    {
        static public bool IsBusy { get; private set; }

        static public void AddTask(Guid gameId, Action action)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        var cf = new ChannelFactory<IGameService>("AGOT2Game");
                        cf.Endpoint.Address = new EndpointAddress($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/GameHost/{gameId}");
                        var service = cf.CreateChannel();
                        var checkResult = service.CheckBlackList(MainWindow.GamePortal.User.Login);

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            switch (checkResult)
                            {
                                case false:
                                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_accessDenied1"));
                                    return;
                                case null:
                                    App.Agot2.questionView.Show(MessageBoxButton.YesNo, App.GetResources("text_accessDenied2"), action);
                                    break;
                                default:
                                    action();
                                    break;
                            }
                        }), DispatcherPriority.ApplicationIdle);
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_checkBlacklist") + ": " + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
