using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace Agot2Client
{
    public class StepTimer : DispatcherTimer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public List<GameTimer> _GameTimerList = new List<GameTimer>();
        private GameTimer _GameTimer;
        public GameTimer GameTimer
        {
            get => _GameTimer;
            set
            {
                _GameTimer = value;
                this.OnPropertyChanged("GameTimer");
            }
        }

        public ExtGame Game { get; private set; }

        public StepTimer()
        {
            Interval = GameTimer.Interval;
            this.Tick += StepTimer_Tick;
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            Game = game;
        }

        private void StepTimer_Tick(object sender, EventArgs e)
        {
            GameTimer.TimeDown();

            if (GameTimer.Time.Seconds == 0
                || (GameTimer.Time < new TimeSpan(0, 1, 0) && GameTimer.Time.Seconds % 10 == 0)
                || GameTimer.Time < new TimeSpan(0, 0, 10))
            {
                App.Agot2.stepTimer.Position = TimeSpan.Zero;
                App.Agot2.stepTimer.Play();
            }


            if (GameTimer.Time <= TimeSpan.Zero)
            {
#if !DEBUG
                if (Game != null)
                    Game.DisConnectTask.AddTask(Game.ClientGameUser.WCFGameUser, false);
#endif

                MainWindow.ClientInfo.ClientGameId = null;
            }
        }

        public void Start(ExtStep step)
        {
            if (step == null || step.Game.WCFGame.Settings.NoTimer)
                return;

            GameTimer = _GameTimerList.SingleOrDefault(p => p.Game == step.WCFStep.Game);

            if (GameTimer == null)
            {
                GameTimer = new GameTimer(step);
                _GameTimerList.Add(GameTimer);
            }
            else
                GameTimer.TimeUp(step.WCFStep.Id);

            base.Start();
        }
    }

    public class GameTimer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public GameTimer(ExtStep step)
        {
            Game = step.WCFStep.Game;
            _StepId = step.WCFStep.Id;
            Brush = new SolidColorBrush(Colors.Red);
            MaxTime = TimeSpan.FromSeconds(step.Game.WCFGame.Settings.MaxTime);
            AddTime = TimeSpan.FromSeconds(step.Game.WCFGame.Settings.AddTime);
            Time = MaxTime;
        }

        private TimeSpan MaxTime;
        private TimeSpan AddTime;
        public static TimeSpan Interval = TimeSpan.FromSeconds(1);
        public Guid Game { get; private set; }
        private int _StepId;
        public Brush Brush { get; private set; }

        private TimeSpan _Time;
        public TimeSpan Time
        {
            get => _Time;
            private set
            {
                if (value > MaxTime)
                    _Time = MaxTime;
                else if (value < TimeSpan.Zero)
                    _Time = TimeSpan.Zero;
                else
                    _Time = value;

                this.OnPropertyChanged("TimeView");

                this.Brush.Opacity = 1 - (this.Time.TotalMinutes / MaxTime.TotalMinutes);
                this.OnPropertyChanged("Brush");
            }
        }

        public string TimeView => Time.TotalHours > 1 ? $"{((int)Time.TotalHours).ToString("00")}:{Time.Minutes.ToString("00")}" : $"{((int)Time.TotalMinutes).ToString("00")}:{Time.Seconds.ToString("00")}";

        public void TimeUp(int stepId)
        {
            if (stepId > _StepId)
            {
                Time += AddTime;
                _StepId = stepId;
            }
        }

        public void TimeDown()
        {
            Time -= GameTimer.Interval;
        }
    }
}
