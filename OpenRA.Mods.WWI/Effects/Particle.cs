using OpenRA.Effects;
using OpenRA.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Effects
{
    class Particle : IEffect
    {
        readonly World world;
        readonly string palette;
        readonly Animation anim;
        readonly WPos gravity;
        readonly bool visibleThroughFog;
        readonly bool scaleSizeWithZoom;

        private WPos pos;

        public Particle(WPos pos, WPos gravity, World world, string image, string sequence, string palette, bool visibleThroughFog = false, bool scaleSizeWithZoom = false)
		{
			this.world = world;
			this.pos = pos;
            this.gravity = gravity;
			this.palette = palette;
			this.scaleSizeWithZoom = scaleSizeWithZoom;
			this.visibleThroughFog = visibleThroughFog;
			anim = new Animation(world, image, () => 0);
			anim.PlayThen(sequence, () => world.AddFrameEndTask(w => w.Remove(this)));
		}

        public void Tick(World world)
        {
            anim.Tick();
            pos = new WPos(pos.X + gravity.X, pos.Y + gravity.Y, pos.Z + gravity.Z);
        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (world.FogObscures(pos) && !visibleThroughFog)
                return SpriteRenderable.None;

            var zoom = scaleSizeWithZoom ? 1f / wr.Viewport.Zoom : 1f;
            return anim.Render(pos, WVec.Zero, 0, wr.Palette(palette), zoom);
        }
    }
}
