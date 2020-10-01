using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

using GameService;
using MyMod;

namespace Agot2Client
{
    public class ExtHomeCardType : MyNotifyObj
    {
        public ExtGame Game { get; private set; }
        public WCFHomeCardType WCFHomeCardType { get; private set; }

        public string ViewName { get; private set; }
        public BitmapImage SourceBitmap { get; private set; }
        public BitmapImage TopCroppedBitmap { get; private set; }
        public BitmapImage BottomCroppedBitmap { get; private set; }

        public ExtHomeCardType(WCFHomeCardType wcfHomeCardType)
        {
            WCFHomeCardType = wcfHomeCardType;

            ViewName = App.GetResources("hero_" + WCFHomeCardType.Name);
            var imagePath = App.GetResources("image_" + WCFHomeCardType.Name);

            try
            {
                SourceBitmap = ImageExt.Load(imagePath, Int32Rect.Empty);
                TopCroppedBitmap = ImageExt.Load(imagePath, new Int32Rect(0, 110, 300, 215));
                BottomCroppedBitmap = ImageExt.Load(imagePath, new Int32Rect(0, 325, 300, 125));
            }
            catch
            {
                
            }
                        
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.CurrentViewKeyCganged -= ExtGame_CurrentViewKeyCgange;
            }
            if (game != null)
            {
                Game = game;
                Game.CurrentViewKeyCganged += ExtGame_CurrentViewKeyCgange;
            }
        }

        void ExtGame_CurrentViewKeyCgange()
        {
            this.OnPropertyChanged("IsUsed");
        }

        public ExtGameUser HomeGameUser
        {
            get
            {
                if (Game == null)
                    return null;

                return Game.ExtGameUser.Single(p => p.WCFGameUser.HomeType == WCFHomeCardType.HomeType);
            }
        }

        public bool IsUsed
        {
            get
            {
                if (HomeGameUser == null || HomeGameUser.LastStep == null)
                    return false;

                return HomeGameUser.LastStep.ExtGameUserInfo.WCFGameUserInfo.UsedHomeCard
                    .Any(p => p.HomeCardType == WCFHomeCardType.Name);
            }
        }
    }
}
