using GameService;
using MyMod;
using System.Collections.Generic;
using System.Linq;

namespace Agot2Client
{
    public class ExtStaticData : MyNotifyObj
    {
        public WCFStaticData WCFStaticData { get; private set; }

        public IEnumerable<ExtHomeCardType> HomeCardType { get; private set; }
        public IEnumerable<ExtHomeType> HomeType { get; private set; }
        public IEnumerable<ExtObjectPoint> ObjectPoint { get; private set; }
        public IEnumerable<ExtOrderType> OrderType { get; private set; }
        public IEnumerable<ExtSymbolic> Symbolic { get; private set; }
        public IEnumerable<ExtTerrain> Terrain { get; private set; }
        public IEnumerable<ExtTrackPoint> TrackPoint { get; private set; }
        public IEnumerable<ExtTokenPoint> TokenPoint { get; private set; }
        public IEnumerable<ExtUnitType> UnitType { get; private set; }
        public int LandCount { get; private set; }


        public ExtStaticData(WCFStaticData staticData)
        {
            WCFStaticData = staticData;

            staticData.RandomDesk.ForEach(p => p.fileName = string.Format("/Image/random_card/{0}.png", p.fileName));
            ObjectPoint = staticData.ObjectPoint.Select(p => new ExtObjectPoint(staticData, p)).ToList();
            TrackPoint = staticData.TrackPoint.Select(p => new ExtTrackPoint(staticData, p)).ToList();
            TokenPoint = staticData.TokenPoint.Select(p => new ExtTokenPoint(staticData, p)).ToList();
            Symbolic = staticData.Symbolic.Select(p => new ExtSymbolic(this, p)).ToList();
            Terrain = staticData.Terrain.Select(p => new ExtTerrain(this, p)).ToList();
            OrderType = staticData.OrderType.Select(p => new ExtOrderType(p)).ToList();
            UnitType = staticData.UnitType.Select(p => new ExtUnitType(p)).ToList();
            HomeCardType = staticData.HomeCardType.Select(p => new ExtHomeCardType(p)).ToList();
            HomeType = staticData.HomeType.Select(p => new ExtHomeType(this, p, HomeCard(p))).ToList();


            foreach (ExtTerrain terrain in Terrain)
            {
                IEnumerable<WCFTerrainTerrain> TerrainTerrain = WCFStaticData.TerrainTerrain.Where(p => p.Terrain == terrain.WCFTerrain.Name);

                foreach (WCFTerrainTerrain item in TerrainTerrain.ToList())
                    terrain.JoinTerrainCol.Add(Terrain.Single(p => p.WCFTerrain.Name == item.JoinTerrain));
            }

            LandCount = Terrain.Count(p => p.WCFTerrain.TerrainType == "Земля");
        }

        private List<ExtHomeCardType> HomeCard(WCFHomeType item)
        {
            return HomeCardType
                .Where(p => p.WCFHomeCardType.HomeType == item.Name)
                .OrderByDescending(p => p.WCFHomeCardType.Strength).ToList();
        }
    }
}
