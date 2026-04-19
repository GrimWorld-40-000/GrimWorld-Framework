using RimWorld;
using Verse;

namespace GWParryShield;

public class HediffComp_GW_Posebreak : HediffComp
{
    public bool isTakenDamage;

    public override bool CompShouldRemove => isTakenDamage;

    public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
        if (dinfo.Instigator is Pawn)
            isTakenDamage = true;
    }
}

public class HediffComp_GW_Parry : HediffComp
{
    public bool isTriggered = false;
    public int parryCooldown = 0;

    public HediffExtension_GW_Parryable modExtension =>
        parent.def.GetModExtension<HediffExtension_GW_Parryable>();

    public override bool CompShouldRemove => isTriggered;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
    }

    public void giveHediff(Pawn target, HediffDef hediffDef, int duration, float severity)
    {
        Hediff h = ParryUtility.CreateHediff(target, hediffDef, duration, severity);
        Pawn.health.AddHediff(h);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        giveHediff(Pawn, ParryGalore_DefOf.GW_ParryCooldown, parryCooldown, 1f);
    }
}
