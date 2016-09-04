using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.WWI.Orders;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Traits
{
    [Desc("Increases veterancy of selected units.")]
    public class InstantVeterancyPowerInfo : SupportPowerInfo
    {
        [Desc("Target actor selection radius in cells.")]
        public readonly int Size = 1;

        [Desc("Amount of levels units gain.")]
        public readonly int LevelGain = 1;

        [Desc("Cursor sequence to use when selecting targets.")]
        public readonly string SelectionCursor = "chrono-select";

        [PaletteReference]
        public readonly string TargetOverlayPalette = TileSet.TerrainPaletteInternalName;

        public readonly string OverlaySpriteGroup = "overlay";
        [SequenceReference("OverlaySpriteGroup", true)]
        public readonly string ValidTileSequencePrefix = "target-valid-";
        [SequenceReference("OverlaySpriteGroup")]
        public readonly string InvalidTileSequence = "target-invalid";
        [SequenceReference("OverlaySpriteGroup")]
        public readonly string SourceTileSequence = "target-select";

        public override object Create(ActorInitializer init) { return new InstantVeterancyPower(init.Self, this); }
    }

    class InstantVeterancyPower : SupportPower
    {
        public InstantVeterancyPower(Actor self, SupportPowerInfo info) : base(self, info) { }

        public override void SelectTarget(Actor self, string order, SupportPowerManager manager)
        {
            var info = Info as InstantVeterancyPowerInfo;

            Game.Sound.PlayToPlayer(manager.Self.Owner, Info.SelectTargetSound);
            self.World.OrderGenerator = new ChargeTarget(Self.World, order, manager, this);
        }

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);

            var info = Info as InstantVeterancyPowerInfo;

            foreach (var target in UnitsInRange(order.TargetLocation))
            {
                target.Trait<GainsExperience>().GiveLevels(1);
            }
        }

        public IEnumerable<Actor> UnitsInRange(CPos xy)
        {
            var range = ((InstantVeterancyPowerInfo)Info).Size;
            var tiles = Self.World.Map.FindTilesInCircle(xy, range);
            var units = new HashSet<Actor>();
            foreach (var t in tiles)
                units.UnionWith(Self.World.ActorMap.GetActorsAt(t));

            return units.Where(a => a.Info.HasTraitInfo<MobileInfo>());
        }

        class ChargeTarget : IOrderGenerator
        {
            readonly InstantVeterancyPower power;
            readonly int range;
            readonly Sprite tile;
            readonly SupportPowerManager manager;
            readonly string order;

            public ChargeTarget(World world, string order, SupportPowerManager manager, InstantVeterancyPower power)
            {
                // Clear selection if using Left-Click Orders
                if (Game.Settings.Game.UseClassicMouseStyle)
                    manager.Self.World.Selection.Clear();

                this.manager = manager;
                this.order = order;
                this.power = power;

                var info = (InstantVeterancyPowerInfo)power.Info;
                range = info.Size;
                tile = world.Map.Rules.Sequences.GetSequence(info.OverlaySpriteGroup, info.SourceTileSequence).GetSprite(0);
            }

            public IEnumerable<Order> Order(World world, CPos cell, int2 worldPixel, MouseInput mi)
            {
                world.CancelInputMode();
                if (mi.Button == MouseButton.Left && power.UnitsInRange(cell).Any())
                    yield return new Order(order, manager.Self, false) { TargetLocation = cell, SuppressVisualFeedback = true };
            }

            public void Tick(World world)
            {
                // Cancel the OG if we can't use the power
                if (!manager.Powers.ContainsKey(order))
                    world.CancelInputMode();
            }

            public IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world)
            {
                var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
                var targetUnits = power.UnitsInRange(xy).Where(a => !world.FogObscures(a));

                foreach (var unit in targetUnits)
                    if (manager.Self.Owner.CanTargetActor(unit))
                        yield return new SelectionBoxRenderable(unit, Color.Red);
            }

            public IEnumerable<IRenderable> Render(WorldRenderer wr, World world)
            {
                var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
                var tiles = world.Map.FindTilesInCircle(xy, range);
                var palette = wr.Palette(((InstantVeterancyPowerInfo)power.Info).TargetOverlayPalette);
                foreach (var t in tiles)
                    yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(t), WVec.Zero, -511, palette, 1f, true);
            }

            public string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
            {
                return ((InstantVeterancyPowerInfo)power.Info).SelectionCursor;
            }
        }
    }
}
