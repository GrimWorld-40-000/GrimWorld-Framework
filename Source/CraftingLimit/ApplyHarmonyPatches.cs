using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GrimworldItemLimit
{
    [StaticConstructorOnStartup]
    public static class ApplyHarmonyPatches
    {
        static ApplyHarmonyPatches()
        {
            Harmony harmony = new Harmony("Grimworld.Framework.ItemLimiter");
            
            harmony.Patch(AccessTools.Method(typeof(Bill_Production), nameof(Bill_Production.ShouldDoNow)),
                postfix: new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostGetBillShouldDo)));
            harmony.Patch(AccessTools.Method(typeof(Bill_Production), nameof(Bill_Production.Notify_IterationCompleted)),
                postfix: new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostBillIterationCompleted)));
        }
        
        private static void PostGetBillShouldDo(ref bool __result, Bill_Production __instance)
        {
            if (!__result) return;
            if (!__instance.recipe.DoesRecipeHaveAnyProductThatIsAtLimit()) return;
            Messages.Message(new Message("GW_BillPausedBecauseOfCraftingLimit".Translate(), MessageTypeDefOf.RejectInput, __instance.billStack.billGiver as Thing));
            __instance.suspended = true;
            __result = false;
        }

        private static void PostBillIterationCompleted(Bill_Production __instance, Pawn billDoer, List<Thing> ingredients)
        {
            __instance.recipe.NotifyRecipeFinished();
        }
    }
}