using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.WWI.Traits;
using OpenRA.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.WWI.Widgets.Logic
{
    public class IngamePopCounterLogic : ChromeLogic
    {
        [ObjectCreator.UseCtor]
        public IngamePopCounterLogic(Widget widget, World world)
		{
			var populationManager = world.LocalPlayer.PlayerActor.Trait<PopulationManager>();
			var label = widget.Get<LabelWithTooltipWidget>("POP");
            var text = label.Text;

            label.GetText = () => text.F(populationManager.PopulationDrained, populationManager.PopulationDrained + populationManager.ExcessPopulation);
            label.GetTooltipText = () => "Population: {0}/{1}".F(populationManager.PopulationDrained, populationManager.PopulationDrained + populationManager.ExcessPopulation);
		}
    }
}
