using System.Windows;
using GameService;

namespace Agot2Client
{
    public class ExtSupport
    {
        public ExtStep Step { get; private set; }
        public WCFSupport WCFSupport { get; private set; }
        public ExtGameUser User { get; private set; }

        public ExtSupport(ExtStep step, WCFSupport wcfSupport)
        {
            Step = step;
            WCFSupport = wcfSupport;
            User = step.ExtGameUser;

            UpdateView();
        }

        private void UpdateView()
        {
            if (Step.Game.ClientStep == Step)
            {
                if (Step.WCFStep.StepType == "Подмога" && !Step.WCFStep.IsFull)
                {
                    if (Step.ExtGameUser.WCFGameUser.Id != Step.Game.ViewGameInfo.ExtBattle.WCFBattle.DefenceUser && Step.Game.ViewGameInfo.ExtBattle.WCFBattle.IsAttackUserNeedSupport)
                        Step.Game.ViewGameInfo.ExtBattle.AttackUser.ExtHomeType.SupportVisibility = Visibility.Visible;

                    if (Step.ExtGameUser.WCFGameUser.Id != Step.Game.ViewGameInfo.ExtBattle.WCFBattle.AttackUser && Step.Game.ViewGameInfo.ExtBattle.WCFBattle.IsDefenceUserNeedSupport == true)
                        Step.Game.ViewGameInfo.ExtBattle.DefenceUser.ExtHomeType.SupportVisibility = Visibility.Visible;
                }
                else
                {
                    foreach (var item in MainWindow.ClientInfo.WorldData.HomeType)
                        item.SupportVisibility = Visibility.Collapsed;
                }
            }
        }
    }
}
