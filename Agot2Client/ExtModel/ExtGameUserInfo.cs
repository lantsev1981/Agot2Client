using GameService;
using MyMod;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Agot2Client
{
    public class ExtGameUserInfo : MyNotifyObj
    {
        public WCFGameUserInfo WCFGameUserInfo { get; private set; }
        public ExtStep Step { get; private set; }

        public List<ExtUnit> ExtUnit { get; private set; }
        public List<ExtPowerCounter> ExtPowerCounter { get; private set; }
        public IEnumerable<ExtOrder> ExtOrder { get; private set; }
        public IEnumerable<ExtTerrain> TerrainCol { get; private set; }
        public WCFGamePoint VictoryPosition { get; private set; }
        public WCFGamePoint SupplyPosition { get; private set; }
        public string ThroneVoting => $"{Step.Game.GetUserLastVoting(Step.ExtGameUser, "Железный_трон").Value?.WCFStep.Voting.PowerCount ?? 0}";//|{Step.ExtGameUserInfo.WCFGameUserInfo.Power}";
        public string BladeVoting => $"{Step.Game.GetUserLastVoting(Step.ExtGameUser, "Вотчины").Value?.WCFStep.Voting.PowerCount ?? 0}";//|{Step.ExtGameUserInfo.WCFGameUserInfo.Power}";
        public string RavenVoting => $"{Step.Game.GetUserLastVoting(Step.ExtGameUser, "Королевский_двор").Value?.WCFStep.Voting.PowerCount ?? 0}";//|{Step.ExtGameUserInfo.WCFGameUserInfo.Power}";


        public ExtGameUserInfo(ExtStep step, WCFGameUserInfo wcfGameUserInfo)
        {
            Step = step;
            WCFGameUserInfo = wcfGameUserInfo;

            ExtUnit = WCFGameUserInfo.Unit.
                Select(p => new ExtUnit(step, p))
                .ToList();
            ExtPowerCounter = WCFGameUserInfo.PowerCounter.
                Select(p => new ExtPowerCounter(step, p))
                .ToList();
            ExtOrder = wcfGameUserInfo.Order.
                Select(p => new ExtOrder(step, p))
                .ToList();
            TerrainCol = wcfGameUserInfo.GameUserTerrain.
                Select(p => p.ExtTerrain());

            VictoryPosition = GetVictoryPosition();
            SupplyPosition = GetSupplyPosition();

            //if (Step.WCFStep.Voting != null)
            //{
            //    switch (Step.WCFStep.Voting.Target)
            //    {
            //        case "Железный_трон": ThroneVoting = Step.WCFStep.Voting.PowerCount.ToString(); break;
            //        case "Вотчины": BladeVoting = Step.WCFStep.Voting.PowerCount.ToString(); break;
            //        case "Королевский_двор": RavenVoting = Step.WCFStep.Voting.PowerCount.ToString(); break;
            //    }
            //}
        }

        private WCFGamePoint GetVictoryPosition()
        {
            int victoryCount = TerrainCol.Count(p1 => p1.WCFTerrain.Strength > 0);
            if (victoryCount > 7) victoryCount = 7;
            if (victoryCount < 1) return new WCFGamePoint();

            return MainWindow.ClientInfo.WorldData.TrackPoint.
                Where(p => p.WCFTrackPoint.TrackType == "Победа"
                    && p.WCFTrackPoint.Value == victoryCount).
                ElementAt(WCFGameUserInfo.ThroneInfluence - 1).
                GamePoint;
        }

        private WCFGamePoint GetSupplyPosition()
        {
            return MainWindow.ClientInfo.WorldData.TrackPoint.
                Where(p => p.WCFTrackPoint.TrackType == "Снабжение"
                    && p.WCFTrackPoint.Value == WCFGameUserInfo.Supply).
                ElementAt(WCFGameUserInfo.ThroneInfluence - 1).
                GamePoint;

        }

        public WCFGamePoint ThronePosition => MainWindow.ClientInfo.WorldData.TrackPoint.
                    Single(p => p.WCFTrackPoint.TrackType == "Железный_трон"
                        && p.WCFTrackPoint.Value == WCFGameUserInfo.ThroneInfluence).
                    GamePoint;

        public WCFGamePoint BladePosition => MainWindow.ClientInfo.WorldData.TrackPoint.
                    Single(p => p.WCFTrackPoint.TrackType == "Вотчины"
                        && p.WCFTrackPoint.Value == WCFGameUserInfo.BladeInfluence).
                    GamePoint;

        public WCFGamePoint RavenPosition => MainWindow.ClientInfo.WorldData.TrackPoint.
                    Single(p => p.WCFTrackPoint.TrackType == "Королевский_двор"
                        && p.WCFTrackPoint.Value == WCFGameUserInfo.RavenInfluence).
                    GamePoint;

        private Visibility _BarbarianVisibility = Visibility.Collapsed;
        public Visibility BarbarianVisibility
        {
            get => _BarbarianVisibility;
            set
            {
                _BarbarianVisibility = value;
                this.OnPropertyChanged("BarbarianVisibility");
            }
        }
    }
}
