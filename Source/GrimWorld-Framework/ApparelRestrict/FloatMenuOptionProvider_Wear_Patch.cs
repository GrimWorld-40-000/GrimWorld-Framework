using RimWorld;
using Verse;

namespace GW_Frame
{
    public static class FloatMenuOptionProvider_Wear_Patch
    {
        public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
        {
            if (__result == null || __result.action == null || clickedThing?.def == null || context?.FirstSelectedPawn == null)
                return;
            if (!clickedThing.def.IsApparel)
                return;

            Pawn pawn = context.FirstSelectedPawn;
            if (ApparelRestrictUtility.CanWearApparel(pawn, clickedThing.def, out string reason))
                return;

            if (reason.NullOrEmpty())
                reason = "GW_ShoulderRequiresPowerArmor".Translate();
            __result = new FloatMenuOption(
                "GW_CannotWearApparel".Translate(clickedThing.LabelShort, reason),
                null);
        }
    }
}
