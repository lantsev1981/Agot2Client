using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для HomeDetail.xaml
    /// </summary>
    public partial class HomeDetail : UserControl
    {
        public ExtGame Game { get; private set; }
        ExtHomeType _HomeType;

        public HomeDetail()
        {
            InitializeComponent();
            DataContextChanged += HomeDetail_DataContextChanged;
            Game = MainWindow.ClientInfo.ClientGame;//TODO из-за tabcontrol который блокирует конструктор до первого обращения к вкладке
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            Game = game;
        }

        void HomeDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _HomeType = (ExtHomeType)e.NewValue;
        }

        private void HomeCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Game == null || Game.ClientStep == null)
                return;

            var grid = sender as Grid;
            if (grid == null)
                return;

            var homeCard = grid.DataContext as ExtHomeCardType;
            if (homeCard == null)
                return;

            //вернуть карту
            if (Game.ClientStep.WCFStep.StepType == "Наездники_на_мамонтах")
            {
                if (!homeCard.IsUsed || _HomeType.WCFHomeType.Name != Game.ClientGameUser.WCFGameUser.HomeType)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_accessDenied"));
                    return;
                }

                Game.ClientStep.WCFStep.Raven.StepType = homeCard.WCFHomeCardType.Name;
                GameView.CompleteStep(Game);
                return;
            }
            if (Game.ClientStep.WCFStep.StepType == "dragon_Melisandre")
            {
                if (!homeCard.IsUsed || _HomeType.WCFHomeType.Name != Game.ClientGameUser.WCFGameUser.HomeType
                    || homeCard.WCFHomeCardType.Strength > Game.ClientGameUser.LastStep.ExtGameUserInfo.WCFGameUserInfo.Power)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_accessDenied"));
                    return;
                }

                Game.ClientStep.WCFStep.BattleUser.AdditionalEffect = homeCard.WCFHomeCardType.Name;
                GameView.CompleteStep(Game);
                return;
            }

            if (Game.ClientStep.WCFStep.StepType == "Сражение" && (Game.ClientStep.WCFStep.BattleUser.AdditionalEffect?.EndsWith("dragon_Qyburn")??false))
            {
                if (!homeCard.IsUsed)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_accessDenied"));
                    return;
                }

                Game.ClientStep.WCFStep.BattleUser.HomeCardType = homeCard.WCFHomeCardType.Name;
                GameView.CompleteStep(Game);
                return;
            }

            //карта в сбросе
            if (homeCard.IsUsed)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_homeCard2"));
                return;
            }

            //сбросить карту
            if (Game.ClientStep.WCFStep.StepType == "Пестряк")
            {
                if (_HomeType.WCFHomeType.Name != Game.ViewGameInfo.ExtBattle.ClientOpponent.WCFGameUser.HomeType)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("text_accessDenied"));
                    return;
                }

                Game.ClientStep.WCFStep.BattleUser.AdditionalEffect = homeCard.WCFHomeCardType.Name;
                GameView.CompleteStep(Game);
                return;
            }

            //Только владелец в свой ход
            if (_HomeType.HomeGameUser.LastStep != Game.ClientStep)
                return;

            switch (Game.ClientStep.WCFStep.StepType)
            {
                case "Сражение":
                    Game.ClientStep.WCFStep.BattleUser.HomeCardType = homeCard.WCFHomeCardType.Name;
                    GameView.CompleteStep(Game);
                    break;
                case "Сбор_на_Молоководной":
                    Game.ClientStep.WCFStep.Raven.StepType = homeCard.WCFHomeCardType.Name;
                    GameView.CompleteStep(Game);
                    break;
            }

            Game.OnHomeCardSelected();
        }
    }
}
