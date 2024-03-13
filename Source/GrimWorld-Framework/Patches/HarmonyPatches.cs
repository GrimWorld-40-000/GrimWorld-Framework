using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW_Frame
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        private static readonly Color FulfilledPrerequisiteColor = ColorLibrary.Green;

        private static readonly Color MissingPrerequisiteColor = ColorLibrary.RedReadable;

        // Will need cached textures for each requirement, and should check for it later
        private static readonly CachedTexture STCRequirementTex = new CachedTexture("UI/ImperiumStudy");


        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Rimworld.Grimworld.Framework.main");
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }),
                postfix: new HarmonyMethod(patchType, nameof(CanEquipPostfix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "DrawResearchPrerequisites"),
                prefix: new HarmonyMethod(patchType, nameof(DrawResearchPrerequisitesPrefix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.PrerequisitesCompleted)),
                postfix: new HarmonyMethod(patchType, nameof(PrerequisitesCompletedPostFix)));
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "ListProjects"),
                postfix: new HarmonyMethod(patchType, nameof(ResearchDrawRightRectPostFix)));
        }

        public static void CanEquipPostfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            EquipRestrictExtension extension = thing.def.GetModExtension<EquipRestrictExtension>();
            if (extension != null && __result)
            {       // Attempt to get the various limiting lists
                List<GeneDef> requiredGenesToEquip = extension.requiredGenesToEquip;
                List<GeneDef> requireOneOfGenesToEquip = extension.requireOneOfGenesToEquip;
                List<GeneDef> forbiddenGenesToEquip = extension.forbiddenGenesToEquip;
                List<XenotypeDef> requireOneOfXenotypeToEquip = extension.requireOneOfXenotypeToEquip;
                List<XenotypeDef> forbiddenXenotypesToEquip = extension.forbiddenXenotypesToEquip;
                List<HediffDef> requiredHediffsToEquip = extension.requiredHediffsToEquip;
                List<HediffDef> requireOneOfHediffsToEquip = extension.requireOneOfHediffsToEquip;
                List<HediffDef> forbiddenHediffsToEquip = extension.forbiddenHediffsToEquip;
                // Gene Check
                if (!pawn.genes.GenesListForReading.NullOrEmpty())
                {
                    Pawn_GeneTracker currentGenes = pawn.genes;
                    if (!requiredGenesToEquip.NullOrEmpty() || !requireOneOfGenesToEquip.NullOrEmpty() || !forbiddenGenesToEquip.NullOrEmpty() ||
                        !requireOneOfXenotypeToEquip.NullOrEmpty() || !forbiddenXenotypesToEquip.NullOrEmpty())
                    {
                        bool flag = true;
                        if (!requireOneOfXenotypeToEquip.NullOrEmpty() && !requireOneOfXenotypeToEquip.Contains(pawn.genes.Xenotype) && flag)
                        {
                            if (requireOneOfXenotypeToEquip.Count > 1) cantReason = "GW_XenoRestrictedEquipment_AnyOne".Translate();
                            else cantReason = "GW_XenoRestrictedEquipment_One".Translate(requireOneOfXenotypeToEquip[0].label);
                            flag = false;
                        }
                        if (!forbiddenXenotypesToEquip.NullOrEmpty() && forbiddenXenotypesToEquip.Contains(pawn.genes.Xenotype) && flag)
                        {
                            cantReason = "GW_XenoRestrictedEquipment_None".Translate(pawn.genes.Xenotype.label);
                            flag = false;
                        }
                        if (!forbiddenGenesToEquip.NullOrEmpty() && flag)
                        {
                            foreach (Gene gene in currentGenes.GenesListForReading)
                            {
                                if (forbiddenGenesToEquip.Contains(gene.def))
                                {
                                    cantReason = "GW_GeneRestrictedEquipment_None".Translate(gene.def.label);
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (!requiredGenesToEquip.NullOrEmpty() && flag)
                        {
                            foreach (Gene gene in currentGenes.GenesListForReading)
                            {
                                if (requiredGenesToEquip.Contains(gene.def)) requiredGenesToEquip.Remove(gene.def);
                            }
                            if (!requiredGenesToEquip.NullOrEmpty())
                            {
                                if (extension.requiredGenesToEquip.Count > 1) cantReason = "GW_GeneRestrictedEquipment_All".Translate();
                                else cantReason = "GW_GeneRestrictedEquipment_One".Translate(extension.requiredGenesToEquip[0].label);
                                flag = false;
                            }
                        }
                        if (!requireOneOfGenesToEquip.NullOrEmpty() && flag)
                        {
                            flag = false;
                            if (requireOneOfGenesToEquip.Count > 1) cantReason = "GW_GeneRestrictedEquipment_AnyOne".Translate();
                            else cantReason = "GW_GeneRestrictedEquipment_One".Translate(requireOneOfGenesToEquip[0].label);
                            foreach (Gene gene in currentGenes.GenesListForReading)
                            {
                                if (requiredGenesToEquip.Contains(gene.def))
                                {
                                    flag = true;
                                    cantReason = null;
                                    break;
                                }
                            }
                        }
                        __result = flag;
                    }
                }
                else
                {
                    if (!requiredGenesToEquip.NullOrEmpty() || !requireOneOfGenesToEquip.NullOrEmpty() || !requireOneOfXenotypeToEquip.NullOrEmpty())
                    {
                        cantReason = "GW_GenesNotFound".Translate();
                        __result = false;
                    }
                }

                // Hediff Check
                HediffSet hediffSet = pawn.health.hediffSet;
                if (__result && !hediffSet.hediffs.NullOrEmpty())
                {
                    if (!requiredHediffsToEquip.NullOrEmpty() || !requireOneOfHediffsToEquip.NullOrEmpty() || !forbiddenHediffsToEquip.NullOrEmpty())
                    {
                        bool flag = true;
                        if (!forbiddenHediffsToEquip.NullOrEmpty())
                        {
                            foreach (HediffDef hediffDef in forbiddenHediffsToEquip)
                            {
                                if (hediffSet.HasHediff(hediffDef))
                                {
                                    cantReason = "GW_HediffRestrictedEquipment_None".Translate(hediffDef.label);
                                    flag = false;
                                    break;
                                }
                            }
                        }

                        if (flag && !requireOneOfHediffsToEquip.NullOrEmpty())
                        {
                            flag = false;
                            foreach (HediffDef hediffDef in requireOneOfHediffsToEquip)
                            {
                                if (hediffSet.HasHediff(hediffDef))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                if (requireOneOfHediffsToEquip.Count > 1) cantReason = "GW_HediffRestrictedEquipment_AnyOne".Translate();
                                else cantReason = "GW_HediffRestrictedEquipment_One".Translate(requireOneOfHediffsToEquip[0].label);
                            }
                        }

                        if (flag && !requiredHediffsToEquip.NullOrEmpty())
                        {
                            foreach (Hediff hediff in hediffSet.hediffs)
                            {
                                if (requiredHediffsToEquip.Contains(hediff.def)) requiredHediffsToEquip.Remove(hediff.def);
                            }
                            if (!requiredHediffsToEquip.NullOrEmpty())
                            {
                                if (extension.requiredHediffsToEquip.Count > 1) cantReason = "GW_HediffRestrictedEquipment_All".Translate();
                                else "GW_HediffRestrictedEquipment_One".Translate(extension.requiredHediffsToEquip[0].label);
                                flag = false;
                            }
                        }

                        __result = flag;
                    }
                }
            }
        }

        public static bool DrawResearchPrerequisitesPrefix(MainTabWindow_Research __instance, Rect rect, ref float y, ResearchProjectDef project)
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
                y = rect.yMin - yMin;
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

        public static void PrerequisitesCompletedPostFix(ref bool __result, ResearchProjectDef __instance)
        {
            if (__result && __instance.HasModExtension<DefModExtension_ExtraPrerequisiteActions>())
                __result = Find.World.GetComponent<WorldComponent_StudyManager>().CompletedAllRequirements(__instance);
        }

        public static void ResearchDrawRightRectPostFix(MainTabWindow_Research __instance, Rect rightInRect, ref ResearchTabDef ___curTabInt, ref ScrollPositioner ___scrollPositioner,
            ref Vector2 ___rightScrollPosition, ref QuickSearchWidget ___quickSearchWidget, ref HashSet<ResearchProjectDef> ___matchingProjects, ref ResearchProjectDef ___selectedProject)
        {
            List<ResearchProjectDef> visibleResearchProjects = __instance.VisibleResearchProjects;
            rightInRect.yMin += 3f;
            Rect outRect = rightInRect.ContractedBy(10f);
            Rect rect2 = new Rect(0f, 0f, rightInRect.width, rightInRect.height);
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
