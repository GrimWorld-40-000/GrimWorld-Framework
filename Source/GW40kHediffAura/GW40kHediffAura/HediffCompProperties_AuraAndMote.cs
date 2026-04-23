using Verse;

namespace GW40kHediffAura;

public class HediffCompProperties_AuraAndMote : HediffCompProperties
{
	public HediffDef allyOrNeutralHediff;

	public HediffDef hostileHediff;

	public HediffDef ownerFactionHediff;

	public bool affectWearer = true;

	public float severityPerTrigger;

	public int tickInterval;

	public float radius = 5f;

	public ThingDef mote;

	public string uiIconEnabled;

	public string uiIconDisabled;

	public HediffCompProperties_AuraAndMote()
	{
		base.compClass = typeof(HediffComp_AuraAndMote);
	}
}
