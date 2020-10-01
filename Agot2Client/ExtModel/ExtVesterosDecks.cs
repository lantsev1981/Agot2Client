using System.Linq;
using GameService;
using System.Windows;

namespace Agot2Client
{
    public class ExtVesterosDecks : MyMod.MyNotifyObj
    {
        public ExtStep Step { get; private set; }
        public WCFVesterosDecks WCFVesterosDecks { get; private set; }
        public WCFVesterosCardType WCFVesterosCardType { get; private set; }
        public int IndexOf { get; set; }


        public string ImageName { get; private set; }
        public string Name { get; private set; }

        public ExtVesterosDecks(ExtStep step, WCFVesterosDecks wcfVesterosDecks)
        {
            Step = step;
            WCFVesterosDecks = wcfVesterosDecks;
            WCFVesterosCardType = MainWindow.ClientInfo.WorldData.WCFStaticData.VesterosCardType.Single(p => p.Id == WCFVesterosDecks.VesterosCardType);
            ImageName = App.GetResources("image_" + WCFVesterosDecks.VesterosCardType);
            Name = App.GetResources("event_" + WCFVesterosDecks.VesterosCardType);

            ActionVisibility = Visibility.Collapsed;
            Step.Game.ClientStepCganged += Game_ClientStepCganged;
        }

        public Visibility ActionVisibility { get; private set; }
        private void Game_ClientStepCganged(ExtStep step)
        {
            if (step == null || step.WCFStep.StepType != "Событие_Вестероса" || step.WCFStep.VesterosAction.VesterosDecks != WCFVesterosDecks.Id)
                ActionVisibility = Visibility.Collapsed;
            else
                ActionVisibility = Visibility.Visible;

            OnPropertyChanged("ActionVisibility");
        }
    }
}
