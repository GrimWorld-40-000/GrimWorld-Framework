using RimWorld;
using Verse;

namespace GW_Frame.Comps.ThingComps
{
	public class CompHeatPusherPoweredScaleByFuel: CompHeatPusherPowered
	{
		private float Scaling => FuelComp.FuelPercentOfMax;
		private CompRefuelable FuelComp => _fuelComp ??= parent.GetComp<CompRefuelable>();
		private CompRefuelable _fuelComp;
		
		public override void CompTick()
		{
			if (!parent.IsHashIntervalTick(60) || !ShouldPushHeatNow)
				return;
			GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * Scaling);
		}

		public override void CompTickRare()
		{
			if (!ShouldPushHeatNow)
				return;
			GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * 4.1666665f * Scaling);
		}
	}
}