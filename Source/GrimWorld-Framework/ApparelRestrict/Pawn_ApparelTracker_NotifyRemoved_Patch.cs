using RimWorld;
using Verse;

namespace GW_Frame
{
    public static class Pawn_ApparelTracker_NotifyRemoved_Patch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (!ApparelRestrictUtility.IsRestrictionEnabled)
                return;
            if (__instance?.pawn == null || apparel?.def == null)
                return;

            if (ApparelRestrictUtility.IsFibrovest(apparel.def))
            {
                ApparelRestrictUtility.DropFullPowerArmorKit(__instance);
                return;
            }

            if (apparel.def.apparel != null && ApparelRestrictUtility.IsPowerArmorTorso(apparel.def, apparel.def.apparel))
                ApparelRestrictUtility.DropPowerArmorAttachments(__instance);
        }
    }
}
