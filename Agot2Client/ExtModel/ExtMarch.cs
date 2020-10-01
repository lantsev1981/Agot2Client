using System.Collections.Generic;
using System.Linq;
using GameService;

namespace Agot2Client
{
    public class ExtMarch
    {
        public ExtStep Step { get; private set; }
        public WCFMarch WCFMarch { get; private set; }
        public IEnumerable<ExtMarchUnit> ExtMarchUnit { get; private set; }
        public int SumCost { get; set; }

        public ExtMarch(ExtStep step, WCFMarch wcfMarch)
        {
            Step = step;
            WCFMarch = wcfMarch;

            //каждый раз запрашивает новый список
            ExtMarchUnit = WCFMarch.MarchUnit.Select(p => new ExtMarchUnit(Step, p));
        }

        public void Clear()
        {
            WCFMarch.MarchUnit.Clear();
            WCFMarch.SourceOrder = null;
            this.SumCost = 0;
        }
    }
}
