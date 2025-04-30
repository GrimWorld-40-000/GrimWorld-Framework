using Verse;

namespace GW_Frame.Comps.ThingComps
{
	public class CompProperties_DamagedWhileOutdoors : CompProperties_MustBeIndoors
	{
		public float damagePerTick;
		public DamageDef damageDef;
		
		public CompProperties_DamagedWhileOutdoors()
		{
			compClass = typeof(CompDamagedWhileOutdoors);
		}
	}
	
	public class CompDamagedWhileOutdoors: CompMustBeIndoors
	{
		protected new CompProperties_DamagedWhileOutdoors Props => (CompProperties_DamagedWhileOutdoors) props;

		public override void CompTick()
		{
			base.CompTick();
			
			if (!parent.IsHashIntervalTick(250)) 
				return;
			
			if (!IsIndoors())
			{
				parent.TakeDamage(new DamageInfo(Props.damageDef,
					Props.damagePerTick * 250, 1));
			}
		}
	}
}