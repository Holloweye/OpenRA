﻿using OpenRA.Traits;
using OpenRA.Mods.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;

namespace OpenRA.Mods.WWI.Traits
{
    [Desc("This actor is passable. Uses the same system as crushable except that units walking of this actor do not crush it.")]
    public class PassableInfo : ITraitInfo
    {
        [Desc("Which classes does this actor belong to.")]
        public readonly HashSet<string> PassableClasses = new HashSet<string> { };
        [Desc("Is passable by friendly units.")]
        public readonly bool PassableByFriendly = true;
        [Desc("Is passable by enemy units.")]
        public readonly bool PassableByEnemy = true;
        [Desc("Weapon to use to inflict damage with when passing.")]
        public string PassingWeapon = null;
        [Desc("Uses damage if no weapon has been chosen.")]
        public readonly int PassingDamage = 0;

        [Desc("Which classes does this actor belong to.")]
        public readonly HashSet<string> CrushesClasses = new HashSet<string> { };
        [Desc("Is crushable by friendly units.")]
        public readonly bool CrushableByFriendly = false;
        [Desc("Is crushable by enemy units.")]
        public readonly bool CrushableByEnemy = true;
        [Desc("Probability of mobile actors noticing and evading a crush attempt.")]
        public readonly int CrushWarnProbability = 75;
        [Desc("Sound to play when being crushed.")]
        public readonly string CrushSound = null;
        [Desc("Weapon to use to inflict damage with when crushing.")]
        public string CrushingWeapon = null;
        [Desc("Uses damage if no weapon has been chosen.")]
        public readonly int CrushingDamage = 0;

        public object Create(ActorInitializer init) { return new Passable(init.Self, this); }
    }

    public class Passable : ICrushable, INotifyCrushed
    {
        readonly Actor self;
		readonly PassableInfo info;

        public Passable(Actor self, PassableInfo info)
		{
			this.self = self;
			this.info = info;
		}

        public void WarnCrush(Actor self, Actor crusher, HashSet<string> classes)
        {
            var mobile = self.TraitOrDefault<Mobile>();
            if (mobile != null && isCrushable(mobile.Info.Crushes) && self.World.SharedRandom.Next(100) <= info.CrushWarnProbability)
                mobile.Nudge(self, crusher, true);
        }

        public void OnCrush(Actor self, Actor crusher, HashSet<string> classes)
        {
            Mobile mobile = crusher.TraitOrDefault<Mobile>();
            if (mobile != null)
            {
                bool crushed = isCrushable(mobile.Info.Crushes);

                if (crushed)
                {
                    attack(crusher, info.CrushingWeapon, info.CrushingDamage);

                    Game.Sound.Play(info.CrushSound, crusher.CenterPosition);

                    self.Kill(crusher);
                }
                else
                {
                    attack(crusher, info.PassingWeapon, info.PassingDamage);
                }
            }
        }

        public bool CrushableBy(Actor self, Actor crusher, HashSet<string> classes)
        {
            if (!self.IsAtGroundLevel())
            {
                return false;
            }

            bool crushable = isCrushable(classes);
            bool passable = isPassable(classes);

            if (crusher.Owner.IsAlliedWith(self.Owner))
            {
                if (crushable && !info.CrushableByFriendly) return false;
                if (passable && !info.PassableByFriendly) return false;
            }
            else
            {
                if (crushable && !info.CrushableByEnemy) return false;
                if (passable && !info.PassableByEnemy) return false;
            }

            return passable || crushable;
        }

        private bool isPassable(HashSet<string> classes)
        {
            return info.PassableClasses.Overlaps(classes);
        }

        private bool isCrushable(HashSet<string> classes)
        {
            return info.CrushesClasses.Overlaps(classes);
        }

        private void attack(Actor actor, string weapon, int damage)
        {
            if (weapon != null)
            {
                var w = self.World.Map.Rules.Weapons[weapon.ToLowerInvariant()];
                w.Impact(Target.FromActor(actor), self, Enumerable.Empty<int>());
            }
            else
            {
                actor.InflictDamage(self, new Damage(damage));
            }
        }
    }
}
