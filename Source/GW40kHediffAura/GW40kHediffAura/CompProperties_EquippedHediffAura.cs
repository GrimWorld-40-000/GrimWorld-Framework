using Verse;

namespace GW40kHediffAura;

public class CompProperties_EquippedHediffAura : CompProperties
{
	public HediffDef allyOrNeutralHediff;

	public HediffDef hostileHediff;

	public HediffDef ownerFactionHediff;

	public bool affectWearer = true;

	public float severityPerTrigger = 0.25f;

	public int tickInterval = 250;

	public float radius = 8f;

	public CompProperties_EquippedHediffAura()
	{
		compClass = typeof(Comp_EquippedHediffAura);
	}
}
