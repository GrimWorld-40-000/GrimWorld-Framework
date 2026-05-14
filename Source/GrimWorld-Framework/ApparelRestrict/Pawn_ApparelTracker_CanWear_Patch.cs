using RimWorld;
using Verse;

namespace GW_Frame
{
    public static class Pawn_ApparelTracker_CanWear_Patch
    {
        public static void Postfix(ref bool __result, Pawn_ApparelTracker __instance, ThingDef apDef)
        {
            if (!__result || __instance?.pawn == null || apDef == null)
                return;

            if (!ApparelRestrictUtility.CanWearApparel(__instance.pawn, apDef, out _))
                __result = false;
        }
    }
}
