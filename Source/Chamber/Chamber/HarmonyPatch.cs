using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.AI;

namespace Chamber
{
    public class IndoctrinatingChamber_HarmonyPatches : Mod
    {
        public IndoctrinatingChamber_HarmonyPatches(ModContentPack content) : base(content)
        {
            new Harmony("Chamber").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Designator_Open), "DesignateThing")]
    public class DesignatorOpen_Chamber_Warning
    {
        [HarmonyPostfix]
        public static void Postfix(Thing t)
        {
            if (t is Building_Chamber chamber && chamber.HasAnyContents)
            {
                Messages.Message(
                    "Ending indoctrination early could have catastrophic side effects.",
                    chamber,
                    MessageTypeDefOf.CautionInput,
                    historical: false);
            }
        }
    }

    [HarmonyPatch(typeof(Building_CryptosleepCasket), "FindCryptosleepCasketFor")]
    public class CasketPostfix
    {
        [HarmonyPostfix]
        public static void fix(ref Building_CryptosleepCasket __result)
        {
            if (__result != null && __result.def.GetModExtension<ChamberModExtension>() != null)
            {
                __result = null;
            }
        }
    }

    // Replaces the 1.4/1.5 AddHumanlikeOrders Harmony patch.
    // RimWorld 1.6 auto-discovers FloatMenuOptionProvider subclasses via reflection.
    public class ChamberFloatMenuProvider : FloatMenuOptionProvider
    {
        protected override bool Drafted => false;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;
            if (pawn == null) yield break;

            foreach (Thing target in context.ClickedThings)
            {
                if (target is not Pawn pawn3) continue;
                if (pawn3.DestroyedOrNull() || pawn3.Dead) continue;
                if (pawn3.IsColonyMech || pawn3.RaceProps?.IsMechanoid == true) continue;
                if (pawn3.RaceProps?.IsFlesh == false) continue;
                if (pawn3.RaceProps?.IsAnomalyEntity == true) continue;
                if (pawn3.guest == null || pawn3.guest.Recruitable || !pawn3.IsPrisonerOfColony) continue;

                if (!pawn.CanReserveAndReach(pawn3, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true)
                    || Building_Chamber.FindChamberFor(pawn3, pawn, ignoreOtherReservations: true) == null)
                {
                    continue;
                }

                string label = "IndoctrinationChamber_CarryToJob".Translate(pawn3.LabelCap, pawn3.thingIDNumber);
                JobDef jDef = DefOfs.CarryToIndoctrinatingChamberJob;
                Pawn pawn3Captured = pawn3;
                Action action = delegate
                {
                    Building_CryptosleepCasket chamber = Building_Chamber.FindChamberFor(pawn3Captured, pawn)
                        ?? Building_Chamber.FindChamberFor(pawn3Captured, pawn, ignoreOtherReservations: true);
                    if (chamber == null)
                    {
                        Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), pawn3Captured, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        Job job = JobMaker.MakeJob(jDef, pawn3Captured, chamber);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                };

                yield return FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(label, action, MenuOptionPriority.Default, null, pawn3),
                    pawn, pawn3);
            }
        }
    }
}
