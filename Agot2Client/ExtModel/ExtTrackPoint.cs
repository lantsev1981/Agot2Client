using System.Linq;
using GameService;

namespace Agot2Client
{
    public partial class ExtTrackPoint
    {
        public WCFTrackPoint WCFTrackPoint { get; private set; }

        public WCFGamePoint GamePoint { get; private set; }

        public ExtTrackPoint(WCFStaticData wcfStaticData, WCFTrackPoint wcfTrackPoint)
        {
            WCFTrackPoint = wcfTrackPoint;

            GamePoint = wcfStaticData.GamePoint
                .Single(p => p.Id == WCFTrackPoint.GamePoint);
        }
    }
}
