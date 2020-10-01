using GameService;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    public class DisConnectTask
    {
        static bool IsBusy;
        ExtGame _Game;

        public DisConnectTask(ExtGame game)
        {
            _Game = game;
        }

        public void AddTask(WCFGameUser gameUser, bool isClosing)
        {
            if (IsBusy)
                return;

            StepBtn.DeleteStepTimer();

            IsBusy = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        _Game.GameService.DisConnect(gameUser);

                        if (isClosing)
                            Environment.Exit(555);
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("text_disconnect") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
