using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
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
    [HarmonyPatch(typeof(Building_CryptosleepCasket), "FindCryptosleepCasketFor")]
    public class CasketPostfix
    {
        [HarmonyPostfix]
        public static void fix(ref Building_CryptosleepCasket __result)
        {
            if (__result!=null)
            {
                if(__result.def.GetModExtension<ChamberModExtension>() != null)
                {
                    __result = null;
                }
            }
        }
    }


    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public class ChamberPostfix
    {
        [HarmonyPostfix]
        public static void fix(Pawn pawn, Vector3 clickPos, ref List<FloatMenuOption> opts)
        {
            
            foreach (LocalTargetInfo item17 in GenUI.TargetsAt(clickPos, TargetingParameters.ForCarryToBiosculpterPod(pawn), thingsOnly: true))
            {
                Pawn pawn3 = (Pawn)item17.Thing;
                if (pawn3.DestroyedOrNull() || pawn3.Dead || pawn3.IsColonyMech || pawn3.RaceProps?.IsMechanoid == true || pawn3.RaceProps?.IsFlesh == false || pawn3.RaceProps?.IsAnomalyEntity == true)
                {
                    continue;
                }
                if ((pawn3.guest != null && !pawn3.guest.Recruitable)&&(pawn3.IsPrisonerOfColony))
                {
                    if (!pawn.CanReserveAndReach(pawn3, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) 
                        || Building_Chamber.FindChamberFor(pawn3, pawn, ignoreOtherReservations: true) == null)
                    {
                        continue;
                    }
                    //gud
                    string text2 = "IndoctrinationChamber_CarryToJob".Translate(pawn3.LabelCap, pawn3.thingIDNumber);
                    JobDef jDef = DefOfs.CarryToIndoctrinatingChamberJob;
                    Action action2 = delegate
                    {
                        Building_CryptosleepCasket building_CryptosleepCasket = Building_Chamber.FindChamberFor(pawn3, pawn);
                        if (building_CryptosleepCasket == null)
                        {
                            building_CryptosleepCasket = Building_Chamber.FindChamberFor(pawn3, pawn, ignoreOtherReservations: true);
                        }
                        if (building_CryptosleepCasket == null)
                        {
                            Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), pawn3, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            Job job25 = JobMaker.MakeJob(jDef, pawn3, building_CryptosleepCasket);
                            job25.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job25, JobTag.Misc);
                        }
                    };
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, action2, MenuOptionPriority.Default, null, pawn3), pawn, pawn3));
                }
            }
        }
    }

}
