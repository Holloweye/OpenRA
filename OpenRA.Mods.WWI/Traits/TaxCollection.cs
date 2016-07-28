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
    public class TaxCollectionInfo : ITraitInfo
    {
        [Desc("Number of ticks to wait between giving money.")]
        public readonly int Period = 375;
        [Desc("Amount of money to give as a base value.")]
        public readonly int BaseAmount = 0;

        [Desc("Which buildings to collect taxes from.")]
        public readonly string[] Buildings = null;
        [Desc("How much taxes each buildings pay.")]
        public readonly int TaxAmount = 0;

        [Desc("Maximum credits gained.")]
        public readonly int MaxCredits = Int16.MaxValue;

        [Desc("Whether to show the cash tick indicators (+$15 rising from actor).")]
        public readonly bool ShowTicks = true;

        public object Create(ActorInitializer init) { return new TaxCollection(this); }
    }

    class TaxCollection : ITick
    {
        readonly TaxCollectionInfo info;
        [Sync] int ticks;

        public TaxCollection(TaxCollectionInfo info)
		{
			this.info = info;
		}

        public void Tick(Actor self)
        {
            if (--ticks < 0)
            {
                var buildings = (
                    from a in self.World.ActorsWithTrait<Building>()
                    where a.Actor.Owner == self.Owner && info.Buildings.Contains(a.Actor.Info.Name)
                    select a);

                ticks = info.Period;

                int amount = Math.Min(info.BaseAmount + info.TaxAmount * buildings.Count(), info.MaxCredits);

                self.Owner.PlayerActor.Trait<PlayerResources>().GiveCash(amount);

                if (info.ShowTicks && amount > 0)
                    self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, self.Owner.Color.RGB, FloatingText.FormatCashTick(amount), 30)));
            }
        }
    }
}
