using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
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
    class DeliverGoods : Activity
    {
        readonly Trader trader;
		readonly TraderInfo traderInfo;
        readonly IMove move;
		readonly IPathFinder pathFinder;

		public DeliverGoods(Actor self)
		{
            trader = self.Trait<Trader>();
            traderInfo = self.Info.TraitInfo<TraderInfo>();
            move = self.Trait<IMove>();
            pathFinder = self.World.WorldActor.Trait<IPathFinder>();
		}

        public override Activity Tick(Actor self)
        {
            if (IsCanceled || NextActivity != null)
                return NextActivity;

            if (trader.deliveryBuilding == null)
            {
                trader.deliveryBuilding = trader.ClosestDeliveryBuilding(self);
            }

            if (trader.deliveryBuilding == null)
            {
                return ActivityUtils.SequenceActivities(new Wait(traderInfo.SearchForDeliveryBuildingDelay), this);
            }

            self.SetTargetLine(Target.FromActor(trader.deliveryBuilding), Color.Green, false);

            if ((self.CenterPosition - trader.deliveryBuilding.CenterPosition).LengthSquared > WDist.FromCells(4).LengthSquared)
            {
                return ActivityUtils.SequenceActivities(move.MoveTo(trader.deliveryBuilding.Location, 4), this);
            }

            if (trader.isLoaded)
            {
                trader.isLoaded = false;

                float distance = (trader.tradeBuilding.CenterPosition - trader.deliveryBuilding.CenterPosition).HorizontalLength / 100;
                int amount = (int)(distance * traderInfo.DistanceMultiplier);

                PlayerResources playerResources = self.Owner.PlayerActor.Trait<PlayerResources>();
                playerResources.GiveCash(amount);

                if (amount > 0)
                    self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, self.Owner.Color.RGB, FloatingText.FormatCashTick(amount), 30)));
            }

            return new FindGoods(self);
        }
    }
}
