using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GW4KArmor.Patches;

public static class InjectPatches
{
    //Inject Painting-Tool Gizmo on items with PaintableThingExtension
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Pawn_GetGizmosPatch
    {
        private static List<ThingWithComps> tempList = new();
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            tempList.Clear();
            if (__instance.equipment != null && __instance.apparel != null)
            {
                tempList.AddRange(__instance.equipment.AllEquipmentListForReading);
                tempList.AddRange(__instance.apparel.WornApparel);
                var comps = tempList.Select(a => a.GetComp<Comp_TriColorMask>()).Where(c => c != null)?.ToList();
                if (comps == null) return;
                if (comps.Count < 1) return;
                var firstComp = comps.First();
                if (comps.Count == 1)
                {
                    var gizmo = firstComp.PaintGizmo;
                    __result = __result.Append(gizmo);
                }

                if (comps.Count > 1)
                {
                    var gizmo = firstComp.PaintGizmoMulti;
                    gizmo.pawn = __instance;
                    __result = __result.Append(gizmo);
                }
            }
        }
    }
}