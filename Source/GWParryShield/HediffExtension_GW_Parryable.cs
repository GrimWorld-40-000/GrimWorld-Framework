using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GWParryShield;

public class HediffExtension_GW_Parryable : DefModExtension
{
    public int parryCooldown = 60;
    public float baseParryChance = 0.1f;

    public bool isStunTarget = false;
    public int stunDuration = 120;

    public bool isNullifyDamage;
    public float nullifyDamageChance = 1f;

    public bool isDamageShieldHp;
    public int damageToShieldAmount;

    public bool isReduceDamageTakenWhenParry;
    public float damageReduction = 0f;

    public bool isGiveBuff;
    public int buffDuration;
    public HediffDef buffHediff;
    public float buffSeverity;

    public bool isGiveDebuff;
    public int debuffDuration;
    public bool isDebuffToSelf;
    public HediffDef debuffHediff;
    public float debuffSeverity;

    public bool isReflectDamage;
    public float reflectDamageChance = 1f;
    public float reflectDamageMultiplier = 1f;

    public bool isDoDamageToAttacker;
    public bool isDoDamageToSelf;
    public bool isDoDamge;
    public DamageDef damageDef;
    public int damageAmount;
    public float armorPen;

    public bool isDoingExplosion;
    public float explosiveRadius;

    public bool isKnockbackAttacker;
    public int knockbackDistance;

    public bool isReflectProjectile;
    public float reflectProjectileChance = 1f;

    public List<DamageDef> blacklistedDamageDefs;
    public List<ThingDef> blacklistedProjectileDef;

    public List<SoundDef> parrySounds;
    public List<EffecterDef> parryEffects;
}
