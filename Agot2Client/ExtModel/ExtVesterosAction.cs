using System.Linq;
using GameService;

namespace Agot2Client
{
    public class ExtVesterosAction
    {
        public ExtStep Step { get; private set; }
        public WCFVesterosAction WCFVesterosAction { get; private set; }

        public ExtVesterosDecks ExtVesterosDecks { get; private set; }

        public ExtVesterosAction(ExtStep step, WCFVesterosAction wcfVesterosAction)
        {
            Step = step;
            WCFVesterosAction = wcfVesterosAction;

            ExtVesterosDecks = Step.Game.ViewGameInfo.ExtVesterosDecks.Single(p => p.WCFVesterosDecks.Id == WCFVesterosAction.VesterosDecks);
        }
    }
}
