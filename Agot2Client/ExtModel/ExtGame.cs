using GamePortal;
using GameService;
using MyMod;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Agot2Client
{
    public class ExtGame : MyNotifyObj
    {
        public IGameService GameService { get; private set; }
        public GetStepTask GetStepTask { get; private set; }
        public GetUserInfoTask GetUserInfoTask { get; private set; }
        public DisConnectTask DisConnectTask { get; private set; }

        #region Events
        public event Action<ExtStep> ClientStepCganged;
        private void OnClientStepCganged(ExtStep newValue)
        {
            ClientStepCganged?.Invoke(newValue);
        }

        public event Action CurrentViewKeyCganged;
        private void OnCurrentViewKeyCganged()
        {
            CurrentViewKeyCganged?.Invoke();
        }

        public event Action<ExtOrder> SelecteOrderChanged;
        private void OnSelecteOrderChanged(ExtOrder newValue)
        {
            SelecteOrderChanged?.Invoke(newValue);
        }

        public event Action NewWesterosPhase;
        public void OnNewWesterosPhase()
        {
            NewWesterosPhase?.Invoke();
        }

        public event Action NewPlanningPhase;
        public void OnNewPlanningPhase()
        {
            NewPlanningPhase?.Invoke();
        }

        public event Action<ExtGameUser, bool> GameUserEvent;

        private void OnGameUserEvent(ExtGameUser user, bool type)
        {
            GameUserEvent?.Invoke(user, type);
        }

        public event Action ToHomeCardEvent;
        public void OnToHomeCardEvent()
        {
            ToHomeCardEvent?.Invoke();
        }

        public event Action<bool> BattleDataChanged;
        public void OnBattleDataChanged(bool isCompleate)
        {
            BattleDataChanged?.Invoke(isCompleate);
        }

        public event Action HomeCardSelected;
        public void OnHomeCardSelected()
        {
            HomeCardSelected?.Invoke();
        }
        #endregion

        #region ViewModels
        public ObservableCollection<ArrowViewModel> CurrentArrows { get; private set; }
        public LogListViewModel LogListViewModel { get; private set; }

        public bool VisibilityNextBarbarian { get; set; }

        private string _NextBarbarian;
        public string NextBarbarian
        {
            get => _NextBarbarian;
            set
            {
                _NextBarbarian = value != null
                    ? App.GetResources("image_" + value)
                    : null;
                this.OnPropertyChanged("NextBarbarian");
                VisibilityNextBarbarian = _NextBarbarian == null ? false : true;
                this.OnPropertyChanged("VisibilityNextBarbarian");
            }
        }

        public List<ExtHomeType> HomeType { get; private set; }
        #endregion

        //TODO при обновлении списка игры постоянно пересоздаются :(
        public WCFGame WCFGame { get; private set; }
        public string ExtGameName { get; private set; }
        public GameTypeItem GameTypeItem { get; private set; }
        public Visibility RavenOverlayVisability { get; private set; }
        public Brush FillBrush { get; private set; }
        public ExtGameUser ClientGameUser { get; private set; }
        public List<ExtStep> ViewUserStep { get; private set; }
        public ExtGameInfo ViewGameInfo { get; private set; }
        public IEnumerable<ExtGameUserInfo> ViewGameUserInfo { get; private set; }
        public IEnumerable<ExtOrder> ViewOrder { get; private set; }
        public List<ExtGameUser> ExtGameUser { get; set; }
        public ExtStep VesterosLastViewStep
        {
            get
            {
                if (ViewUserStep == null)
                    return null;

                return ViewUserStep.Single(p => p.ExtGameUser.WCFGameUser.Login == "Вестерос");
            }
        }
        public string BarbarianStrength => VesterosLastViewStep == null ? null : string.Format(App.GetResources("text_barbarianStrength"), VesterosLastViewStep.WCFStep.GameInfo.Barbarian);
        public ExtStep LastStep => AllStep[LastStepIndex];
        public int LastStepIndex => AllStep.Count == 0 ? 0 : AllStep.Last().Key;
        public WCFUser Creator => MainWindow.GamePortal.GetUser(this.WCFGame.Settings.CreatorLogin);

        private SortedList<int, ExtStep> AllStep = new SortedList<int, ExtStep>();

        public ExtGame(WCFGame wcfGame, IGameService service)
        {
            #region ViewModels
            CurrentArrows = new ObservableCollection<ArrowViewModel>();
            LogListViewModel = new LogListViewModel();
            #endregion

            WCFGame = wcfGame;

            ExtGameUser = new List<ExtGameUser>();
            UpdateGameUsers(WCFGame.GameUser);

            ExtGameName = string.IsNullOrEmpty(WCFGame.Settings.Name)
                    ? App.GetResources("text_NewGame")
                    : WCFGame.Settings.Name;
            GameTypeItem = MainWindow.GameTypes.First(p => p.Id == wcfGame.Settings.GameType);
            RavenOverlayVisability = GameTypeItem.PlayerCount < 5 ? Visibility.Visible : Visibility.Collapsed;


            FillBrush = GetFillBrush();

            GameService = service;
            DisConnectTask = new DisConnectTask(this);
            GetStepTask = new GetStepTask(this);
            GetUserInfoTask = new GetUserInfoTask(this);
        }

        public IEnumerable<ExtUnit> ViewUnit => CurrentViewKey == 0
                    ? null
                    : ViewGameUserInfo.SelectMany(p => p.ExtUnit);

        public IEnumerable<ExtPowerCounter> ViewPowerCounter => CurrentViewKey == 0
                    ? null
                    : ViewGameUserInfo.SelectMany(p => p.ExtPowerCounter);

        public Brush GetFillBrush()
        {
            if (WCFGame.OpenTime == null)
                return new SolidColorBrush(Colors.LightGreen) { Opacity = .5 };

            if (WCFGame.CloseTime != null)
                return new SolidColorBrush(Colors.Orange) { Opacity = .5 };

            if (WCFGame.GameUser.Any(p => !string.IsNullOrEmpty(p.HomeType) && string.IsNullOrEmpty(p.Login)))
                return new SolidColorBrush(Colors.Yellow) { Opacity = .5 };

            return new SolidColorBrush(Colors.WhiteSmoke) { Opacity = .5 };
        }

        private int _CurrentViewKey;
        public int CurrentViewKey
        {
            get => _CurrentViewKey;
            set
            {
                {
                    _CurrentViewKey = value;

                    ViewUserStep = ExtGameUser.Select(p => p.LastStep)
                        .Where(p => p != null)
                        .OrderBy(p => p.WCFStep.Id)
                        .ToList();
                    //ViewUserStep.Count() < 7 ? null :
                    ViewGameInfo = ViewUserStep.Single(p => p.ExtGameInfo != null).ExtGameInfo;
                    ViewGameUserInfo = ViewUserStep.Where(p => p.ExtGameUserInfo != null).Select(p => p.ExtGameUserInfo).ToList();
                    ViewOrder = ViewGameUserInfo.SelectMany(p => p.ExtOrder).ToList();

                    App.Agot2.gameView.worldLayer.LayoutUpdated += worldLayer_LayoutUpdated;
                    OnCurrentViewKeyCganged();
                    MainWindow.ClientInfo.OnPropertyChanged("ClientGame");

                    ClientStep = ViewUserStep.FirstOrDefault(p => p.IsClientStep);

                    SyncArrow();
                }
            }
        }

        private void SyncArrow()
        {
            List<ArrowModel> arrowModelList = AllStep[_CurrentViewKey].WCFStep.ArrowModelList;

            //удаляем устаревшие стрелки
            foreach (ArrowViewModel item in CurrentArrows.ToArray())
            {
                if (!arrowModelList.Any(p => p.FirstId == item.Model.FirstId))
                    CurrentArrows.Remove(item);
            }
            //Добавляем новые стрелки
            foreach (ArrowModel item in arrowModelList)
            {
                if (!CurrentArrows.Any(p => p.Model.FirstId == item.FirstId))
                    CurrentArrows.Add(new ArrowViewModel(item));
            }
        }

        private void worldLayer_LayoutUpdated(object sender, EventArgs e)
        {
            App.Agot2.gameView.worldLayer.LayoutUpdated -= worldLayer_LayoutUpdated;

            //Если игра завершена и вы игрок
            if (this.WCFGame.CloseTime == null
                && !string.IsNullOrEmpty(ClientGameUser.WCFGameUser.HomeType)
                && ClientGameUser.LastStep.WCFStep.StepType == "Победа")
            {
                bool winner = ClientGameUser.LastStep.WCFStep.Raven.StepType == "1";

                //обновляем список и отправляем на донат победителя и бездомных
                App.Agot2.leftPanelView.leaderBoardView.UsersUpdate();
                if (ClientGameUser.GPUser.AllPower < 100 || winner)
                    MainWindow.GamePortal.Donate();
            }
        }

        private ExtStep _ClientStep;
        public ExtStep ClientStep
        {
            get => _ClientStep;
            set => Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                 {
                     //исключаем повторение хода
                     if (_ClientStep == value)
                         return;

                     _SelectedOrder = null;

                     //сброс view
                     if (value == null)
                         NextBarbarian = null;
                     else
                     {
                         switch (value.WCFStep.StepType)
                         {
                             case "Разведка_за_стеной":
                                 NextBarbarian = value.WCFStep.Raven.StepType;
                                 break;

                             case "Серсея_Ланнистер":
                                 value.WCFStep.BattleUser.AdditionalEffect = string.Empty;
                                 foreach (ExtOrder item in ViewGameInfo.ExtBattle.ClientOpponent.LastStep.ExtGameUserInfo.ExtOrder)
                                     item.Opacity = 1;
                                 break;

                             case "Королева_Шипов":
                                 value.WCFStep.BattleUser.AdditionalEffect = string.Empty;
                                 IEnumerable<ExtOrder> viewOrder = ViewGameInfo.ExtBattle.ClientOpponent.LastStep.ExtGameUserInfo.ExtOrder;
                                 viewOrder = viewOrder.Where(p => p.ExtTerrain.JoinTerrainCol.Any(p1 => p1 == ViewGameInfo.ExtBattle.DefenceTerrain)).ToList();
                                 viewOrder = viewOrder.Where(p => p.ExtTerrain != ViewGameInfo.ExtBattle.AttackTerrain);
                                 foreach (ExtOrder item in viewOrder)
                                     item.Opacity = 1;
                                 break;

                             case "Подмога":
                                 if (!value.WCFStep.IsFull)
                                 {
                                     if (value.ExtGameUser.WCFGameUser.Id != ViewGameInfo.ExtBattle.WCFBattle.DefenceUser && ViewGameInfo.ExtBattle.WCFBattle.IsAttackUserNeedSupport)
                                         ViewGameInfo.ExtBattle.AttackUser.ExtHomeType.SupportVisibility = Visibility.Visible;

                                     if (value.ExtGameUser.WCFGameUser.Id != ViewGameInfo.ExtBattle.WCFBattle.AttackUser && ViewGameInfo.ExtBattle.WCFBattle.IsDefenceUserNeedSupport == true)
                                         ViewGameInfo.ExtBattle.DefenceUser.ExtHomeType.SupportVisibility = Visibility.Visible;
                                 }
                                 break;
                             case "RequestSupport":
                                 App.Agot2.questionView.Show(MessageBoxButton.YesNo, App.GetResources("text_RequestSupport"),
                                       () => { value.WCFStep.IsNeedSupport = true; SendStepTask.AddTask(this); },
                                       () => { value.WCFStep.IsNeedSupport = false; SendStepTask.AddTask(this); });
                                 break;

                             case "Железный_трон":
                                 if (ViewUserStep.Any(p => p.WCFStep.Voting != null && p.WCFStep.Voting.Target == "Одичалые"))
                                 {
                                     foreach (string item in value.WCFStep.Raven.StepType.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                                     {
                                         ExtGameUserInfo home = ViewGameUserInfo.SingleOrDefault(p => p.Step.ExtGameUser.WCFGameUser.HomeType == item);
                                         if (home != null)
                                             home.BarbarianVisibility = Visibility.Visible;
                                         else
                                             NextBarbarian = item;
                                     }
                                 }
                                 break;

                         }
                     }

                     OnClientStepCganged(_ClientStep = value);
                     this.OnPropertyChanged("ClientStep");
                 }), DispatcherPriority.ApplicationIdle);
        }

        public ExtStep GetUserViewStep(ExtGameUser user)
        {
            return ViewUserStep.SingleOrDefault(p => p.ExtGameUser.WCFGameUser.Id == user.WCFGameUser.Id);
        }

        public KeyValuePair<int, ExtStep> GetUserStep(ExtGameUser user)
        {
            return AllStep.LastOrDefault(p => p.Value.ExtGameUser.WCFGameUser.Id == user.WCFGameUser.Id && p.Value.WCFStep.Id <= CurrentViewKey);
        }

        public KeyValuePair<int, ExtStep> GetUserLastVoting(ExtGameUser user, string votingType)
        {
            return AllStep.LastOrDefault(p => p.Value.WCFStep.Voting?.Target == votingType && p.Value.ExtGameUser.WCFGameUser.Id == user.WCFGameUser.Id && p.Value.WCFStep.Id <= CurrentViewKey);
        }

        public ExtStep GetStep(int id)
        {
            AllStep.TryGetValue(id, out ExtStep step);
            return step;
        }

        private ExtOrder _SelectedOrder;
        public ExtOrder SelectedOrder
        {
            get => _SelectedOrder;
            set
            {
                ExtOrder oldValue = value;

                //передаём информацию всем OrderView
                OnSelecteOrderChanged(_SelectedOrder = value);

                ClientStep.ExtGameUserInfo.ExtUnit.RemoveAll(p => p.WCFUnit.Id == Guid.Empty);
                foreach (ExtUnit item in ClientStep.ExtGameUserInfo.ExtUnit)
                {
                    item.IsSelected = false;
                    item.TempTerrain = null;
                    item.TempUnitType = null;
                }
                OnPropertyChanged("ViewUnit");

                ClientStep.ExtGameUserInfo.ExtPowerCounter.RemoveAll(p => p.IsTemp);
                OnPropertyChanged("ViewPowerCounter");

                if (_SelectedOrder != null && _SelectedOrder.WCFOrder.OrderType != null)
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), string.Format(App.GetResources("text_selectedOrder"), App.GetResources("orderType_" + _SelectedOrder.WCFOrder.OrderType)), 2);
            }
        }

        private void UpdateGameUser(WCFGameUser serverUser)
        {
            if (serverUser.Game != WCFGame.Id)
                return;

            ExtGameUser clientUser = ExtGameUser.SingleOrDefault(p => p.WCFGameUser.Id == serverUser.Id);

            //Добавляем пользователя
            if (clientUser == null)
            {
                clientUser = new ExtGameUser(this, serverUser);
                ExtGameUser.Add(clientUser);

                //Сообщаем об изменениях
                if (!string.IsNullOrEmpty(serverUser.Login) && serverUser.Login != "Вестерос")
                    OnGameUserEvent(clientUser, true);

                //текущий пользователь
                if (MainWindow.GamePortal.User.Login == serverUser.Login)
                    ClientGameUser = clientUser;
            }
            //Синхронизируем пользователя
            else
            {
                bool flag = string.IsNullOrEmpty(serverUser.Login) != string.IsNullOrEmpty(clientUser.WCFGameUser.Login);
                clientUser.Sync(serverUser);

                //Сообщаем об изменениях
                if (flag && serverUser.Login != "Вестерос")
                    OnGameUserEvent(clientUser, !string.IsNullOrEmpty(serverUser.Login));
            }
        }

        public void UpdateGameUsers(List<WCFGameUser> items)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //сообщаем о устаревшем пользователе
                ExtGameUser.ToList().ForEach(p =>
                {
                    if (!items.Any(p1 => p1.Id == p.WCFGameUser.Id))
                    {
                        OnGameUserEvent(p, false);
                        ExtGameUser.Remove(p);
                    }
                });

                items.ForEach(p => UpdateGameUser(p));
                if (HomeType == null)
                    HomeType = ExtGameUser.Where(p => p.ExtHomeType != null).Select(p => p.ExtHomeType).OrderBy(p => p.Sort).ToList();

                //скрываем профиль игрока
                if (this.WCFGame.CloseTime == null)
                    MainWindow.GamePortal.SetLeaderBoardVisibility(items.Where(p => p.HomeType != null && p.Login != null && p.Login != "Вестерос").ToList(), false);
            }), DispatcherPriority.ApplicationIdle).Wait();
        }

        public bool CheckAllowStep(int stepKey)
        {
            return AllStep.ContainsKey(stepKey) ? true : false;
        }

        public bool AddStep(WCFStep wcfStep)
        {
            if (wcfStep.Game != WCFGame.Id)
                return false;

            if (AllStep.ContainsKey(wcfStep.Id))
                return false;

            ExtStep step = new ExtStep(wcfStep, this);
            AllStep.Add(wcfStep.Id, step);
            if (step.WCFStep.Message.Count > 0)
            {
                LogListViewModel.Insert(0, new LogItemViewModel(step));
            }

            return true;
        }
    }
}