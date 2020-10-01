using System.Linq;
using GameService;

namespace Agot2Client
{
    public class ExtMarchUnit
    {
        public ExtStep Step { get; private set; }
        public WCFMarchUnit WCFMarchUnit { get; private set; }

        public ExtUnit ExtUnit { get; private set; }

        public ExtMarchUnit(ExtStep step, WCFMarchUnit wcfMarchUnit)
        {
            Step = step;
            WCFMarchUnit = wcfMarchUnit;

            ExtUnit = Step.Game.ViewUnit.Single(p => p.WCFUnit.Id == WCFMarchUnit.Unit);
        }
    }
}
