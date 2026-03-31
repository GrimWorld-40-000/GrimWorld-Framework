using System;
using System.Collections.Generic;
using System.Linq;
using Chamber.Settings;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Chamber
{
    [StaticConstructorOnStartup]
    public class Building_Chamber : Building_CryptosleepCasket, IThingHolderWithDrawnPawn
    {

        private Graphic chamberTopGraphic;

        public float HeldPawnDrawPos_Y => this.DrawPos.y + 1f;

        public float HeldPawnBodyAngle => this.Rotation.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.Standing;

        public int ticksToFinish = -1;
        public bool conversionReady = false;
        public static int daysToFinish = SettingsRecord_Chamber.daysToFinish;
        public static float brainDamageChance = 0.6f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        protected override void Tick()
        {
            base.Tick();
            if (conversionReady || innerContainer.Count < 1)
            {
                return;
            }
            if (ticksToFinish > 0)
            {
                ticksToFinish--;
            }
            else
            {
                conversionReady = true;
                Pawn p = innerContainer.First() as Pawn;
                p.guest.Recruitable = true;
                string s = "Pawn " + p.Name + "IndoctrinationChamber_Ready".Translate();
                Messages.Message(s, new LookTargets(p), MessageTypeDefOf.NeutralEvent);
                p.guest.resistance = 0;
                if (Rand.Chance(brainDamageChance))
                {
                    string s2 = "Pawn " + p.Name + "IndoctrinationChamber_BrainDamage".Translate();
                    Messages.Message(s2, new LookTargets(p), MessageTypeDefOf.NegativeEvent);

                    float sev = this.def.GetModExtension<ChamberModExtension>().brainDamageSeverity;
                    Hediff firstHediffOfDef = HediffMaker.MakeHediff(DefOfs.IndoctrinationChamber_BrainDamage, p);
                    firstHediffOfDef.Severity = sev;
                    firstHediffOfDef.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
                    p.health.AddHediff(firstHediffOfDef, p.health.hediffSet.GetBrain());
                }
            }
        }

        public override void Open()
        {
            base.Open();
            conversionReady = false;
        }

        public override string GetInspectString()
        {
            if (innerContainer.Count < 1)
            {
                return "IndoctrinationChamber_Empty".Translate();
            }
            if (ticksToFinish < 1 && innerContainer.Count >= 1)
            {
                return "IndoctrinationChamber_Finished".Translate();
            }
            return "IndoctrinationChamber_Progress".Translate() + ": " + ticksToFinish.TicksToDays().ToString("F3") + " days left.";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToFinish, "ticksToFinish", -1);
            Scribe_Values.Look(ref conversionReady, "conversionReady", false);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (chamberTopGraphic == null)
            {
                chamberTopGraphic = this.def.GetModExtension<ChamberModExtension>().chamberTopGraphic.Graphic;
            }
            Vector3 pos = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
            pos = pos + new Vector3(0, 2, 0.5f);
            chamberTopGraphic.Draw(pos, Rot4.North, this);
            if (innerContainer.Count > 0)
            {
                Pawn p = innerContainer.First() as Pawn;
                Vector3 pos2 = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingBelowTop);
                pos2 += new Vector3(0, 2, -0.15f);
                p.Rotation = Rot4.South;
                p.Drawer.renderer.RenderPawnAt(pos2, Rot4.South, neverAimWeapon: true);
            }
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            Pawn p = thing as Pawn;
            if (p.IsColonist)
            {
                return false;
            }
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map));
                    Pawn pa = thing as Pawn;
                    float res = pa.guest.Resistance;
                    float multiplier = 1;
                    if (res < 5)
                    {
                        multiplier = 1f;
                    }
                    else if (res < 10)
                    {
                        multiplier = 1.5f;
                    }
                    else if (res < 15)
                    {
                        multiplier = 2f;
                    }
                    else
                    {
                        multiplier = 2.5f;
                    }
                    ticksToFinish = (int)(daysToFinish * 60000 * multiplier);
                }
                return true;
            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.IsQuestLodger())
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
                yield break;
            }
            if (innerContainer.Count != 0)
            {
                yield break;
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield break;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action cmd)
                {
                    if (cmd.defaultLabel == "CommandPodEject".Translate())
                        continue;

                    if (innerContainer.Count > 0)
                    {
                        Action originalAction = cmd.action;
                        cmd.action = delegate
                        {
                            Messages.Message("Ending indoctrination early could have catastrophic side effects.", MessageTypeDefOf.CautionInput, historical: false);
                            originalAction?.Invoke();
                        };
                    }
                }
                yield return gizmo;
            }
            if (base.Faction == Faction.OfPlayer && innerContainer.Count > 0 && def.building.isPlayerEjectable)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = delegate
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "Ending indoctrination early could have catastrophic side effects.",
                        EjectContents));
                };
                command_Action.defaultLabel = "Eject occupant";
                command_Action.defaultDesc = "Release from the indoctrination chamber. Could have catastrophic side effects.";
                if (innerContainer.Count == 0)
                {
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                yield return command_Action;
            }
            if (DebugSettings.godMode && innerContainer.Count > 0 && !conversionReady)
            {
                Command_Action devFinish = new Command_Action();
                devFinish.defaultLabel = "DEV: Finish now";
                devFinish.defaultDesc = "Immediately completes the indoctrination cycle.";
                devFinish.action = () => { ticksToFinish = 0; };
                devFinish.icon = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot", false);
                yield return devFinish;
            }
        }

        public override void EjectContents()
        {
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                if (item is Pawn pawn)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filth_Slime);
                    if (pawn.RaceProps.IsFlesh && Rand.Chance(0.2f))
                    {
                        pawn.health.AddHediff(DefOfs.GW_PersonaDisso);
                    }
                }
            }
            if (!base.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
            }
            base.EjectContents();
        }

        public static Building_CryptosleepCasket FindChamberFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.GetModExtension<ChamberModExtension>() != null))
            {
                Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.PositionHeld, p.MapHeld, ThingRequest.ForDef(item), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, (Thing x) => !((Building_CryptosleepCasket)x).HasAnyContents && traveler.MapHeld.reservationManager.CanReserve(traveler, x, 1, -1, null, ignoreOtherReservations));
                if (building_CryptosleepCasket != null)
                {
                    return building_CryptosleepCasket;
                }
            }
            return null;
        }
    }
}
