using RimWorld;
using Verse;

namespace GWParryShield;

public class GW_CompParryable : ThingComp
{
    public float parryChance;

    public CompProperties_GW_Parryable Props => (CompProperties_GW_Parryable)props;

    public HediffExtension_GW_Parryable extension =>
        Props.parryHediff.GetModExtension<HediffExtension_GW_Parryable>();

    public float parryChanceGet
    {
        get
        {
            if (parryChance <= 0f)
                parryChance = Props.parryChance;
            return parryChance;
        }
    }

    public Pawn currentWearer
    {
        get
        {
            if (parent is Apparel apparel)
                return apparel.Wearer;
            if (parent?.TryGetComp<CompEquippable>() != null)
                return (parent.ParentHolder as Pawn_EquipmentTracker)?.pawn;
            return null;
        }
    }

    public void DoParry()
    {
        Pawn wearer = currentWearer;
        if (wearer == null) return;
        if (wearer.health.hediffSet.GetFirstHediffOfDef(ParryGalore_DefOf.GW_ParryCooldown) != null)
            return;

        Hediff h = HediffMaker.MakeHediff(Props.parryHediff, wearer);
        h.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Props.parryWindow;
        h.TryGetComp<HediffComp_GW_Parry>().parryCooldown = Props.parryCooldown;
        if (!wearer.health.hediffSet.HasHediff(Props.parryHediff))
            wearer.health.AddHediff(h);
    }
}

public class CompProperties_GW_Parryable : CompProperties
{
    public HediffDef parryHediff;
    public float parryChance = 0.5f;
    public int parryWindow = 60;
    public int parryCooldown = 60;

    public CompProperties_GW_Parryable()
    {
        compClass = typeof(GW_CompParryable);
    }
}
