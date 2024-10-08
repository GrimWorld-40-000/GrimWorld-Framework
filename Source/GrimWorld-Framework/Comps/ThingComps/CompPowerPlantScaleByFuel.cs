using RimWorld;

namespace GW_Frame.Comps.ThingComps
{
	public class CompPowerPlantScaleByFuel: CompPowerPlant
	{
		protected override float DesiredPowerOutput => -Props.PowerConsumption * refuelableComp.FuelPercentOfMax;
	}
}