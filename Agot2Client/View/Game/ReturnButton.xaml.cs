using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ReturnButton.xaml
    /// </summary>
    public partial class ReturnButton : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ExtGame _game;
        public ExtGame Game
        {
            get => _game;
            private set
            {
                _game = value;
                App.Settings.Value.LastGameId = value?.WCFGame.Id.ToString();
#if DEBUG
                App.Settings.Write();
#endif
            }
        }

        public ReturnButton()
        {
            InitializeComponent();

            timerBtn.DataContext = this;
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
            MainWindow.ClientInfo.ReturnTimer.Tick += ReturnTimer_Tick;

            Brush = new SolidColorBrush(Colors.Red);
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (game != null)
            {
                if (!game.WCFGame.CloseTime.HasValue && game.WCFGame.GameUser.Any(p => p.HomeType != null && p.Login == MainWindow.GamePortal.User.Login))
                {
                    Game = game;
                    MainWindow.ClientInfo.ReturnTimer.Stop();
                    MaxTime = Time = new TimeSpan(0, 0, Game.WCFGame.Settings.MaxTime);
                    MainWindow.ClientInfo.OnPropertyChanged("ReturnTimer");
                }
                else if (game.WCFGame.Id == Game?.WCFGame.Id)
                {
                    Game = null;
                    MainWindow.ClientInfo.ReturnTimer.Stop();
                    MainWindow.ClientInfo.OnPropertyChanged("ReturnTimer");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ClientInfo.ClientGameId = Game.WCFGame.Id;
        }

        private void ReturnTimer_Tick(object sender, EventArgs e)
        {
            Time -= MainWindow.ClientInfo.ReturnTimer.Interval;

            if (Time.Seconds >= 0
                && ((Time < new TimeSpan(0, 1, 0) && Time.Seconds % 10 == 0)
                || Time < new TimeSpan(0, 0, 10)))
            {
                App.Agot2.stepTimer.Position = TimeSpan.Zero;
                App.Agot2.stepTimer.Play();
            }


            if (Time <= TimeSpan.Zero)
            {
                Game.DisConnectTask.AddTask(Game.ClientGameUser.WCFGameUser, false);
                MainWindow.ClientInfo.ReturnTimer.Stop();
            }
        }

        public Brush Brush { get; private set; }

        private TimeSpan MaxTime;
        private TimeSpan _Time;
        public TimeSpan Time
        {
            get => _Time;
            private set
            {
                _Time = value;
                this.OnPropertyChanged("TimeView");

                this.Brush.Opacity = 1 - (this.Time.TotalMinutes / MaxTime.TotalMinutes);
                this.OnPropertyChanged("Brush");
            }
        }

        public string TimeView
        {
            get { return Time.TotalHours > 1 ? $"{((int)Time.TotalHours).ToString("00")}:{Time.Minutes.ToString("00")}" : $"{((int)Time.TotalMinutes).ToString("00")}:{Time.Seconds.ToString("00")}"; }
        }
    }
}
