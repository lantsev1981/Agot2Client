using System.Collections.Generic;
using System.Linq;

using GameService;

namespace Agot2Client
{
    public class ExtUnitType
    {
        public string Key { get { return Keys[WCFUnitType.Name]; } }
        static public readonly Dictionary<string, string> Keys;
        static ExtUnitType()
        {
            Keys = new Dictionary<string, string>();
            Keys.Add("Корабль", "ship");
            Keys.Add("Пеший_воин", "foot");
            Keys.Add("Рыцарь", "knight");
            Keys.Add("Осадная_башня", "tower");
        }

        public ExtGame Game { get; private set; }
        public WCFUnitType WCFUnitType { get; private set; }

        public ExtUnitType(WCFUnitType wcfUnitType)
        {
            WCFUnitType = wcfUnitType;
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            Game = game;
        }

        public bool CheckUnitTypeCount(ExtOrder extOrder)
        {
            return extOrder.Step.ExtGameUserInfo.ExtUnit
                .Count(p => p.TempUnitType == this) < this.WCFUnitType.Count
                ? true
                : false;
        }

        //используется в UnitTypeMenu
        public string ImageName
        {
            get
            {
                if (Game == null)
                    return null;

                return string.Format(Game.ClientGameUser.ExtHomeType.UnitImageFormate, Key);
            }
        }
    }
}
