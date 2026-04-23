using Verse;

namespace GW40kHediffAura;

public class Comp_ApparelGiveHediff : ThingComp
{
	private Mote mote;

	private Pawn wearer;

	public CompProperties_ApparelGiveHediff Props => (CompProperties_ApparelGiveHediff)(object)base.props;

	public override void PostExposeData()
	{
		((ThingComp)this).PostExposeData();
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		((ThingComp)this).Notify_Equipped(pawn);
		Hediff val = HediffMaker.MakeHediff(Props.hediffDef, pawn, (BodyPartRecord)null);
		val.Severity = 1f;
		pawn.health.AddHediff(val);
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		((ThingComp)this).Notify_Unequipped(pawn);
		pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef, false));
	}
}
