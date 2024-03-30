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
            harmony.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "DrawBottomRow"),
                prefix: new HarmonyMethod(patchType, nameof(DrawBottomRowPreFix)));
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

        public static bool DrawBottomRowPreFix(MainTabWindow_Research __instance, Rect rect, ResearchProjectDef project, Color techprintColor, Color studiedColor)
        {
            if (!project.HasModExtension<DefModExtension_ExtraPrerequisiteActions>() || project.TechprintCount > 0 || project.RequiredAnalyzedThingCount > 0)
                return true;

            DefModExtension_ExtraPrerequisiteActions modExtension = project.GetModExtension<DefModExtension_ExtraPrerequisiteActions>();
            WorldComponent_StudyManager stcManager = Find.World.GetComponent<WorldComponent_StudyManager>();
            List<string> defsToCheckFor = new List<string> { "GW_STC_Fragment" };

            Color color = GUI.color;
            TextAnchor anchor = Text.Anchor;

            float num = rect.width / 2;

            Rect rect2 = rect;
            rect2.x = rect.x;
            rect2.width = num;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect2, project.CostApparent.ToString());
            rect2.x += num;
            foreach (StudyRequirement req in modExtension.ItemStudyRequirements)
            {
                if (defsToCheckFor.Contains(req.StudyObject.ToString()))
                {
                    string text;

                    if (stcManager.CompletedRequirement(project, req.StudyObject))
                        text = req.NumberRequired.ToString() + " / " + req.NumberRequired.ToString();
                    else
                        text = "0 / " + req.NumberRequired.ToString();

                    Vector2 vector = Text.CalcSize(text);
                    Rect rect3 = rect2;
                    rect3.xMin = rect2.xMax - vector.x - 10f;
                    Rect rect4 = rect2;
                    rect4.width = rect4.height;
                    rect4.x = rect3.x - rect4.width;
                    GUI.color = stcManager.CompletedRequirement(project, req.StudyObject) ? Color.green : ColorLibrary.RedReadable;
                    Widgets.Label(rect3, text);
                    GUI.color = Color.white;
                    if (req.StudyObject.ToString() == "GW_STC_Fragment")
                        GUI.DrawTexture(rect4.ContractedBy(3f), STCRequirementTex.Texture);
                    rect2.x += num;
                    GUI.color = color;
                    Text.Anchor = anchor;
                    return false;
                }
            }

            return true;
        }
    }
}
