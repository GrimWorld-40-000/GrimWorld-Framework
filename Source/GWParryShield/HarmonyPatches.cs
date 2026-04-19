using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GWParryShield;

[DefOf]
internal static class ParryGalore_DefOf
{
    public static HediffDef GW_ParryCooldown;
}

[StaticConstructorOnStartup]
public class StarPatch
{
    static StarPatch()
    {
        new Harmony("FarmerJoe.GWVersionParryGalore").PatchAll();
    }
}

[HarmonyPatch(typeof(Bullet), "Impact")]
public class Bullet_ImpactPatch
{
    private static void Postfix(ref Thing hitThing, ref Bullet __instance)
    {
        if (__instance.Launcher == null || hitThing == null)
            return;

        if (hitThing is not Pawn pawn)
            return;

        Hediff parryHediff = ParryUtility.GetParryHediff(pawn);
        if (parryHediff == null)
            return;

        HediffExtension_GW_Parryable modExtension = parryHediff.def.GetModExtension<HediffExtension_GW_Parryable>();
        if (modExtension == null)
            return;

        bool projectileBlacklisted = !modExtension.blacklistedProjectileDef.NullOrEmpty()
            && modExtension.blacklistedProjectileDef.Contains(__instance.def);

        if (modExtension.isReflectProjectile
            && Rand.Value <= modExtension.reflectProjectileChance
            && !projectileBlacklisted)
        {
            Projectile reflected = (Projectile)GenSpawn.Spawn(__instance.def, hitThing.Position.ClampInsideMap(hitThing.Map), hitThing.Map);
            reflected.Launch(hitThing, __instance.Launcher, __instance.Launcher, ProjectileHitFlags.IntendedTarget);

            if (__instance.Launcher is Pawn attacker)
                ParryUtility.CheckExtraEffect(modExtension, pawn, attacker);
            else
                ParryUtility.CheckExtraEffect(modExtension, pawn);
        }
    }
}

[HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
public class Pawn_PreApplyDamage_Parry
{
    private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
    {
        if (__instance == null || !__instance.RaceProps.Humanlike)
            return;

        GW_CompParryable parryComp = null;

        Apparel parryApparel = __instance.apparel?.WornApparel
            .FirstOrDefault(a => a.TryGetComp<GW_CompParryable>() != null);

        if (parryApparel != null)
        {
            parryComp = parryApparel.TryGetComp<GW_CompParryable>();
        }
        else if (__instance.equipment?.Primary != null)
        {
            parryComp = __instance.equipment.Primary.TryGetComp<GW_CompParryable>();
        }

        if (parryComp == null)
            return;

        if (!(Rand.Value <= parryComp.parryChanceGet))
            return;

        parryComp.DoParry();

        Hediff parryHediff = ParryUtility.GetParryHediff(__instance);
        if (parryHediff == null)
            return;

        HediffExtension_GW_Parryable ext = parryHediff.def.GetModExtension<HediffExtension_GW_Parryable>();
        if (ext == null)
            return;

        if (!ext.blacklistedDamageDefs.NullOrEmpty() && ext.blacklistedDamageDefs.Contains(dinfo.Def))
            return;

        float originalAmount = dinfo.Amount;

        if (ext.isNullifyDamage && Rand.Value <= ext.nullifyDamageChance)
            dinfo.SetAmount(0f);

        if (ext.isDamageShieldHp && parryApparel != null)
            parryApparel.TakeDamage(new DamageInfo(DamageDefOf.Blunt, ext.damageToShieldAmount, instigator: dinfo.Instigator));

        if (ext.isReduceDamageTakenWhenParry)
            dinfo.SetAmount(dinfo.Amount * (1f - ext.damageReduction));

        if (ext.isStunTarget && dinfo.Instigator is Pawn attacker && !dinfo.Def.isRanged)
            attacker.stances?.stunner?.StunFor(ext.stunDuration, __instance);

        if (ext.isReflectDamage && dinfo.Instigator != null && Rand.Value <= ext.reflectDamageChance)
            dinfo.Instigator.TakeDamage(new DamageInfo(dinfo.Def, originalAmount * ext.reflectDamageMultiplier, dinfo.ArmorPenetrationInt, dinfo.Angle, __instance));

        if (ext.isDoDamageToAttacker && dinfo.Instigator != null)
            dinfo.Instigator.TakeDamage(new DamageInfo(ext.damageDef, ext.damageAmount, ext.armorPen, instigator: __instance));

        if (dinfo.Instigator is Pawn attackerPawn)
            ParryUtility.CheckExtraEffect(ext, __instance, attackerPawn);
        else
            ParryUtility.CheckExtraEffect(ext, __instance);

        MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "Parry!");
    }
}
