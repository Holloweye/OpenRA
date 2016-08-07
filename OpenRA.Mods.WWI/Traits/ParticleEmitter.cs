using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.WWI.Effects;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Traits
{
    public class ParticleEmitterInfo : UpgradableTraitInfo
    {
        [Desc("Offset for the particle emitter.")]
        public readonly WVec OffsetMin = WVec.Zero;

        [Desc("Offset for the particle emitter.")]
        public readonly WVec OffsetMax = WVec.Zero;

        [Desc("Randomize particle gravity.")]
        public readonly WVec GravityMin = new WVec(0, 1, 0);

        [Desc("Randomize particle gravity.")]
        public readonly WVec GravityMax = new WVec(0, 1, 0);

        [Desc("How many particles should spawn.")]
        public readonly WVec SpawnFrequency = new WVec(100, 150, 0);

        [Desc("Which sprite to use.")]
        public readonly string Sprite = null;

        [Desc("Which sequence to use.")]
        public readonly string Sequence = null;

        [Desc("Which palette to use.")]
        public readonly string Palette = null;

        public override object Create(ActorInitializer init) { return new ParticleEmitter(init.Self, this); }
    }

    class ParticleEmitter : UpgradableTrait<ParticleEmitterInfo>, ITick
    {
        int ticks;

        WVec offset = WVec.Zero;

        public ParticleEmitter(Actor self, ParticleEmitterInfo info)
			: base(info) 
        {
            offset = new WVec(Game.CosmeticRandom.Next(Info.OffsetMin.X, Info.OffsetMax.X),
                              Game.CosmeticRandom.Next(Info.OffsetMin.Y, Info.OffsetMax.Y),
                              Game.CosmeticRandom.Next(Info.OffsetMin.Z, Info.OffsetMax.Z));
        }

        public void Tick(Actor self)
        {
            if (IsTraitDisabled)
                return;

            if (--ticks < 0)
            {
                ticks = Game.CosmeticRandom.Next(Info.SpawnFrequency.X, Info.SpawnFrequency.Y);

                var pos = new WPos(self.CenterPosition.X + offset.X,
                                   self.CenterPosition.Y + offset.Y, 
                                   self.CenterPosition.Z + offset.Z);

                self.World.AddFrameEndTask(w => w.Add(new Particle(pos, new WPos(Game.CosmeticRandom.Next(Info.GravityMin.X, Info.GravityMax.X), Game.CosmeticRandom.Next(Info.GravityMin.Y, Info.GravityMax.Y), 0), w, Info.Sprite, Info.Sequence, Info.Palette, false, false)));
            }
        }
    }
}
