using Verse;

namespace GW40kHediffAura;

public class HediffComp_AuraAndMote : HediffComp
{
	public bool isToggleOn = true;

	public HediffCompProperties_AuraAndMote Props => (HediffCompProperties_AuraAndMote)(object)base.props;

	public override void CompExposeData()
	{
		((HediffComp)this).CompExposeData();
		Scribe_Values.Look(ref isToggleOn, "isToggleOn", true, false);
	}
}
