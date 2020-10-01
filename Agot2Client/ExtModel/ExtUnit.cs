using System.Linq;
using MyMod;
using GameService;

namespace Agot2Client
{
    public partial class ExtUnit : MyNotifyObj, IPosition
    {
        public WCFUnit WCFUnit { get; private set; }
        public ExtStep Step { get; private set; }

        public ExtTerrain ExtTerrain { get; private set; }
        public ExtUnitType ExtUnitType { get; private set; }

        public ExtUnit(ExtStep step, WCFUnit wcfUnit)
        {
            Step = step;
            WCFUnit = wcfUnit;

            ExtTerrain = MainWindow.ClientInfo.WorldData.Terrain.Single(p => p.WCFTerrain.Name == WCFUnit.Terrain);
            ExtUnitType = MainWindow.ClientInfo.WorldData.UnitType.Single(p => p.WCFUnitType.Name == WCFUnit.UnitType);
        }

        //Временная территория
        ExtTerrain _TempTerrain;
        public ExtTerrain TempTerrain
        {
            get
            {
                return _TempTerrain != null ? _TempTerrain : ExtTerrain;
            }
            set
            {
                _TempTerrain = value == ExtTerrain ? null : value;

                //Пересмотреть все юниты на временной территории
                foreach (var item in TempTerrain.TempUnit)
                    item.OnPropertyChanged("Position");
            }

        }

        ExtUnitType _TempUnitType;
        public ExtUnitType TempUnitType
        {
            get
            {
                return _TempUnitType != null ? _TempUnitType : ExtUnitType;
            }
            set
            {
                _TempUnitType = value == ExtUnitType ? null : value;
                OnPropertyChanged("ImageName");
            }
        }

        public WCFGamePoint Position
        {
            get
            {
                int index = TempTerrain.TempUnit.IndexOf(this);
                return this.TempTerrain.ExtTokenPoint.Where(p => p.WCFTokenPoint.TokenType == "Войско").ElementAt(index).WCFGamePoint;

            }
            set { }
        }

        public string ImageName
        {
            get
            {
                return this.TempUnitType == null ? null : string.Format(this.Step.ExtGameUser.ExtHomeType.UnitImageFormate, TempUnitType.Key);
            }
        }

        bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
        }

        public double Angle
        {
            get
            {
                if (WCFUnit.IsWounded) return 90;
                else return 0;
            }
        }
    }
}
