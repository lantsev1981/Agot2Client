using System.Linq;
using GameService;
using System.Windows;

namespace Agot2Client
{
    public class ExtTokenPoint
    {
        public WCFTokenPoint WCFTokenPoint { get; private set; }

        public WCFGamePoint WCFGamePoint { get; private set; }

        public ExtTokenPoint(WCFStaticData wcfStaticData, WCFTokenPoint wcfTokenPoint)
        {
            WCFTokenPoint = wcfTokenPoint;

            WCFGamePoint = wcfStaticData.GamePoint
                .Single(p => p.Id == WCFTokenPoint.GamePoint);
        }
                
        static public Point GetPoint(string terrainName, string tokenType)
        {
            var point = MainWindow.ClientInfo.WorldData.TokenPoint.Single(p => p.WCFTokenPoint.Terrain == terrainName && p.WCFTokenPoint.TokenType == "Приказ").WCFGamePoint;
            return new Point(point.X, point.Y);
        }
    }
}
