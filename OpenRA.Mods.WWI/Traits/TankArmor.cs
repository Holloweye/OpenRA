using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRA.Mods.WWI.Traits
{
    [Desc("Modifies damange taken depending on projectile angle.")]
    public class TankArmorInfo : ITraitInfo
    {
        [Desc("Frontal attack modifier 0 = Takes no damage, 100 = Takes full damage.")]
        public readonly int FrontalDamageModifier = 100;
        [Desc("Side attack modifier 0 = Takes no damage, 100 = Takes full damage.")]
        public readonly int SideDamageModifier = 100;
        [Desc("Back attack modifier 0 = Takes no damage, 100 = Takes full damage.")]
        public readonly int BackDamageModifier = 100;

        public object Create(ActorInitializer init) { return new TankArmor(init.Self, this); }
    }

    class TankArmor : IDamageModifier
    {
        readonly Actor self;
        readonly TankArmorInfo info;

        public TankArmor(Actor self, TankArmorInfo info)
        {
            this.self = self;
            this.info = info;
        }

        public int GetDamageModifier(Actor attacker, Damage damage)
        {
            int modifier;

            // Not entirely accurate as it uses positions of actors and not warhead.
            // TODO: Use warhead angle if possible?

            var angle = (self.CenterPosition - attacker.CenterPosition).Yaw.Facing - getFacingAngle(self).Facing;

            if (isFrontalAttack(angle))
            {
                modifier = info.FrontalDamageModifier;
            }
            else if (isBackAttack(angle))
            {
                modifier = info.BackDamageModifier;
            }
            else
            {
                modifier = info.SideDamageModifier;
            }

            return modifier;
        }

        private bool isFrontalAttack(int angle)
        {
            return angle >= 179 || angle <= -13;
        }

        private bool isBackAttack(int angle)
        {
            return angle >= 51 && angle <= 115;
        }

        private WAngle getFacingAngle(Actor a)
        {
            IFacing facing = a.TraitOrDefault<IFacing>();
            return new WAngle((int)(facing != null ? (float)WAngle.FromFacing(facing.Facing).Angle / 1024.0f * 360.0f : 0.0f));
        }
    }
}
