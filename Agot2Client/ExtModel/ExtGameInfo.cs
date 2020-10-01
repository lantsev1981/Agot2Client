using GameService;
using System.Collections.Generic;
using System.Linq;

namespace Agot2Client
{
    public class ExtGameInfo
    {
        public ExtStep Step { get; private set; }
        public WCFGameInfo WCFGameInfo { get; private set; }

        public ExtBattle ExtBattle { get; private set; }
        public List<ExtGarrison> ExtGarrison { get; private set; }
        public List<ExtVesterosDecks> ExtVesterosDecks { get; private set; }
        public WCFGamePoint TurnPosition { get; private set; }
        public WCFGamePoint BarbarianPosition { get; private set; }
        public WCFGamePoint RavenOverlayPosition { get; private set; }

        public ExtGameInfo(ExtStep step, WCFGameInfo wcfGameInfo)
        {
            Step = step;
            WCFGameInfo = wcfGameInfo;

            if (wcfGameInfo.Battle != null)
                ExtBattle = new ExtBattle(Step, wcfGameInfo.Battle);

            ExtGarrison = wcfGameInfo.Garrison
                .Select(p => new ExtGarrison(p))
                .ToList();

            ExtVesterosDecks = wcfGameInfo.VesterosDecks
                .Select(p => new ExtVesterosDecks(Step, p))
                .OrderBy(p => p.WCFVesterosDecks.Sort)
                .ToList();

            TurnPosition = MainWindow.ClientInfo.WorldData.TrackPoint.
                Single(p => p.WCFTrackPoint.TrackType == "Раунд" && p.WCFTrackPoint.Value == WCFGameInfo.Turn).
                GamePoint;

            BarbarianPosition = MainWindow.ClientInfo.WorldData.TrackPoint.
                Single(p => p.WCFTrackPoint.TrackType == "Одичалые" && p.WCFTrackPoint.Value == WCFGameInfo.Barbarian).
                GamePoint;

            RavenOverlayPosition = MainWindow.ClientInfo.WorldData.TrackPoint.
                Single(p => p.WCFTrackPoint.TrackType == "Королевский_двор" && p.WCFTrackPoint.Value == 1).
                GamePoint;

            if (Step.Game.ViewGameInfo != null && Step.Game.ViewGameInfo.WCFGameInfo.Turn != wcfGameInfo.Turn)
                Step.Game.OnNewWesterosPhase();
        }
    }
}
