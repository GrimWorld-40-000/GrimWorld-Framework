using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ChainProjectiles
{
    public class ChainProjectile : Bullet
    {
        public override Vector3 ExactPosition => destination + Vector3.up * def.Altitude;

        public List<Thing> hitThings = new List<Thing>();
        public int? chainLeft;
        public Verb_LaunchProjectile verb;

        public float? ChainRange => def.GetModExtension<ChainProjectileModExtension>()?.chainRange * 2;
        public int ChainLeft => chainLeft ?? (chainLeft = (def.GetModExtension<ChainProjectileModExtension>()?.chainMaxCount).Value) ?? 0;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);

            if (chainLeft <= 0)
            {
                return;
            }

            LocalTargetInfo currentTarget = GetNextTarget(hitThing);
            if (!currentTarget.HasThing)
            {
                return;
            }

            ChainProjectile projectile2 = (ChainProjectile)GenSpawn.Spawn(def, hitThing.Position, hitThing.Map);

            projectile2.hitThings = hitThings;
            projectile2.hitThings.Add(hitThing);
            projectile2.chainLeft = ChainLeft;
            projectile2.verb = verb;

            if (verb == null)
            {
                Log.Error("chain projectile cant chain: no verb set");
                return;
            }

            verb.TryFindShootLineFromTo(hitThing.Position, currentTarget, out ShootLine resultingLine);

            if (verb.EquipmentSource != null)
            {
                verb.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                verb.EquipmentSource.GetComp<CompApparelVerbOwner_Charged>()?.UsedOnce();
            }

            Thing manningPawn = verb.caster;
            Thing equipmentSource = verb.EquipmentSource;
            CompMannable compMannable = verb.caster.TryGetComp<CompMannable>();
            if (compMannable?.ManningPawn != null)
            {
                manningPawn = compMannable.ManningPawn;
                equipmentSource = verb.caster;
            }

            Vector3 drawPos = DrawPos;
            if (equipmentSource.TryGetComp(out CompUniqueWeapon comp))
            {
                foreach (WeaponTraitDef item in comp.TraitsListForReading)
                {
                    if (item.damageDefOverride != null)
                    {
                        projectile2.damageDefOverride = item.damageDefOverride;
                    }

                    if (!item.extraDamages.NullOrEmpty())
                    {
                        Projectile projectile3 = projectile2;
                        if (projectile3.extraDamages == null)
                        {
                            projectile3.extraDamages = new List<ExtraDamage>();
                        }

                        projectile2.extraDamages.AddRange(item.extraDamages);
                    }
                }
            }

            // ########## chain projectiles should not have forced miss. if it can miss, it wouldnt have chained to the target
            //if (verb.verbProps.ForcedMissRadius > 0.5f)
            //{
            //    float num = verb.verbProps.ForcedMissRadius;
            //    if (manningPawn is Pawn pawn)
            //    {
            //        num *= verb.verbProps.GetForceMissFactorFor(equipmentSource, pawn);
            //    }

            //    float num2 = VerbUtility.CalculateAdjustedForcedMiss(num, currentTarget.Cell - verb.caster.Position);
            //    if (num2 > 0.5f)
            //    {
            //        IntVec3 forcedMissTarget = verb.GetForcedMissTarget(num2);
            //        if (forcedMissTarget != currentTarget.Cell)
            //        {
            //            verb.ThrowDebugText("ToRadius");
            //            verb.ThrowDebugText("Rad\nDest", forcedMissTarget);
            //            ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
            //            if (Rand.Chance(0.5f))
            //            {
            //                projectileHitFlags = ProjectileHitFlags.All;
            //            }

            //            if (!verb.canHitNonTargetPawnsNow)
            //            {
            //                projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
            //            }

            //            projectile2.Launch(manningPawn, drawPos, forcedMissTarget, currentTarget, projectileHitFlags, preventFriendlyFire, equipmentSource);
            //            return;
            //        }
            //    }
            //}

            ShotReport shotReport = ShotReport.HitReportFor(verb.caster, verb, currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;

            // ############ chain projectile should not be able to go wild
            //if (verb.verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            //{
            //    bool flyOverhead = projectile2?.def?.projectile != null && projectile2.def.projectile.flyOverhead;
            //    resultingLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget, flyOverhead, caster.Map);
            //    //ThrowDebugText("ToWild" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
            //    //ThrowDebugText("Wild\nDest", resultingLine.Dest);
            //    ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
            //    //if (Rand.Chance(0.5f) && verb.canHitNonTargetPawnsNow)
            //    //{
            //    //    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
            //    //}

            //    projectile2.Launch(manningPawn, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags2, preventFriendlyFire, equipmentSource, targetCoverDef);
            //    return;
            //}

            if (currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover && !Rand.Chance(shotReport.PassCoverChance))
            {
                //ThrowDebugText("ToCover" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
                //ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                //if (verb.canHitNonTargetPawnsNow)
                //{
                //    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                //}

                projectile2.Launch(manningPawn, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, preventFriendlyFire, equipmentSource, targetCoverDef);
                return;
            }

            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            //if (verb.canHitNonTargetPawnsNow)
            //{
            //    projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            //}

            if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }

            //ThrowDebugText("ToHit" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
            if (currentTarget.Thing != null)
            {
                projectile2.Launch(manningPawn, drawPos, currentTarget, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
                //ThrowDebugText("Hit\nDest", currentTarget.Cell);
            }
            else
            {
                projectile2.Launch(manningPawn, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
                //ThrowDebugText("Hit\nDest", resultingLine.Dest);
            }
        }

        LocalTargetInfo GetNextTarget(Thing fromThing)
        {
            if (fromThing == null || !fromThing.Spawned)
            {
                //Log.Error("flag1");
                return new LocalTargetInfo();
            }
            if (ChainRange == null)
            {
                //Log.Error("flag2");
                Log.Error("chain projectile defined without ChainProjectileModExtension. defaulting to 0 chainRange");
            }
            Dictionary<Pawn, float> targets = new Dictionary<Pawn, float>();
            foreach (IntVec3 radialCell in GenRadial.RadialPatternInRadius(ChainRange.Value))
            {
                //Log.Error("flag3");
                IntVec3 cell = radialCell + fromThing.Position;
                if (!cell.InBounds(fromThing.Map))
                {
                    continue;
                }
                List<Thing> targetsAt = fromThing.Map.thingGrid.ThingsListAt(radialCell + fromThing.Position);
                if (targetsAt == null)
                {
                    //Log.Error("flag4");
                    continue;
                }
                foreach (Thing target in targetsAt)
                {
                    //Log.Error("flag5");
                    if (target is Pawn pawn && pawn.Spawned && pawn != verb.CasterPawn)
                    {
                        targets.SetOrAdd(pawn, fromThing.Position.DistanceTo(target.Position));
                    }
                }
            }

            //Log.Error("flag6");
            targets.OrderBy(d => d.Value);

            Thing toThing = null;
            foreach (Thing target in targets.Keys)
            {
                //Log.Error("flag7");
                if (GenSight.LineOfSight(fromThing.Position, target.Position, fromThing.Map) && !hitThings.Contains(target))
                {
                    if (!(target is Pawn))
                    {
                        continue;
                    }
                    toThing = target;
                    break;
                }
            }

            return toThing;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hitThings, "hitThings");
            Scribe_Values.Look(ref chainLeft, "chainLeft");
            Scribe_References.Look(ref verb, "verb");
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

            if (verb == null)
            {
                if (launcher is IAttackTargetSearcher shooter && shooter.CurrentEffectiveVerb is Verb_LaunchProjectile verb)
                {
                    this.verb = verb;
                }
                else
                {
                    Log.Error("ChainProjectile launched without verb");
                }
            }

            Vector3 offsetA = (ExactPosition - launcher.Position.ToVector3Shifted()).Yto0().normalized * def.projectile.beamStartOffset;
            if (def.projectile.beamMoteDef != null)
            {
                MoteMaker.MakeInteractionOverlay(def.projectile.beamMoteDef, hitThings.Count > 0 ? hitThings.Last() : launcher, usedTarget.ToTargetInfo(launcher.Map), offsetA, Vector3.zero);
            }

            ImpactSomething();

            Log.Warning("launched with ChainLeft: " + ChainLeft + ", verb: " + verb + ", hitThings count: " + hitThings.Count + ", ChainRange: " + ChainRange);
        }
    }
}
