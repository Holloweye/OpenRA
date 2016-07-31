using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Traits
{
    public class PaysTaxesInfo : ITraitInfo
    {
        [Desc("How much taxes to pay.")]
        public readonly int Amount = 0;

        public object Create(ActorInitializer init) { return new PaysTaxes(this); }
    }

    class PaysTaxes
    {
        public readonly PaysTaxesInfo info;

        public PaysTaxes(PaysTaxesInfo info)
		{
			this.info = info;
		}
    }
}
