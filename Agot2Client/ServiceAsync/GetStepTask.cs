using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    public class GetStepTask
    {
        public bool IsBusy { get; private set; }
        ExtGame _Game;
        int curentStepIndex;

        public GetStepTask(ExtGame game)
        {
            _Game = game;
        }

        public void AddTask(int stepIndex = 0)
        {
            if (IsBusy)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("notify_stepSyncProgress"));
                return;
            }

            IsBusy = true;
            curentStepIndex = stepIndex;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(GetStep);
            }), DispatcherPriority.ApplicationIdle);
        }

        public void GetStep()
        {
            try
            {
                var result = _Game.GameService.GetStep(_Game.ClientGameUser.WCFGameUser, curentStepIndex);

                if (result == null || result.Count == 0)
                {
                    MainWindow.ClientInfo.OnLineStatus = false;
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_syncSteps2"));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (!result.Any(p => !_Game.AddStep(p)))
                            {
                                if (curentStepIndex == 0)
                                {
                                    App.Agot2.notify.Position = TimeSpan.Zero;
                                    App.Agot2.notify.Play();
                                }

                                int lastStepIndex = _Game.LastStepIndex;
                                _Game.ClientGameUser.WCFGameUser.LastStepIndex = lastStepIndex;
                                _Game.CurrentViewKey = curentStepIndex == 0 ? lastStepIndex : result.Last().Id;
                            }

                        }
                        catch (Exception exp)
                        {
                            MainWindow.ClientInfo.OnLineStatus = false;
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_syncSteps3") + exp.Message);
                        }

                        IsBusy = false;
                    }), DispatcherPriority.ApplicationIdle);
                }
            }
            catch (Exception exp)
            {
                MainWindow.ClientInfo.OnLineStatus = false;
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_syncSteps1") + exp.Message);
                IsBusy = false;
            }
        }
    }
}
