using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для BattleView.xaml
    /// </summary>
    public partial class BattleView : UserControl
    {

        BattleViewModel _model;
        public BattleView()
        {
            InitializeComponent();
            this.DataContext = _model = new BattleViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GameView.CompleteStep(_model.Game);
        }

        private void StackPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _model.Game.OnToHomeCardEvent();
        }
    }

    public class BattleViewModel : MyMod.MyNotifyObj
    {
        bool _IsCompleate;

        ExtBattle _Battle;
        public ExtBattle Battle
        {
            get { return _Battle; }
            private set
            {
                //Всегда показываем последний бой
                if (value == null)
                    return;
                //не меняем если без изменений
                if (_Battle != null && _Battle.WCFBattle.Id == value.WCFBattle.Id)
                    return;

                SetBattle(value);
                Game.OnBattleDataChanged(_IsCompleate = false);
            }
        }

        private void SetBattle(ExtBattle value)
        {
            _Battle = value;
            _AttackInfo = null;
            _DefenceInfo = null;
            this.OnPropertyChanged("Battle");
        }

        ExtBattleUser _AttackInfo;
        public ExtBattleUser AttackInfo
        {
            get { return _AttackInfo; }
            private set
            {
                if (value == null || value.WCFBattleUser.BattleId != Battle.WCFBattle.Id)
                    return;

                //не меняем если без изменений
                if (_AttackInfo != null && _AttackInfo.WCFBattleUser.Step == value.WCFBattleUser.Step)
                    return;

                if (_AttackInfo == null || _AttackInfo.WCFBattleUser.IsWinner == null)
                {
                    _AttackInfo = value;
                    this.OnPropertyChanged("AttackInfo");

                    if (value.WCFBattleUser.IsWinner == null)
                    {
                        var order = value.Step.ExtGameUserInfo?.ExtOrder.SingleOrDefault(p => p.WCFOrder.Terrain == Battle.WCFBattle.AttackTerrain);
                        AttackOrderFileName = order?.WCFOrder.OrderType?.Contains("Поход") == true ? order.ImageName : null;
                        this.OnPropertyChanged("AttackOrderFileName");
                    }
                }
                else
                    Game.OnBattleDataChanged(_IsCompleate = true);
            }
        }

        ExtBattleUser _DefenceInfo;
        public ExtBattleUser DefenceInfo
        {
            get { return _DefenceInfo; }
            private set
            {
                if (value == null || value.WCFBattleUser.BattleId != Battle.WCFBattle.Id)
                    return;

                //не меняем если без изменений
                if (_DefenceInfo != null && _DefenceInfo.WCFBattleUser.Step == value.WCFBattleUser.Step)
                    return;

                if (_DefenceInfo == null || !_DefenceInfo.WCFBattleUser.IsWinner.HasValue)
                {
                    _DefenceInfo = value;
                    this.OnPropertyChanged("DefenceInfo");

                    if (!value.WCFBattleUser.IsWinner.HasValue)
                    {
                        var order = value.Step.ExtGameUserInfo?.ExtOrder.SingleOrDefault(p => p.WCFOrder.Terrain == Battle.WCFBattle.DefenceTerrain);
                        DefenceOrderFileName = order?.WCFOrder.OrderType?.Contains("Оборона") == true ? order.ImageName : null;
                        this.OnPropertyChanged("DefenceOrderFileName");
                        var garrison = value.Step.Game.ViewGameInfo.ExtGarrison.SingleOrDefault(p => p.WCFGarrison.Terrain == Battle.WCFBattle.DefenceTerrain);
                        GarrisonFileName = garrison == null ? null : garrison.ImageName;
                        this.OnPropertyChanged("GarrisonFileName");
                    }
                }
                else
                    Game.OnBattleDataChanged(_IsCompleate = true);
            }
        }

        public List<ExtGameUser> AttackSupport
        {
            get
            {
                if (Game == null)
                    return null;

                return Game.ViewUserStep
                    .Where(p => p.ExtSupport != null && p.ExtSupport.WCFSupport.BattleId == Battle.WCFBattle.Id && p.ExtSupport.WCFSupport.SupportUser == Battle.WCFBattle.AttackUser)
                    .Select(p => p.ExtGameUser).ToList();
            }
        }

        public List<ExtGameUser> DefenceSupport
        {
            get
            {
                if (Game == null)
                    return null;

                return Game.ViewUserStep
                    .Where(p => p.ExtSupport != null && p.ExtSupport.WCFSupport.BattleId == Battle.WCFBattle.Id && p.ExtSupport.WCFSupport.SupportUser == Battle.WCFBattle.DefenceUser)
                    .Select(p => p.ExtGameUser).ToList();
            }
        }

        public List<ExtGameUser> SupportUser
        {
            get
            {
                if (Game == null)
                    return null;

                return Game.ViewUserStep
                    .Where(p => p.ExtSupport != null && p.ExtSupport.WCFSupport.BattleId == Battle.WCFBattle.Id && p.ExtSupport.WCFSupport.SupportUser == null)
                    .Select(p => p.ExtGameUser).ToList();
            }
        }

        public Visibility BladeVisibility
        {
            get
            {
                if (Game == null)
                    return Visibility.Collapsed;

                return Game.ViewUserStep.Any(p => p.ExtGameUserInfo != null && p.ExtGameUserInfo.WCFGameUserInfo.IsBladeUse)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }
        public string GarrisonFileName { get; private set; }
        public string AttackOrderFileName { get; private set; }
        public string DefenceOrderFileName { get; private set; }

        public ExtGame Game { get; private set; }

        public BattleViewModel()
        {
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.CurrentViewKeyCganged -= ExtGame_CurrentViewKeyCgange;
            }
            if (game != null)
            {
                Game = game;
                Game.CurrentViewKeyCganged += ExtGame_CurrentViewKeyCgange;
            }

            SetBattle(null);
        }

        void ExtGame_CurrentViewKeyCgange()
        {
            this.OnPropertyChanged("BladeVisibility");

            if (Game.ViewGameInfo == null || Game.ViewGameInfo.ExtBattle == null)
                return;

            Battle = Game.ViewGameInfo.ExtBattle;
            //if (Battle.AttackUser.ViewStep == null || Battle.DefenceUser.ViewStep == null)
            //    return;

            if (_IsCompleate || Battle.AttackUser.ViewStep == null || Battle.DefenceUser.ViewStep == null)
                return;

            AttackInfo = Battle.AttackUser.ViewStep.ExtBattleUser;
            DefenceInfo = Battle.DefenceUser.ViewStep.ExtBattleUser;

            this.OnPropertyChanged("AttackSupport");
            this.OnPropertyChanged("DefenceSupport");
            this.OnPropertyChanged("SupportUser");
        }
    }
}
