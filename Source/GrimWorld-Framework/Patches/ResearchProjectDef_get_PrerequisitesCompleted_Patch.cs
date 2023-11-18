using HarmonyLib;
using Verse;

namespace GW_Frame
{
    [HarmonyPatch(typeof(ResearchProjectDef), "get_PrerequisitesCompleted")]
    public static class ResearchProjectDef_get_PrerequisitesCompleted_Patch
    {
        public static bool Postfix(bool __result, ResearchProjectDef __instance)
        {
            if (__result && __instance.HasModExtension<DefModExtension_ExtraPrerequisiteActions>())
                return __result && Find.World.GetComponent<WorldComponent_StudyManager>().CompletedAllRequirements(__instance);
            return __result;
        }
    }
}