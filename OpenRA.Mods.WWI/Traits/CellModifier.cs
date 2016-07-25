using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRA.Mods.WWI.Traits
{
    [Desc("Modifies actors that occupy same cell.")]
    public class CellModifierInfo : ITraitInfo
    {
        public readonly bool AffectsFriendly = true;
        public readonly bool AffectsEnemy = true;

        public readonly int DamageModifier = 100;
        public readonly int FirepowerModifier = 100;
        public readonly int ReloadModifier = 100;
        public readonly int InaccuracyModifer = 100;
        public readonly int SpeedModifier = 100;

        public object Create(ActorInitializer init) { return new CellModifier(init.Self, this); }
    }

    public class CellModifier : INotifyAddedToWorld, INotifyRemovedFromWorld
    {
        readonly Actor self;
        readonly CellModifierInfo info;

        private int triggerId = -1;

        public CellModifier(Actor self, CellModifierInfo info)
		{
			this.self = self;
			this.info = info;
		}

        public void AddedToWorld(Actor self)
        {
            CPos pos = self.OccupiesSpace.TopLeft;
            triggerId = self.World.ActorMap.AddCellTrigger(new CPos[1]{pos}, addBonusTo, removeBonusFrom);
        }

        public void RemovedFromWorld(Actor self)
        {
            self.World.ActorsWithTrait<CellModification>().Where(a => a.Actor.Location == self.Location).Do(a => removeBonusFrom(a.Actor));
            self.World.ActorMap.RemoveCellTrigger(triggerId);
        }

        private void addBonusTo(Actor a)
        {
            CellModification trait = a.TraitOrDefault<CellModification>();
            if (trait == null && a != null && affects(a))
            {
                trait = new CellModification(a);
                trait.damageModifier = info.DamageModifier;
                trait.firepowerModifier = info.FirepowerModifier;
                trait.reloadModifier = info.ReloadModifier;
                trait.inaccuracyModifier = info.InaccuracyModifer;
                trait.speedModifier = info.SpeedModifier;
                a.AddTrait(trait);
            }
        }

        private void removeBonusFrom(Actor a)
        {
            if (a.IsDead || !a.IsInWorld) return;

            CellModification trait = a.TraitOrDefault<CellModification>();
            if (trait != null && a != null && affects(a))
            {
                trait.damageModifier = 100;
                trait.firepowerModifier = 100;
                trait.reloadModifier = 100;
                trait.inaccuracyModifier = 100;
                trait.speedModifier = 100;
            }
        }

        private bool affects(Actor a)
        {
            return (self.Owner.IsAlliedWith(a.Owner) == info.AffectsFriendly || !self.Owner.IsAlliedWith(a.Owner) == info.AffectsEnemy);
        }
    }

    public class CellModification : IDamageModifier, IFirepowerModifier, IReloadModifier, IInaccuracyModifier, ISpeedModifier
    {
        readonly Actor self;

        public int damageModifier;
        public int firepowerModifier;
        public int reloadModifier;
        public int inaccuracyModifier;
        public int speedModifier;

        public CellModification(Actor self)
        {
            this.self = self;
        }

        public int GetDamageModifier(Actor attacker, Damage damage)
        {
            return damageModifier;
        }

        public int GetFirepowerModifier()
        {
            return firepowerModifier;
        }

        public int GetReloadModifier()
        {
            return reloadModifier;
        }

        public int GetInaccuracyModifier()
        {
            return inaccuracyModifier;
        }

        public int GetSpeedModifier()
        {
            return speedModifier;
        }
    }
}
