using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Pathfinder;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.WWI.Activities;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace OpenRA.Mods.WWI.Traits
{
    public class TraderInfo : ITraitInfo, Requires<MobileInfo>
    {
        public readonly string[] DeliveryBuildings = { };
        public readonly string[] TradeBuildings = { };

        [Desc("How long (in ticks) to wait until (re-)checking for a nearby available TradeActors if not yet linked to one.")]
        public readonly int SearchForTradeBuildingDelay = 125;

        [Desc("How long (in ticks) to wait until (re-)checking for a nearby available DeliveryBuilding if not yet linked to one.")]
        public readonly int SearchForDeliveryBuildingDelay = 125;

        [Desc("Automatically scan for trade building when created.")]
        public readonly bool SearchOnCreation = true;

        [Desc("Value for succeessful trade.")]
        public readonly float DistanceMultiplier = 0;

        public object Create(ActorInitializer init) { return new Trader(init.Self, this); }
    }
    public class Trader : IIssueOrder, IResolveOrder, ISync, INotifyCreated, INotifyIdle, INotifyBlockingMove
    {
        readonly TraderInfo info;
        readonly Mobile mobile;

        public bool isLoaded = false;
        bool idleSmart = true;

        [Sync] public Actor deliveryBuilding = null;
        [Sync] public Actor tradeBuilding = null;

        public Trader(Actor self, TraderInfo info)
		{
			this.info = info;
			mobile = self.Trait<Mobile>();
		}

        public void Created(Actor self)
        {
            if (info.SearchOnCreation)
                self.QueueActivity(new FindGoods(self));
        }

        public void OnNotifyBlockingMove(Actor self, Actor blocking)
        {
            // I'm blocking someone else from moving to my location:
            var act = self.GetCurrentActivity();

            // If I'm just waiting around then get out of the way:
            if (act is Wait)
            {
                self.CancelActivity();

                var cell = self.Location;
                var moveTo = mobile.NearestMoveableCell(cell, 2, 5);
                self.QueueActivity(mobile.MoveTo(moveTo, 0));
                self.SetTargetLine(Target.FromCell(self.World, moveTo), Color.Gray, false);

                if (isLoaded)
                {
                    self.QueueActivity(new DeliverGoods(self));
                }
                else
                {
                    self.QueueActivity(new FindGoods(self));
                }
            }
        }

        public void TickIdle(Actor self)
        {
            // Should we be intelligent while idle?
            if (!idleSmart) return;

            if (isLoaded)
            {
                self.QueueActivity(new DeliverGoods(self));
            }
            else 
            {
                self.QueueActivity(new FindGoods(self));
            }
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                yield return new EnterAlliedActorTargeter<Building>("Trade", 5,
                    a => info.TradeBuildings.Contains(a.Info.Name),
                    a => !isLoaded);
                yield return new EnterAlliedActorTargeter<Building>("DeliverTrade", 5,
                    a => info.DeliveryBuildings.Contains(a.Info.Name),
                    a => isLoaded);
            }
        }

        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID == "Trade")
                return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };

            if (order.OrderID == "DeliverTrade")
                return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };

            return null;
        }

        public void ResolveOrder(Actor self, Order order)
        {
            if (order.OrderString == "Trade")
            {
                var building = order.TargetActor.TraitOrDefault<Building>();
                if (building == null || !info.TradeBuildings.Contains(order.TargetActor.Info.Name))
                    return;

                if (isLoaded)
                    return;

                if (order.TargetActor != tradeBuilding)
                    tradeBuilding = order.TargetActor;

                idleSmart = true;

                self.SetTargetLine(Target.FromOrder(self.World, order), Color.Green);

                self.CancelActivity();

                var next = new FindGoods(self);
                self.QueueActivity(next);
            }
            else if (order.OrderString == "DeliverTrade")
            {
                var building = order.TargetActor.TraitOrDefault<Building>();
                if (building == null || !info.DeliveryBuildings.Contains(order.TargetActor.Info.Name))
                    return;

                if (!isLoaded)
                    return;

                if (order.TargetActor != deliveryBuilding)
                    deliveryBuilding = order.TargetActor;

                idleSmart = true;

                self.SetTargetLine(Target.FromOrder(self.World, order), Color.Green);

                self.CancelActivity();

                var next = new DeliverGoods(self);
                self.QueueActivity(next);
            }
            else if (order.OrderString == "Stop" || order.OrderString == "Move")
            {
                // Turn off idle smarts to obey the stop/move:
                idleSmart = false;
            }
        }

        public Actor ClosestTradeBuilding(Actor self)
        {
            return ClosestActorOfTypes(self, info.TradeBuildings);
        }

        public Actor ClosestDeliveryBuilding(Actor self)
        {
            return ClosestActorOfTypes(self, info.DeliveryBuildings);
        }

        private Actor ClosestActorOfTypes(Actor self, string[] types)
        {
            // Find all buildings
            var buildings = (
                from a in self.World.ActorsWithTrait<Building>()
                where types.Contains(a.Actor.Info.Name)
                select new { Location = a.Actor.Location, Actor = a.Actor}).ToDictionary(a => a.Location);

            // Start a search from each refinery's delivery location:
            var mi = self.Info.Traits.Get<MobileInfo>();
            var path = self.World.WorldActor.Trait<IPathFinder>().FindPath(
                PathSearch.FromPoints(self.World, mi, self, buildings.Values.Select(r => r.Location), self.Location, false)
            );

            if (path.Count != 0)
                return buildings[path.Last()].Actor;

            return null;
        }
    }
}
