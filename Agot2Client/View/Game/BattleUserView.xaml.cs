using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для BattleUser.xaml
    /// </summary>
    public partial class BattleUserView : UserControl
    {
        public ExtGame Game { get; private set; }
        public ExtStep ClientStep { get; private set; }

        public BattleUserView()
        {
            InitializeComponent();
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.ClientStepCganged -= ExtGame_ClientStepCganged;
            }
            if (game != null)
            {
                Game = game;
                Game.ClientStepCganged += ExtGame_ClientStepCganged;
            }
        }

        private void ExtGame_ClientStepCganged(ExtStep step)
        {
            ClientStep = step;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            var battleInfo = (ExtBattleUser)this.DataContext;
            ClientStep.ExtSupport.WCFSupport.SupportUser = battleInfo.Step.ExtGameUser.WCFGameUser.Id;
            GameView.CompleteStep(Game);
        }

        private void StrengthUp_Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            ClientStep.WCFStep.Raven.StepType = "Валирийский_меч";
            GameView.CompleteStep(Game);
        }

        private void ChangeCard_Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            ClientStep.WCFStep.Raven.StepType = "Карта_перевеса";
            GameView.CompleteStep(Game);
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            GameView.CompleteStep(Game);
        }

        private void Yes_Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            switch (ClientStep.WCFStep.StepType)
            {
                case "Тирион_Ланнистер":
                    ClientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = ClientStep.Game.ViewGameInfo.ExtBattle.ClientOpponent.LastStep.WCFStep.BattleUser.HomeCardType;
                    break;

                default:
                    ClientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = "Yes";
                    break;
            }

            GameView.CompleteStep(Game);
        }

        private void No_Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            ClientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = string.Empty;
            GameView.CompleteStep(Game);
        }

        private void BarbarianUpBtn(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            ClientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = "Up";

            GameView.CompleteStep(Game);
        }

        private void BarbarianDownBtn(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (ClientStep == null)
                return;

            ClientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = "Down";

            GameView.CompleteStep(Game);
        }
    }
}
