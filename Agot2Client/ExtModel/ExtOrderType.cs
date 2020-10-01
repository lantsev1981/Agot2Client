using System.Collections.Generic;
using GameService;

namespace Agot2Client
{
    public class ExtOrderType
    {

        static public readonly Dictionary<string, string> Keys;
        static ExtOrderType()
        {
            Keys = new Dictionary<string, string>();
            Keys.Add("Набег_0", "raid_0");
            Keys.Add("Набег_0_специальный", "raid_0_special");
            Keys.Add("Оборона_1", "defense_1");
            Keys.Add("Оборона_2_специальный", "defense_2_special");
            Keys.Add("Подмога_0", "support_0");
            Keys.Add("Подмога_1_специальный", "support_1_special");
            Keys.Add("Поход_-1", "march_-1");
            Keys.Add("Поход_0", "march_0");
            Keys.Add("Поход_1_специальный", "march_1_special");
            Keys.Add("Усиление_власти_0", "consolidate_0");
            Keys.Add("Усиление_власти_0_специальный", "consolidate_0_special");
        }

        public string Key { get { return Keys[WCFOrderType.Name]; } }

        public WCFOrderType WCFOrderType { get; private set; }

        public string ImageName { get; private set; }

        public ExtOrderType(WCFOrderType wcfOrderType)
        {
            WCFOrderType = wcfOrderType;

            ImageName = string.Format("/Image/Order/{0}.png", Key); 
        }
    }
}
