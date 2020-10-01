using GameService;
//using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    public static class NewGameTask
    {
        public static bool IsBusy { get; private set; }

        public static void AddTask()
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
                        WCFGame result = App.Service.NewGame(App.ClientVersion, MainWindow.ClientInfo.GameSettings, string.IsNullOrEmpty(MainWindow.ClientInfo.GamePassword) ? null : MainWindow.ClientInfo.GamePassword);

                        if (result == null)
                        {
                            MainWindow.ClientInfo.OnLineStatus = false;
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("error_createGame2"));
                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                App.Agot2.homeSelectView.Load(result);
                            }), DispatcherPriority.ApplicationIdle);
                        }
                    }
                    catch (Exception exp)
                    {
                        MainWindow.ClientInfo.OnLineStatus = false;
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_createGame1") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
