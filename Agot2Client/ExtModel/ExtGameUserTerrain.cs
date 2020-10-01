using System.Linq;
using GameService;

namespace Agot2Client
{
    static public class ExtGameUserTerrain
    {
        //получает территорию
        static public ExtTerrain ExtTerrain(this WCFGameUserTerrain o)
        {
            ExtTerrain result = MainWindow.ClientInfo.WorldData.Terrain
                .Single(p => p.WCFTerrain.Name == o.Terrain);

            return result;
        }
    }
}
