using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRA.Mods.WWI.Traits
{
    public class PopulationInfo : UpgradableTraitInfo
    {
        [Desc("If negative, it will drain population. If positive, it will provide population.")]
        public readonly int Amount = 0;

        public override object Create(ActorInitializer init) { return new Population(init.Self, this); }
    }

    public class Population : UpgradableTrait<PopulationInfo>, INotifyAddedToWorld, INotifyRemovedFromWorld, INotifyOwnerChanged
    {
        public PopulationManager PlayerPopulation { get; private set; }

        public int GetPopulationModifier()
        {
            return Info.Amount;
        }

        public Population(Actor self, PopulationInfo info) : base(info)
        {
            PlayerPopulation = self.Owner.PlayerActor.Trait<PopulationManager>();
        }

        protected override void UpgradeEnabled(Actor self) { PlayerPopulation.UpdateActor(self); }
        protected override void UpgradeDisabled(Actor self) { PlayerPopulation.UpdateActor(self); }
        public void AddedToWorld(Actor self) { PlayerPopulation.UpdateActor(self); }
        public void RemovedFromWorld(Actor self) { PlayerPopulation.RemoveActor(self); }
        public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            PlayerPopulation.RemoveActor(self);
            PlayerPopulation = newOwner.PlayerActor.Trait<PopulationManager>();
            PlayerPopulation.UpdateActor(self);
        }
    }
}
