using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.WWI.Traits;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace OpenRA.Mods.WWI.Activities
{
    public class FindGoods : Activity
    {
        readonly Trader trader;
		readonly TraderInfo traderInfo;
		readonly IMove move;
		readonly IPathFinder pathFinder;

		public FindGoods(Actor self)
		{
            trader = self.Trait<Trader>();
            traderInfo = self.Info.Traits.Get<TraderInfo>();
            move = self.Trait<IMove>();
			pathFinder = self.World.WorldActor.Trait<IPathFinder>();
		}

        public override Activity Tick(Actor self)
        {
            if (IsCanceled || NextActivity != null)
                return NextActivity;

            if (trader.tradeBuilding == null)
            {
                trader.tradeBuilding = trader.ClosestTradeBuilding(self);
            }

            if (trader.tradeBuilding == null)
            {
                return Util.SequenceActivities(new Wait(traderInfo.SearchForTradeBuildingDelay), this);
            }

            self.SetTargetLine(Target.FromActor(trader.tradeBuilding), Color.Green, false);

            if ((self.CenterPosition - trader.tradeBuilding.CenterPosition).LengthSquared > WDist.FromCells(4).LengthSquared)
            {
                return Util.SequenceActivities(move.MoveTo(trader.tradeBuilding.Location, 4), this);
            }

            trader.isLoaded = true;

            return new DeliverGoods(self);
        }
    }
}
