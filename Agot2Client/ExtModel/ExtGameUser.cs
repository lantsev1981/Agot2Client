using GameService;
using MyMod;
using System.Collections.Generic;
using System.Linq;

namespace Agot2Client
{
    public class ExtGameUser : MyNotifyObj
    {
        public ExtGame Game { get; private set; }
        public WCFGameUser WCFGameUser { get; private set; }

        public ExtHomeType ExtHomeType { get; private set; }

        public GPUser GPUser { get; private set; }

        public ExtGameUser(ExtGame game, WCFGameUser wcfGameUser)
        {
            Game = game;
            WCFGameUser = wcfGameUser;

            if (wcfGameUser.Login == "Вестерос")
            {
                GPUser = MainWindow.GamePortal.Vesteros;
            }
            else if (!string.IsNullOrEmpty(wcfGameUser.Login))
            {
                GPUser = MainWindow.GamePortal.GetUser(wcfGameUser.Login);
            }

            if (!string.IsNullOrEmpty(WCFGameUser.HomeType))
            {
                ExtHomeType = MainWindow.ClientInfo.WorldData.HomeType.Single(p => p.WCFHomeType.Name == WCFGameUser.HomeType);
            }

            Sync(wcfGameUser);
        }

        public void Sync(WCFGameUser serverUser)
        {
            if (Game != null)
            {
                if (Game.ClientGameUser == this && serverUser.NeedReConnect)
                {
                    MainWindow.ClientInfo.ClientGameId = Game.WCFGame.Id;
                }
            }

            if (WCFGameUser.Login != serverUser.Login)
            {
                WCFGameUser.Login = serverUser.Login;
                GPUser = MainWindow.GamePortal.GetUser(WCFGameUser.Login);
                base.OnPropertyChanged("GPUser");
            }
            WCFGameUser.NewChat = serverUser.NewChat;
            WCFGameUser.NewStep = serverUser.NewStep;
        }

        public ExtStep LastStep
        {
            get
            {
                KeyValuePair<int, ExtStep> result = Game.GetUserStep(this);
                return result.Value;
            }
        }

        public ExtStep ViewStep => Game.GetUserViewStep(this);
    }
}
