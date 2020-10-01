using System.Linq;

using MyMod;
using GameService;

namespace Agot2Client
{
    public class ExtGarrison : MyNotifyObj, IPosition
    {
        public WCFGarrison WCFGarrison { get; private set; }

        public ExtTerrain ExtTerrain { get; private set; }
        public WCFGamePoint Position { get; set; }
        public string ImageName { get; private set; }

        public ExtGarrison(WCFGarrison wcfGarrison)
        {
            WCFGarrison = wcfGarrison;

            ExtTerrain = MainWindow.ClientInfo.WorldData.Terrain
                    .Single(p => p.WCFTerrain.Name == WCFGarrison.Terrain);
            var extTokinPoint = this.ExtTerrain.ExtTokenPoint
                    .SingleOrDefault(p => p.WCFTokenPoint.TokenType == "Гарнизон");
            if (extTokinPoint == null) extTokinPoint = this.ExtTerrain.ExtTokenPoint
                    .SingleOrDefault(p => p.WCFTokenPoint.TokenType == "Приказ");

            Position = extTokinPoint.WCFGamePoint;
            ImageName = string.Format("/Image/Defance/{0}-defance.png", WCFGarrison.Strength);
        }
    }
}
