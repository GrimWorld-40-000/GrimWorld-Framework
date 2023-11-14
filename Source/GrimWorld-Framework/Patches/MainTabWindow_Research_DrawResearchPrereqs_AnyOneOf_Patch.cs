using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW_Frame
{
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawResearchPrereqs")]
    public static class MainTabWindow_Research_DrawResearchPrereqs_AnyOneOf_Patch
    {
        private static readonly Color FulfilledPrerequisiteColor = ColorLibrary.Green;

        private static readonly Color MissingPrerequisiteColor = ColorLibrary.RedReadable;

        [HarmonyPrefix]
        public static bool Prefix(MainTabWindow_Research __instance, ResearchProjectDef project, Rect rect, ref float __result)
        {
            bool flag = false;
            if (project.HasModExtension<DefModExtension_ExtraPrerequisiteActions>())
            {
                float xMin = rect.xMin;
                float yMin = rect.yMin;
                var modExtension = project.GetModExtension<DefModExtension_ExtraPrerequisiteActions>();
                if (!project.prerequisites.NullOrEmpty())
                {
                    Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":");
                    rect.yMin += rect.height;
                    rect.xMin += 6f;
                    for (int i = 0; i < project.prerequisites.Count; i++)
                    {
                        SetPrerequisiteStatusColor(project.prerequisites[i].IsFinished, project);
                        Widgets.LabelCacheHeight(ref rect, project.prerequisites[i].LabelCap);
                        rect.yMin += rect.height;
                    }
                    GUI.color = Color.white;
                    rect.xMin -= 6f;
                }
                if (!Find.World.GetComponent<WorldComponent_StudyManager>().CompletedAllRequirements(project))
                {
                    Widgets.LabelCacheHeight(ref rect, "GW_RequiresStudyOf".Translate());
                    rect.xMin += 6f;
                    rect.yMin += rect.height;

                    var stcManager = Find.World.GetComponent<WorldComponent_StudyManager>();
                    GUI.color = MissingPrerequisiteColor;
                    foreach (var req in modExtension.ItemStudyRequirements)
                    {
                        if (stcManager.CompletedRequirement(project, req.StudyObject))
                            GUI.color = FulfilledPrerequisiteColor;
                        else
                            GUI.color = MissingPrerequisiteColor;

                        string numRequired = "GW_MoreNeeded".Translate(req.NumberRequired);
                        string reqLabel = req.StudyObject.LabelCap;
                        string atCogitator;
                        if (modExtension.StudyLocation != null)
                        {
                            atCogitator = "GW_StudyAt".Translate(modExtension.StudyLocation.LabelCap);
                        }
                        else
                        {
                            atCogitator = "GW_StudyAt".Translate("nowhere. Please set StudyLocation.");
                        }
                        var labelPart1Size = Text.CalcSize(numRequired);
                        var stcFragmentsSize = Text.CalcSize(reqLabel);
                        var num = labelPart1Size.y;
                        rect.height = num;
                        Widgets.Label(rect, numRequired);
                        rect.x += labelPart1Size.x;
                        Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(req.StudyObject);
                        Widgets.ButtonText(rect, reqLabel, drawBackground: false, doMouseoverSound: false, active: false);
                        if (Widgets.ButtonInvisible(rect))
                        {
                            hyperlink.ActivateHyperlink();
                        }
                        rect.x += stcFragmentsSize.x;
                        Widgets.Label(rect, atCogitator);
                        rect.x -= stcFragmentsSize.x + labelPart1Size.x;

                        rect.yMin += rect.height;
                    }
                }
                else
                {
                    GUI.color = FulfilledPrerequisiteColor;
                    Widgets.LabelCacheHeight(ref rect, "GW_DiscoveredResearch".Translate());
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
                __result = rect.yMin - yMin;
                flag = true;
            }
            return !flag;
        }

        private static void SetPrerequisiteStatusColor(bool present, ResearchProjectDef project)
        {
            if (!project.IsFinished)
            {
                if (present)
                {
                    GUI.color = FulfilledPrerequisiteColor;
                }
                else
                {
                    GUI.color = MissingPrerequisiteColor;
                }
            }
        }
    }
}