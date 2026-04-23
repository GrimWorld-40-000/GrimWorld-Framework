using Verse;

namespace GW40kHediffAura;

public class CompProperties_ApparelGiveHediff : CompProperties
{
	public HediffDef hediffDef;

	public CompProperties_ApparelGiveHediff()
	{
		base.compClass = typeof(Comp_ApparelGiveHediff);
	}
}
