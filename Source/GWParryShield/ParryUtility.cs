using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GWParryShield;

public static class ParryUtility
{
    public static Hediff GetParryHediff(Pawn p)
    {
        List<Hediff> hediffs = p.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++)
        {
            if (hediffs[i].def.GetModExtension<HediffExtension_GW_Parryable>() != null)
                return hediffs[i];
        }
        return null;
    }

    public static Hediff CreateHediff(Pawn p, HediffDef hediffDef, int duration, float severity)
    {
        Hediff h = HediffMaker.MakeHediff(hediffDef, p);
        h.TryGetComp<HediffComp_Disappears>().ticksToDisappear = duration;
        if (severity > 0f)
            h.Severity = severity;
        return h;
    }

    public static void CheckExtraEffect(HediffExtension_GW_Parryable modExtension, Pawn user = null, Pawn attacker = null)
    {
        if (modExtension == null || user == null) return;

        if (modExtension.isDoDamge)
        {
            if (modExtension.isDoDamageToSelf)
            {
                user.TakeDamage(new DamageInfo(modExtension.damageDef, modExtension.damageAmount, modExtension.armorPen, instigator: null));
            }
            else if (modExtension.isDoDamageToAttacker && attacker != null)
            {
                attacker.TakeDamage(new DamageInfo(modExtension.damageDef, modExtension.damageAmount, modExtension.armorPen, instigator: null));
            }
        }

        if (modExtension.isGiveBuff)
        {
            Hediff buff = CreateHediff(user, modExtension.buffHediff, modExtension.buffDuration, modExtension.buffSeverity);
            user.health.AddHediff(buff);
        }

        if (modExtension.isGiveDebuff)
        {
            if (modExtension.isDebuffToSelf)
            {
                Hediff debuff = CreateHediff(user, modExtension.debuffHediff, modExtension.debuffDuration, modExtension.debuffSeverity);
                user.health.AddHediff(debuff);
            }
            else if (attacker != null)
            {
                Hediff debuff = CreateHediff(attacker, modExtension.debuffHediff, modExtension.debuffDuration, modExtension.debuffSeverity);
                attacker.health.AddHediff(debuff);
            }
        }

        if (modExtension.isDoingExplosion)
        {
            GenExplosion.DoExplosion(
                user.Position, user.Map,
                modExtension.explosiveRadius, modExtension.damageDef, user,
                modExtension.damageAmount, modExtension.armorPen,
                ignoredThings: new List<Thing> { user });
        }

        if (modExtension.isKnockbackAttacker && attacker != null
            && attacker.Position.DistanceTo(user.Position) <= 2f)
        {
            IntVec3 delta = new IntVec3(
                Mathf.Max(user.PositionHeld.x, attacker.PositionHeld.x) - Mathf.Min(user.PositionHeld.x, attacker.PositionHeld.x),
                0,
                Mathf.Max(user.PositionHeld.z, attacker.PositionHeld.z) - Mathf.Min(user.PositionHeld.z, attacker.PositionHeld.z));

            if (user.PositionHeld.x > attacker.PositionHeld.x)
            {
                delta.x *= -1;
                delta.x += modExtension.knockbackDistance * -1;
            }
            else
            {
                delta.x += modExtension.knockbackDistance;
            }

            if (user.PositionHeld.z > attacker.PositionHeld.z)
            {
                delta.z *= -1;
                delta.z += modExtension.knockbackDistance * -1;
            }
            else if (user.PositionHeld.z != attacker.PositionHeld.z)
            {
                delta.z += modExtension.knockbackDistance;
            }

            IntVec3 target = delta + attacker.PositionHeld;
            IntVec3 dest = GenSight.LastPointOnLineOfSight(attacker.Position, target, x => !x.Impassable(attacker.Map), false);
            attacker.Position = dest;
            attacker.Notify_Teleported(true, true);
        }

        if (!modExtension.parrySounds.NullOrEmpty())
            modExtension.parrySounds.RandomElement().PlayOneShot(new TargetInfo(user.Position, user.Map));

        if (!modExtension.parryEffects.NullOrEmpty())
        {
            Effecter e = modExtension.parryEffects.RandomElement().Spawn(user.Position, user.Map);
            e.Cleanup();
        }

        GetParryHediff(user)?.TryGetComp<HediffComp_GW_Parry>()?.Let(c => c.isTriggered = true);
    }
}

// Helper to avoid statement-expression limitation on property assignment
internal static class ParryExtensions
{
    internal static void Let<T>(this T obj, System.Action<T> action) where T : class
    {
        if (obj != null) action(obj);
    }
}
