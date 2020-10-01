using GameService;
using MyMod;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    public class ClientInfo : MyNotifyObj
    {
        #region ViewModel
        public string MapImgPath { get; private set; }
        #endregion

        public DispatcherTimer ReturnTimer { get; private set; }

        public DispatcherTimer PlayStepsTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = new TimeSpan(0, 0, 1) };
        private string _PlayPauseImageName = "/Image/play.png";
        public string PlayPauseImageName
        {
            get => _PlayPauseImageName;
            set
            {
                _PlayPauseImageName = value;
                OnPropertyChanged("PlayPauseImageName");
            }
        }

        /// <summary>
        /// Событие происходит до изменения текущей игры
        /// EventHandler.sender содержит новый экземпляр игры
        /// </summary>
        public static event Action<ExtGame> ClientGameChanging;
        private void OnClientGameChanging(ExtGame newValue)
        {
            ClientGameChanging?.Invoke(_ClientGame = newValue);
        }

        public ClientInfo()
        {
            GameSettings = new WCFGameSettings() { AddTime = 150, MaxTime = 600 };
            MapImgPath = App.GetResources("image_map");
            ReturnTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = new TimeSpan(0, 0, 1) };
        }

        public string OnLineStatusImage => OnLineStatus
                    ? "/Image/on-line.png"
                    : "/Image/off-line.png";

        private bool _OnLineStatus;
        public bool OnLineStatus
        {
            get => _OnLineStatus;
            set => Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                 {
                     //онлайн
                     if (_OnLineStatus == false && value == true)
                     {
                         //если подключены к игре
                         if (ClientGameId.HasValue)
                         {
                             ExtGame game = ClientGame;
                             if (game == null)
                             {
                                 ConnectTask.AddTask(ClientGameId.Value);
                                 return;
                             }

                             //game.UpdateGameUsers(_ClientGame.WCFGame.GameUser);
                             if (game.WCFGame.CloseTime == null)
                                 game.GetUserInfoTask.StartAddTask();
                             else
                                 game.GetStepTask.AddTask();

                             App.Agot2.Title = $"{App.Title} | {App.GetResources("dictionary_game")}: '{ClientGame.ExtGameName}', {App.GetResources("dictionary_home")}: '{game.ClientGameUser.ExtHomeType?.Name ?? App.GetResources("dictionary_empty")}'";
                         }
                     }

                     //офлайн
                     if (_OnLineStatus == true && value == false)
                     {
                         PlayStepsTimer.Stop();
                         PlayPauseImageName = "/Image/play.png";
                         App.Agot2.Title = App.Title;
                         if (ClientGame != null)
                         {
                             ClientGame.GetUserInfoTask.StopAddTask();
                             ClientGame = null;
                         }
                     }

                     _OnLineStatus = value;
                     OnPropertyChanged("OnLineStatus");
                     OnPropertyChanged("OnLineStatusImage");
                 }), DispatcherPriority.ApplicationIdle);
        }

        public double WorldScale => (WorldLayer.MapLayerScale * WorldLayer.ViewboxScale);

        private double _WorldAngle;
        public double WorldAngle
        {
            get => _WorldAngle;
            set
            {
                _WorldAngle = value;
                OnPropertyChanged("WorldAngle");
            }
        }


        private string _GamePassword;
        public string GamePassword
        {
            get => _GamePassword;
            set
            {
                _GamePassword = value;

                if (string.IsNullOrWhiteSpace(_GamePassword))
                    NoTimer = false;

                OnPropertyChanged("GamePassword");
                OnPropertyChanged("NoTimerVisibility");
            }
        }
        public static readonly int NoTimerMaxTime = 172800;
        private bool _noTimer;
        public bool NoTimer
        {
            get => _noTimer;
            set
            {
                _noTimer = value;

                GameSettings.NoTimer = _noTimer;
                GameSettings.MaxTime = _noTimer ? NoTimerMaxTime : 600;
                GameSettings.AddTime = _noTimer ? NoTimerMaxTime : 150;
                OnPropertyChanged("GameSettings");

                OnPropertyChanged("NoTimer");
                OnPropertyChanged("TimerSettingsVisibility");
            }
        }
        public Visibility NoTimerVisibility => string.IsNullOrWhiteSpace(_GamePassword) ? Visibility.Hidden : Visibility.Visible;
        public Visibility TimerSettingsVisibility => !string.IsNullOrWhiteSpace(_GamePassword) && NoTimer ? Visibility.Hidden : Visibility.Visible;
        public string LangImage => GameSettings.LangImage;
        public WCFGameSettings GameSettings { get; set; }

        private ExtStaticData _WorldData;
        public ExtStaticData WorldData
        {
            get => _WorldData;
            set
            {
                _WorldData = value;
                OnPropertyChanged("WorldData");
            }
        }

        public string HomeSelected { get; set; }

        private Guid? _ClientGameId;
        public Guid? ClientGameId
        {
            get => _ClientGameId;
            set => Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                 {
                     if (_ClientGameId != value && value.HasValue)
                     {
                         DisConnect();
                         if (_ClientGameId != null)
                             return;
                     }

                     if (value.HasValue)
                         CheckBlackListTask.AddTask(value.Value, () => _ConnectToGame(value.Value));
                     else
                         ClearGameData();
                 }), DispatcherPriority.ApplicationIdle);
        }

        private void _ConnectToGame(Guid gameId)
        {
            ConnectTask.AddTask(gameId);
            _ClientGameId = gameId;
            OnPropertyChanged("ClientGameId");
        }

        private ExtGame _ClientGame;

        public ExtGame ClientGame
        {
            get => _ClientGame;
            set => Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                 {
                     OnClientGameChanging(value);

                     //если null то удаляет отрисованные элементы игры из лобби
                     OnPropertyChanged("ClientGame");
                 }), DispatcherPriority.ApplicationIdle);
        }

        public void DisConnect(bool isClosing = false)
        {
            if (ClientGame == null)
            {
                if (isClosing)
                    Environment.Exit(555);
                else
                    return;
            }

            if (ClientGame.WCFGame.CloseTime == null && ClientGame.ClientGameUser.WCFGameUser.HomeType != null)
            {
                App.Agot2.questionView.Show(MessageBoxButton.YesNoCancel, string.Format(App.GetResources("text_leaveGame"), ClientGame.WCFGame.Settings.MaxTime),
                //Yes Action
                () =>
                {
                    ClientGame.DisConnectTask.AddTask(ClientGame.ClientGameUser.WCFGameUser, isClosing);
                    App.Settings.Value.LastGameId = null;
#if DEBUG
                    App.Settings.Write();
#endif
                    if (isClosing)
                        Environment.Exit(555);

                    ClearGameData();
                },
                //No Action
                () =>
                {
                    if (isClosing)
                        Environment.Exit(555);

                    ClearGameData();

                    //if (ClientGame.WCFGame.Settings.NoTimer)
                    //{
                        MainWindow.ClientInfo.ReturnTimer.Start();
                        MainWindow.ClientInfo.OnPropertyChanged("ReturnTimer");
                    //}
                }
                );
            }
            else
            {
                if (isClosing)
                    Environment.Exit(555);

                ClearGameData();
            }
        }

        private void ClearGameData()
        {
            _ClientGameId = null;
            OnPropertyChanged("ClientGameId");
            OnLineStatus = false;
        }
    }
}
