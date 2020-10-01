using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;

using MyMod;
using GameService;

namespace Agot2Client
{
    public class ExtTerrain : MyNotifyObj
    {
        public ExtGame Game { get; private set; }
        public WCFTerrain WCFTerrain { get; private set; }

        public IEnumerable<ExtObjectPoint> ObjectPoint { get; private set; }
        public PointCollection Points { get; private set; }
        public IEnumerable<ExtTokenPoint> ExtTokenPoint { get; private set; }

        //TODO типы домов ещё не загружены
        public ExtHomeType ExtHomeType { get; set; }
        public ICollection<ExtTerrain> JoinTerrainCol { get; private set; }

        public string Name { get; private set; }


        public ExtTerrain(ExtStaticData extStaticData, WCFTerrain wcfTerrain)
        {
            WCFTerrain = wcfTerrain;
            JoinTerrainCol = new List<ExtTerrain>();

            ObjectPoint = extStaticData.ObjectPoint.Where(p => p.WCFObjectPoint.Terrain == WCFTerrain.Name);
            ObjectPoint = ObjectPoint.OrderBy(p => p.WCFObjectPoint.Sort).ToList();

            Points = new PointCollection(this.ObjectPoint.Select(p => new Point(p.GamePoint.X, p.GamePoint.Y)));

            ExtTokenPoint = extStaticData.TokenPoint.Where(p => p.WCFTokenPoint.Terrain == WCFTerrain.Name).ToList();
            Name = App.GetResources("terrain_" + WCFTerrain.Name);

            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        void ClientInfo_ClientGameChanging(ExtGame game)
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

            this.ExtHolderUser = null;
        }

        void ExtGame_CurrentViewKeyCgange()
        {
            ExtGameUserInfo extGameUserInfo = Game.ViewGameUserInfo.LastOrDefault(p => p.TerrainCol.Any(p1 => p1 == this));
            this.ExtHolderUser = extGameUserInfo == null ? null : extGameUserInfo.Step.ExtGameUser;
        }

        public ExtGarrison ExtGarrison { get { return Game.ViewGameInfo.ExtGarrison.SingleOrDefault(p => p.ExtTerrain == this); } }

        public ExtOrder Order { get { return Game.ViewOrder.SingleOrDefault(p => p.ExtTerrain == this); } }

        public ExtPowerCounter PowerCounter { get { return Game.ViewPowerCounter.SingleOrDefault(p => p.ExtTerrain == this); } }

        //TODO List используется 1 раз (IEnumerable)
        public List<ExtUnit> TempUnit { get { return Game.ViewUnit.Where(p => p.TempTerrain == this).ToList(); } }

        public IEnumerable<ExtUnit> Unit { get { return Game.ViewUnit.Where(p => p.ExtTerrain == this); } }

        public Brush FillBrush { get { return this.ExtHolderUser == null ? new SolidColorBrush(Colors.Transparent) : this.ExtHolderUser.ExtHomeType.HomeColor; } }

        ExtGameUser _ExtHolderUser;
        public ExtGameUser ExtHolderUser
        {
            set
            {
                _ExtHolderUser = value;
                this.OnPropertyChanged("FillBrush");
            }
            get
            {
                return _ExtHolderUser;
            }
        }
    }
}
