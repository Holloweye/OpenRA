using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Traits
{
    [Desc("As long as this actor is in world the sound volume is modified.")]
    public class SoundModifierInfo : ITraitInfo
    {
        [Desc("What volume? 100 = 100%.")]
        public readonly int Volume = 100;

        public object Create(ActorInitializer init) { return new SoundModifier(init.Self, this); }
    }

    public class SoundModifier : IWorldLoaded
    {
        readonly SoundModifierInfo info;

        public SoundModifier(Actor self, SoundModifierInfo info)
		{
			this.info = info;
		}

        ~SoundModifier()
        {
            Game.Sound.SoundVolumeModifier = 1.0f;
        }

        public void WorldLoaded(World w, Graphics.WorldRenderer wr)
        {
            Game.Sound.SoundVolumeModifier = (float)info.Volume / 100.0f;
        }
    }
}
