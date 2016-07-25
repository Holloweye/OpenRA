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
            int facing = getFacingAngle(self);
            int angleOfAttack = angleBetweenActors(self, attacker);
            if (isFrontalAttack(facing, angleOfAttack))
            {
                modifier = info.FrontalDamageModifier;
            }
            else if (isSideAttack(facing, angleOfAttack))
            {
                modifier = info.SideDamageModifier;
            }
            else 
            {
                modifier = info.BackDamageModifier;
            }

            return modifier;
        }

        private bool isFrontalAttack(int facing, int angleOfAttack)
        {
            return distance(facing, angleOfAttack) > 120;
        }

        private bool isSideAttack(int facing, int angleOfAttack)
        {
            int dist = distance(facing, angleOfAttack);
            return dist > 60 && dist <= 120;
        }

        private int distance(int alpha, int beta)
        {
            int phi = Math.Abs(beta - alpha) % 360;
            int distance = phi > 180 ? 360 - phi : phi;
            return distance;
        }

        private int angleBetweenActors(Actor a, Actor b)
        {
            int angle = 0;
            if (a != null && b != null)
            {
                int deltaX = a.Location.X - b.Location.X;
                int deltaY = a.Location.Y - b.Location.Y;

                angle = (int)(Math.Atan2(deltaY, deltaX) * 180.0f / Math.PI) + 90;
            }
            return angle;
        }

        private int getFacingAngle(Actor a)
        {
            IFacing facing = a.TraitOrDefault<IFacing>();
            return (int)(facing != null ? (float)WAngle.FromFacing(facing.Facing).Angle / 1024.0f * 360.0f : 0);
        }
    }
}
