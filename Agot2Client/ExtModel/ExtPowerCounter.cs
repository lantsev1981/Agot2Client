using System.Linq;
using MyMod;
using GameService;

namespace Agot2Client
{
    public class ExtPowerCounter : MyNotifyObj, IPosition
    {
        public WCFPowerCounter WCFPowerCounter { get; private set; }
        public ExtStep Step { get; private set; }

        public ExtTerrain ExtTerrain { get; private set; }
        public string ImageName { get; private set; }
        public WCFGamePoint Position { get; set; }
        public bool IsTemp { get; set; }

        public ExtPowerCounter(ExtStep step, WCFPowerCounter wcfPowerCounter)
        {
            Step = step;
            WCFPowerCounter = wcfPowerCounter;

            ExtTerrain = MainWindow.ClientInfo.WorldData.Terrain.Single(p => p.WCFTerrain.Name == WCFPowerCounter.Terrain);
            ImageName = Step.ExtGameUser.ExtHomeType.ImageName;
            Position = ExtTerrain.ExtTokenPoint
                    .Single(p => p.WCFTokenPoint.TokenType == "Жетон_власти")
                    .WCFGamePoint;
        }


        bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                OnPropertyChanged("Opacity");
            }
        }

        public double Opacity
        {
            get
            {
                if (IsTemp)
                    return IsSelected
                        ? 1
                        : .66;

                return 1;
            }
        }
    }
}
