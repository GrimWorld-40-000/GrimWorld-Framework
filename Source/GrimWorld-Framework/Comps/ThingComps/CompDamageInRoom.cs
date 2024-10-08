using System.Linq;
using RimWorld;
using Verse;

namespace GW_Frame.Comps.ThingComps
{
	public class CompProperties_DamageInRoom : CompProperties
	{
		public DamageDef damageType;
		public int damageInterval;
		public FloatRange damageDealt;
		public ThingCategory damagedCategory;
		public bool mustBePowered;
		public bool scaleByFuelPercentage;
		public float minFuelPercentToDamage;
		
		public CompProperties_DamageInRoom()
		{
			compClass = typeof(CompDamageInRoom);
		}
	}
	
	public class CompDamageInRoom: ThingComp
	{
		private float Scaling => Props.scaleByFuelPercentage ? FuelComp.FuelPercentOfMax : 1;
		private CompRefuelable FuelComp => _fuelComp ??= parent.GetComp<CompRefuelable>();
		private CompRefuelable _fuelComp;
		private CompPowerTrader PowerComp => _powerComp ??= parent.GetComp<CompPowerTrader>();
		private CompPowerTrader _powerComp;
		
		protected CompProperties_DamageInRoom Props => (CompProperties_DamageInRoom) props;

		public override void CompTick()
		{
			base.CompTick();
			if (!parent.IsHashIntervalTick(Props.damageInterval)) return;
			if (Props.mustBePowered)
			{
				if (!PowerComp.PowerOn) return;
			}

			if (Props.scaleByFuelPercentage)
			{
				if (FuelComp.FuelPercentOfMax < Props.minFuelPercentToDamage) return;
			}
			
			Room room = parent.GetRoom();
			if (room is not {ProperRoom: true}) return;
			foreach (Thing thing in room.ContainedAndAdjacentThings.Where(thing => thing.def.category == Props.damagedCategory).ToArray())
			{
				thing.TakeDamage(new DamageInfo(Props.damageType, Props.damageDealt.RandomInRange * Scaling,
					instigator: parent));
			}
		}
	}
}