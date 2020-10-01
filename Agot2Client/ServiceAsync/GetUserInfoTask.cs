using System;
using System.Linq;
using System.Windows.Threading;
using System.Windows;

namespace Agot2Client
{
    public class GetUserInfoTask
    {
        bool _IsBusy, _Abort;
        DispatcherTimer _Timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        ExtGame _Game;

        public GetUserInfoTask(ExtGame game)
        {
            _Game = game;
            _Timer.Interval = TimeSpan.FromSeconds(2);
            _Timer.Tick += _DispatcherTimer_Tick;
        }

        public void StartAddTask()
        {
            if (_IsBusy || _Game.GetStepTask.IsBusy)
                return;

            _IsBusy = true;
            _Abort = false;
            _Timer.Stop();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        var result = _Game.GameService.GetUserInfo(_Game.ClientGameUser.WCFGameUser);
                        
                        if (result == null || result.Count == 0)
                        {
                            MainWindow.ClientInfo.OnLineStatus = false;
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_syncUsers2"));
                        }
                        else
                        {
                            result.Where(p => !string.IsNullOrEmpty(p.Login) && p.Login != "Вестерос").ToList().ForEach(p => MainWindow.GamePortal.GetUser(p.Login));
                            //Обновление пользователей и ходов
                            _Game.UpdateGameUsers(result);
                            if (_Game.ClientGameUser.WCFGameUser.NewStep)
                                _Game.GetStepTask.GetStep();

                            if (!_Abort)
                                _Timer.Start();
                        }
                    }
                    catch (Exception exp)
                    {
                        MainWindow.ClientInfo.OnLineStatus = false;
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_syncUsers1") + exp.Message);
                    }

                    _IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void StopAddTask()
        {
            _Abort = true;
            _Timer.Stop();
        }

        void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            StartAddTask();
        }
    }
}
