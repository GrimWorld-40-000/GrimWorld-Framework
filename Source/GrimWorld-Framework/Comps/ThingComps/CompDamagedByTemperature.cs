using UnityEngine;
using Verse;

namespace GW_Frame.Comps.ThingComps
{
	public class CompProperties_DamagedByTemperature : CompProperties
	{
		public SimpleCurve overHeatCurve;
		public float damagePerTick;
		public DamageDef damageDef;
		public float maxSafeTemperature;
		
		public CompProperties_DamagedByTemperature()
		{
			compClass = typeof(CompDamagedByTemperature);
		}
	}
	
	
	public class CompDamagedByTemperature: ThingComp
	{
		protected CompProperties_DamagedByTemperature Props => (CompProperties_DamagedByTemperature) props;

		public override void CompTick()
		{
			base.CompTick();
			
			//TODO is .AmbientTemperature faster or slower than just checking the room temp/map temp
			//parent.GetRoom()?.Temperature ?? parent.Map.mapTemperature.OutdoorTemp;
			//parent.AmbientTemperature
			if (!parent.IsHashIntervalTick(250)) return;
			{
				if (IsTooHot)
				{
					parent.TakeDamage(new DamageInfo(Props.damageDef, 
						GetDamagePerRareTick(parent.AmbientTemperature), 1));
				}
			}
		}

		public bool IsTooHot => parent.AmbientTemperature > Props.maxSafeTemperature;

		private float GetDamagePerRareTick(float temperature) => Props.damagePerTick * 250 *
		                                                         Props.overHeatCurve.Evaluate(temperature -
			                                                         Props.maxSafeTemperature);

		public override string CompInspectStringExtra()
		{
			return "GW_ThingDamagedByTemperature".Translate(Props.maxSafeTemperature.ToStringTemperature());
		}
	}
}