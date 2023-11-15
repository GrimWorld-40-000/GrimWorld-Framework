using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using HarmonyLib;

namespace GW_Frame
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawRightRect")]
    public static class MainTabWindow_Research_DrawRightRect_AnyOneOf_Patch
    {
        // Will need cached textures for each requirement, and should check for it later
        private static readonly CachedTexture STCRequirementTex = new CachedTexture("UI/ImperiumStudy");
        // Add Logs

        [HarmonyPostfix]
        public static void Postfix(MainTabWindow_Research __instance, Rect rightOutRect, ref ResearchTabDef ___curTabInt, ref ScrollPositioner ___scrollPositioner,
            ref Vector2 ___rightScrollPosition, ref float ___rightViewWidth, ref float ___rightViewHeight, ref QuickSearchWidget ___quickSearchWidget,
            ref HashSet<ResearchProjectDef> ___matchingProjects, ref ResearchProjectDef ___selectedProject)
        {
            List<ResearchProjectDef> visibleResearchProjects = __instance.VisibleResearchProjects;
            rightOutRect.yMin += 3f;
            Rect outRect = rightOutRect.ContractedBy(10f);
            Rect rect2 = new Rect(0f, 0f, ___rightViewWidth, ___rightViewHeight);
            rect2.ContractedBy(10f);
            rect2.width = ___rightViewWidth;
            Rect rect3 = rect2.ContractedBy(10f);
            ___scrollPositioner.ClearInterestRects();
            Widgets.ScrollHorizontal(outRect, ref ___rightScrollPosition, rect2);
            Widgets.BeginScrollView(outRect, ref ___rightScrollPosition, rect2);
            Widgets.BeginGroup(rect3);
            for (int l = 0; l < visibleResearchProjects.Count; l++)
            {
                ResearchProjectDef researchProjectDef3 = visibleResearchProjects[l];
                // Check if the project is even on screen and has the extension
                if (researchProjectDef3.tab != ___curTabInt || !researchProjectDef3.HasModExtension<DefModExtension_ExtraPrerequisiteActions>())
                {
                    continue;
                }
                var modExtension = researchProjectDef3.GetModExtension<DefModExtension_ExtraPrerequisiteActions>();
                var stcManager = Find.World.GetComponent<WorldComponent_StudyManager>();

                // Gives a list of all study items that should be recorded on the main page. Ideally the only ones listed will be non-standard resources
                List<string> defsToCheckFor = new List<string>() { "GW_STC_Fragment" };

                Rect rect4 = new Rect(researchProjectDef3.ResearchViewX * 190f, researchProjectDef3.ResearchViewY * 100f, 140f, 50f);
                Rect rect5 = new Rect(rect4);
                bool flag3 = ___quickSearchWidget.filter.Active && ___matchingProjects.Contains(researchProjectDef3);
                Rect rect6 = rect5;
                rect6.y = rect5.y + rect5.height - rect6.height;
                Rect rect7 = rect6;
                rect7.x += 10f;
                rect7.width = rect7.width / 2f - 10f;
                Color color6 = GUI.color;
                TextAnchor anchor = Text.Anchor;
                GUI.color = Widgets.NormalOptionColor;
                float num = rect4.xMax;
                if (researchProjectDef3.TechprintCount == 0 && researchProjectDef3.RequiredStudiedThingCount == 0)
                {
                    foreach (StudyRequirement req in modExtension.ItemStudyRequirements)
                    {
                        if (defsToCheckFor.Contains(req.StudyObject.ToString()))
                        {
                            string text2;
                            if (stcManager.CompletedRequirement(researchProjectDef3, req.StudyObject))
                            {
                                text2 = req.NumberRequired.ToString() + " / " + req.NumberRequired.ToString();
                            }
                            else
                            {
                                text2 = "0 / " + req.NumberRequired.ToString();
                            }
                            Vector2 vector2 = Text.CalcSize(text2);
                            num -= vector2.x + 10f; 

                            // The first half attempts to automatically fix the display issue, but the second half allows for ensuring it's fixed using the extension
                            if (researchProjectDef3.LabelCap.Length > 22 || modExtension.longLabel == true) rect7.y += 16f;

                            Rect rect9 = new Rect(num, rect7.y, vector2.x, rect7.height);
                            GUI.color = (stcManager.CompletedRequirement(researchProjectDef3, req.StudyObject) ? Color.green : ColorLibrary.RedReadable);
                            Text.Anchor = TextAnchor.MiddleRight;
                            Widgets.Label(rect9, text2);
                            num -= rect7.height - 10f;
                            GUI.color = Color.white;

                                // Ties the specific items to its relevant image when applicable
                            if (req.StudyObject.ToString() == "GW_STC_Fragment")
                            {
                                GUI.DrawTexture(new Rect(num, rect7.y, rect7.height, rect7.height).ContractedBy(12f), STCRequirementTex.Texture);
                            }
                            GUI.color = color6;
                            break;
                        }
                    }
                }
                Text.Anchor = anchor;
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
        }
    }
}