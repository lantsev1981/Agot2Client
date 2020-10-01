using GameService;
using MyMod;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Agot2Client
{
    public class ExtHomeType : MyNotifyObj
    {
        #region Key на всех языках один - это путь к файлам
        public static readonly Dictionary<string, string> Keys;
        static ExtHomeType()
        {
            Keys = new Dictionary<string, string>
            {
                { "Баратеон", "baratheon" },
                { "Ланнистер", "lannister" },
                { "Старк", "stark" },
                { "Мартелл", "martell" },
                { "Грейджой", "greyjoy" },
                { "Тирелл", "tyrell" },

                { "dragon_Баратеон", "baratheon" },
                { "dragon_Ланнистер", "lannister" },
                { "dragon_Болтон", "bolton" },
                { "dragon_Тирелл", "tyrell" },
                { "dragon_Мартелл", "martell" },
                { "dragon_Грейджой", "greyjoy" }
            };
        }

        private string Key => Keys[WCFHomeType.Name];
        #endregion

        public ExtGame Game { get; private set; }
        public WCFHomeType WCFHomeType { get; private set; }

        public ExtTerrain ExtTerrain { get; private set; }

        public SolidColorBrush HomeColor { get; private set; }
        public string VictoryImageName { get; private set; }
        public string SupplyImageName { get; private set; }
        public string InfluenceImageName { get; private set; }
        public string HomeCardBackName { get; private set; }
        public string ImageName { get; private set; }
        public string UnitImageFormate { get; private set; }
        public string Name { get; private set; }
        public WCFGamePoint Position { get; private set; }
        public int Sort { get; private set; }

        public ExtHomeType(ExtStaticData extStaticData, WCFHomeType wcfHomeType, List<ExtHomeCardType> homeCard)
        {
            WCFHomeType = wcfHomeType;

            ExtTerrain = extStaticData.Terrain.Single(p => p.WCFTerrain.Name == WCFHomeType.Terrain);
            ExtTerrain.ExtHomeType = this;//TODO даёт ссылку на себя для родовой земли (Обратная связь)

            Position = this.ExtTerrain.ExtTokenPoint.Single(p => p.WCFTokenPoint.TokenType == "Жетон_власти").WCFGamePoint;

            switch (Key)
            {
                case "baratheon": Sort = 0; HomeColor = new SolidColorBrush(Colors.Yellow) { Opacity = 0.25 }; break;
                case "lannister": Sort = 1; HomeColor = new SolidColorBrush(Colors.Red) { Opacity = 0.25 }; break;
                case "stark": Sort = 2; HomeColor = new SolidColorBrush(Colors.White) { Opacity = 0.25 }; break;
                case "martell": Sort = 3; HomeColor = new SolidColorBrush(Colors.Orange) { Opacity = 0.25 }; break;
                case "greyjoy": Sort = 4; HomeColor = new SolidColorBrush(Colors.Black) { Opacity = 0.25 }; break;
                case "tyrell": Sort = 5; HomeColor = new SolidColorBrush(Colors.Green) { Opacity = 0.25 }; break;
                case "bolton": Sort = 2; HomeColor = new SolidColorBrush(Colors.Purple) { Opacity = 0.25 }; break;
            }

            Name = App.GetResources("homeType_" + WCFHomeType.Name);
            ImageName = string.Format("/Image/{0}/{0}.png", Key);
            SupplyImageName = ImageName;
            VictoryImageName = string.Format("/Image/{0}/{0}_victory.png", Key);
            InfluenceImageName = string.Format("/Image/{0}/{0}_influence.png", Key);
            HomeCardBackName = string.Format("/Image/{0}/{0}_back.png", Key);
            UnitImageFormate = string.Format("/Image/{0}/{0}_{1}", Key, "{0}.png");
            HomeCard = homeCard;

            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.ClientStepCganged -= ExtGame_ClientStepCgange;
                Game.CurrentViewKeyCganged -= ExtGame_CurrentViewKeyCgange;
            }
            if (game != null)
            {
                Game = game;
                Game.ClientStepCganged += ExtGame_ClientStepCgange;
                Game.CurrentViewKeyCganged += ExtGame_CurrentViewKeyCgange;
            }

            this.SupportVisibility = Visibility.Collapsed;
        }

        private void ExtGame_CurrentViewKeyCgange()
        {
            this.OnPropertyChanged("HomeCard");
            this.OnPropertyChanged("TokenList");
        }

        private void ExtGame_ClientStepCgange(ExtStep newValue)
        {
            if (newValue == null)
                this.SupportVisibility = Visibility.Collapsed;
        }

        public ExtGameUser HomeGameUser
        {
            get
            {
                if (Game == null)
                    return null;

                return Game.ExtGameUser.Single(p => p.WCFGameUser.HomeType == WCFHomeType.Name);
            }
        }

        private Visibility _SupportVisibility = Visibility.Collapsed;
        public Visibility SupportVisibility
        {
            get => _SupportVisibility;
            set
            {
                _SupportVisibility = value;
                OnPropertyChanged("SupportVisibility");
            }
        }

        public IEnumerable<Token> TokenList
        {
            get
            {
                if (HomeGameUser == null || HomeGameUser.LastStep == null)
                    return null;

                List<Token> result = new List<Token>();

                //земли
                Token landToken = new Token
                {
                    ImageName = "/Image/land.jpg",
                    BankCount = MainWindow.ClientInfo.WorldData.LandCount,
                    UserCount = HomeGameUser.LastStep.ExtGameUserInfo.TerrainCol.Count(p => p.WCFTerrain.TerrainType == "Земля")
                };
                result.Add(landToken);

                //жетоны власти
                Token token = new Token
                {
                    ImageName = ImageName,
                    BankCount = 20,
                    UserCount = HomeGameUser.LastStep.ExtGameUserInfo.WCFGameUserInfo.Power
                };
                result.Add(token);

                //войска
                foreach (ExtUnitType item in MainWindow.ClientInfo.WorldData.UnitType)
                {
                    token = new Token
                    {
                        ImageName = string.Format("/Image/{0}/{0}_{1}.png", Key, item.Key),
                        BankCount = item.WCFUnitType.Count,
                        UserCount = HomeGameUser.LastStep.ExtGameUserInfo.ExtUnit.Count(p => p.WCFUnit.UnitType == item.WCFUnitType.Name)
                    };
                    result.Add(token);
                }

                return result;
            }
        }

        public List<ExtHomeCardType> HomeCard { get; private set; }
    }

    public class Token
    {
        public string ImageName { get; set; }
        public int UserCount { get; set; }
        public int BankCount { get; set; }
    }
}
