using System;
using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Interaction logic for StepBtn.xaml
    /// </summary>
    public partial class StepBtn : UserControl
    {
        static public Action DeleteStepTimer { get; private set; }

        public ExtGame Game { get; private set; }
        public StepTimer StepTimer { get; private set; }

        public StepBtn()
        {
            InitializeComponent();

            StepTimer = new StepTimer();
            DeleteStepTimer = () => StepTimer._GameTimerList.Remove(StepTimer.GameTimer);
            timerBtn.DataContext = StepTimer;

            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.ClientStepCganged -= ExtGame_ClientStepCgange;
            }
            if (game != null)
            {
                Game = game;
                Game.ClientStepCganged += ExtGame_ClientStepCgange;
            }

            StepTimer.Stop();
        }

        private void ExtGame_ClientStepCgange(ExtStep step)
        {
            if (step == null)
                StepTimer.Stop();
            else
                StepTimer.Start(step);
        }

        private void _IsFull_Button_Click(object sender, RoutedEventArgs e)
        {
            GameView.CompleteStep(Game);
        }
    }
}
