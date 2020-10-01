using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для BarbarianView.xaml
    /// </summary>
    public partial class BarbarianView : UserControl
    {
        public ExtGame Game { get; private set; }
        public ExtStep ClientStep { get; private set; }

        public BarbarianView()
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

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientStep == null)
                return;

            ClientStep.WCFStep.Raven.StepType = "Yes";
            GameView.CompleteStep(Game);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientStep == null)
                return;

            ClientStep.WCFStep.Raven.StepType = "No";
            GameView.CompleteStep(Game);
        }
    }
}
