using System.Linq;
using GameService;


namespace Agot2Client
{
    public class ExtBattle
    {
        public ExtStep Step { get; private set; }
        public WCFBattle WCFBattle { get; private set; }

        public ExtGameUser AttackUser { get; private set; }
        public ExtGameUser DefenceUser { get; private set; }
        public ExtTerrain AttackTerrain { get; private set; }
        public ExtTerrain DefenceTerrain { get; private set; }

        //TODO возможно стоит перенести в gameuser
        public ExtGameUser ClientOpponent
        {
            get
            {
                if (Step.Game.ClientGameUser == AttackUser)
                    return DefenceUser;
                if (Step.Game.ClientGameUser == DefenceUser)
                    return AttackUser;

                return null;
            }
        }

        public ExtBattleUser BattleUser { get; set; }

        public ExtBattle(ExtStep step, WCFBattle wcfBattle)
        {
            Step = step;
            WCFBattle = wcfBattle;

            AttackUser = Step.Game.ExtGameUser.Single(p => p.WCFGameUser.Id == WCFBattle.AttackUser);
            DefenceUser = Step.Game.ExtGameUser.Single(p => p.WCFGameUser.Id == WCFBattle.DefenceUser);
            AttackTerrain = MainWindow.ClientInfo.WorldData.Terrain.Single(p => p.WCFTerrain.Name == wcfBattle.AttackTerrain);
            DefenceTerrain = MainWindow.ClientInfo.WorldData.Terrain.Single(p => p.WCFTerrain.Name == wcfBattle.DefenceTerrain);
        }
    }
}
