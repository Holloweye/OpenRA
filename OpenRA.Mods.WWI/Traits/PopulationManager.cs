using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRA.Mods.WWI.Traits
{
    public class PopulationManagerInfo : ITraitInfo, ITechTreePrerequisiteInfo, Requires<DeveloperModeInfo>
    {
        public object Create(ActorInitializer init) { return new PopulationManager(init.Self, this); }
    }

    public class PopulationManager : ITick, ISync, ITechTreePrerequisite
    {
        readonly Actor self;
        readonly PopulationManagerInfo info;

        readonly Dictionary<Actor, int> populationDrain = new Dictionary<Actor, int>();
        [Sync]
        int totalProvided;
        public int PopulationProvided { get { return totalProvided; } }

        [Sync]
        int totalDrained;
        public int PopulationDrained { get { return totalDrained; } }

        public int ExcessPopulation { get { return totalProvided - totalDrained; } }

        public PopulationManager(Actor self, PopulationManagerInfo info)
        {
            this.self = self;
            this.info = info;
        }

        public IEnumerable<string> ProvidesPrerequisites
        {
            get
            {
                List<string> prerequisites = new List<string>();
                for (int i = 1; i < this.ExcessPopulation + 1; i++)
                {
                    prerequisites.Add("Pop" + i.ToString());
                }

                return prerequisites;
            }
        }

        public void UpdateActor(Actor a)
        {
            int old;
            populationDrain.TryGetValue(a, out old); // old is 0 if a is not in powerDrain
            var amount = a.TraitsImplementing<Population>().Where(t => !t.IsTraitDisabled).Aggregate(0, (v, p) => v + p.GetPopulationModifier());
            populationDrain[a] = amount;
            if (amount == old)
                return;
            if (old > 0)
                totalProvided -= old;
            else if (old < 0)
                totalDrained += old;
            if (amount > 0)
                totalProvided += amount;
            else if (amount < 0)
                totalDrained -= amount;

            a.Owner.PlayerActor.Trait<TechTree>().ActorChanged(self);
        }

        public void RemoveActor(Actor a)
        {
            int amount;
            if (!populationDrain.TryGetValue(a, out amount))
                return;
            populationDrain.Remove(a);

            if (amount > 0)
                totalProvided -= amount;
            else if (amount < 0)
                totalDrained += amount;

            a.Owner.PlayerActor.Trait<TechTree>().ActorChanged(self);
        }

        public void Tick(Actor self)
        {
            totalProvided = 0;
            totalDrained = 0;

            foreach (var kv in populationDrain)
                if (kv.Value > 0)
                    totalProvided += kv.Value;
                else if (kv.Value < 0)
                    totalDrained -= kv.Value;
        }

        public enum PopulationState { Full, Normal }

        public PopulationState PopState
        {
            get
            {
                if (PopulationProvided >= PopulationDrained) return PopulationState.Full;
                return PopulationState.Normal;
            }
        }
    }
}
