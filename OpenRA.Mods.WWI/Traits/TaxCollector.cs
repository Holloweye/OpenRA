using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Traits
{
    public class TaxCollectorInfo : ITraitInfo
    {
        [Desc("Number of ticks to wait between giving money.")]
        public readonly int Period = 375;

        [Desc("Amount of money to give as a base value.")]
        public readonly int BaseAmount = 0;

        [Desc("Which buildings to collect taxes from.")]
        public readonly string[] Buildings = null;

        [Desc("Maximum credits gained.")]
        public readonly int MaxCredits = Int16.MaxValue;

        [Desc("Max distance from which to collect from.")]
        public readonly int CollectionDistance = 0;

        [Desc("Whether to show the cash tick indicators (+$15 rising from actor).")]
        public readonly bool ShowTicks = true;

        public object Create(ActorInitializer init) { return new TaxCollector(this); }
    }

    class TaxCollector : ITick
    {
        readonly TaxCollectorInfo info;
        [Sync] int ticks;

        public TaxCollector(TaxCollectorInfo info)
		{
			this.info = info;
		}

        public void Tick(Actor self)
        {
            if (--ticks < 0)
            {
                ticks = info.Period;

                int amount = 0;

                amount += (
                    from a in self.World.ActorsWithTrait<PaysTaxes>()
                    where a.Actor.Owner == self.Owner && info.Buildings.Contains(a.Actor.Info.Name) && (info.CollectionDistance == 0 || info.CollectionDistance > (a.Actor.CenterPosition - self.CenterPosition).HorizontalLength )
                    select a.Trait.info.Amount).Sum();

                amount += info.BaseAmount;
                amount = Math.Min(amount, info.MaxCredits);

                self.Owner.PlayerActor.Trait<PlayerResources>().GiveCash(amount);

                if (info.ShowTicks && amount > 0)
                    self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, self.Owner.Color.RGB, FloatingText.FormatCashTick(amount), 30)));
            }
        }
    }
}
