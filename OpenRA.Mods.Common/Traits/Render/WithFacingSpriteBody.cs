#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class WithFacingSpriteBodyInfo : WithSpriteBodyInfo, Requires<IBodyOrientationInfo>, Requires<IFacingInfo>
	{
		public override object Create(ActorInitializer init) { return new WithFacingSpriteBody(init, this); }

		public override IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, RenderSpritesInfo rs, string image, int facings, PaletteReference p)
		{
			var ifacing = init.Actor.Traits.GetOrDefault<IFacingInfo>();
			var facing = ifacing != null ? init.Contains<FacingInit>() ? init.Get<FacingInit, int>() : ifacing.GetInitialFacing() : 0;

			var anim = new Animation(init.World, image, () => facing);
			anim.PlayRepeating(RenderSprites.NormalizeSequence(anim, init.GetDamageState(), Sequence));

			yield return new SpriteActorPreview(anim, WVec.Zero, 0, p, rs.Scale);
		}

		public override int QuantizedBodyFacings(ActorInfo ai, SequenceProvider sequenceProvider, string race)
		{
			var rsi = ai.Traits.Get<RenderSpritesInfo>();
			return sequenceProvider.GetSequence(rsi.GetImage(ai, sequenceProvider, race), Sequence).Facings;
		}
	}

	public class WithFacingSpriteBody : WithSpriteBody
	{
		public WithFacingSpriteBody(ActorInitializer init, WithFacingSpriteBodyInfo info)
			: base(init, info, RenderSprites.MakeFacingFunc(init.Self)) { }
	}
}
