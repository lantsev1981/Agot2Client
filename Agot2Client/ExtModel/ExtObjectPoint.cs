using System.Linq;

using GameService;

namespace Agot2Client
{

    public class ExtObjectPoint
    {
        public WCFObjectPoint WCFObjectPoint { get; private set; }

        public WCFGamePoint GamePoint { get; private set; }

        public ExtObjectPoint(WCFStaticData staticData, WCFObjectPoint wcfObjectPoint)
        {
            WCFObjectPoint = wcfObjectPoint;

            GamePoint = staticData.GamePoint
                .Single(p => p.Id == WCFObjectPoint.GamePoint);
        }
    }
}
