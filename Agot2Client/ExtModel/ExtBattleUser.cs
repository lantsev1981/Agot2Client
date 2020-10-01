using System.Linq;
using GameService;
using MyMod;

namespace Agot2Client
{
    public class ExtBattleUser : MyNotifyObj
    {
        public WCFBattleUser WCFBattleUser { get; private set; }

        public ExtHomeCardType ExtHomeCardType { get; private set; }

        public ExtStep Step { get; private set; }

        public string Strength { get; private set; }
        public WCFRandomDesk RandomCard { get; private set; }

        public ExtBattleUser(ExtStep step, WCFBattleUser wcfBattleUser)
        {
            WCFBattleUser = wcfBattleUser;
            Step = step;

            ExtHomeCardType = string.IsNullOrEmpty(WCFBattleUser.HomeCardType)
                ? null
                : MainWindow.ClientInfo.WorldData.HomeCardType.Single(p => p.WCFHomeCardType.Name == WCFBattleUser.HomeCardType);

            Strength = WCFBattleUser.Strength.HasValue ? WCFBattleUser.Strength.Value.ToString() : "?";
            RandomCard = WCFBattleUser.RandomDeskId.HasValue ? ExtRandomDesk.GetRandomDesk(WCFBattleUser.RandomDeskId.Value) : null;
        }
    }
}
