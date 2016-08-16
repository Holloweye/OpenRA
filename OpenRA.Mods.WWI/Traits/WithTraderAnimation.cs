using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Traits
{
    class WithTraderAnimationInfo : ITraitInfo, Requires<WithSpriteBodyInfo>, Requires<TraderInfo>
    {
        [Desc("Prefix added when trader is full.")]
        [SequenceReference]
        public readonly string FullPreix = "full-";

        [Desc("Prefix added when trader is empty.")]
        [SequenceReference]
        public readonly string EmptyPreix = "empty-";

        public object Create(ActorInitializer init) { return new WithTraderAnimation(init, this); }
    }

    class WithTraderAnimation : ITick
    {
        public readonly WithTraderAnimationInfo Info;
		readonly WithSpriteBody wsb;
		readonly Trader trader;

        public WithTraderAnimation(ActorInitializer init, WithTraderAnimationInfo info)
		{
			Info = info;
			trader = init.Self.Trait<Trader>();
			wsb = init.Self.Trait<WithSpriteBody>();
		}

		protected virtual string NormalizeTraderSequence(Actor self, string baseSequence)
		{
			var desiredPrefix = trader.isLoaded ? Info.FullPreix : Info.EmptyPreix;

			if (wsb.DefaultAnimation.HasSequence(desiredPrefix + baseSequence))
				return desiredPrefix + baseSequence;
			else
				return baseSequence;
		}

		public void Tick(Actor self)
		{
			var baseSequence = wsb.NormalizeSequence(self, wsb.Info.Sequence);
            var sequence = NormalizeTraderSequence(self, baseSequence);
			if (wsb.DefaultAnimation.HasSequence(sequence) && wsb.DefaultAnimation.CurrentSequence.Name != sequence)
				wsb.DefaultAnimation.ReplaceAnim(sequence);
		}
    }
}
