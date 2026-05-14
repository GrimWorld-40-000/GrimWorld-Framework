using System.Collections.Generic;
using System.Linq;
using GW_Frame.Settings;
using RimWorld;
using Verse;

namespace GW_Frame
{
    public static class ApparelRestrictUtility
    {
        public const string PatchKey = "GWPowerArmorRestrict";

        private static readonly HashSet<string> BroadGwSetTags = new HashSet<string>
        {
            "GW_Custodes",
            "GW_Champion",
        };

        private static readonly HashSet<string> ExcludedTradeTags = new HashSet<string>
        {
            "GW_AM_Helmet",
            "GW_AM_Backpacks",
            "GW_HOI_Armor",
        };

        private static readonly HashSet<string> ExcludedApparelTags = new HashSet<string>
        {
            "GW_AM_Backpack",
            "GW_CadianBackpack",
            "GW_HOI_Armor",
            "40kCadianGuard",
            "40kCadianHelmet",
            "40kKarskinBackpack",
            "40kEngineerBackpack",
        };

        private static readonly string[] DefaultBypassGeneDefNames =
        {
            "Gene_placeholderA",
            "Gene_placeholderB",
        };

        private static readonly string[] ExcludedApparelTagPrefixes =
        {
            "40kCadian",
            "40kKarskin",
        };

        private static List<GeneDef> cachedDefaultBypassGenes;

        private static ApparelLayerDef shoulderLayer;
        private static ApparelLayerDef backpackLayer;
        private static ApparelLayerDef shellLayer;
        private static ApparelLayerDef middleLayer;
        private static ApparelLayerDef overheadLayer;
        private static BodyPartGroupDef torsoGroup;
        private static BodyPartGroupDef fullHeadGroup;
        private static ThingCategoryDef gwBackpacksCategory;

        private static ApparelLayerDef ShoulderLayer =>
            shoulderLayer ??= DefDatabase<ApparelLayerDef>.GetNamedSilentFail("GW_Shoulder");

        private static ApparelLayerDef BackpackLayer =>
            backpackLayer ??= DefDatabase<ApparelLayerDef>.GetNamedSilentFail("Backpack");

        private static ApparelLayerDef ShellLayer =>
            shellLayer ??= ApparelLayerDefOf.Shell;

        private static ApparelLayerDef MiddleLayer =>
            middleLayer ??= ApparelLayerDefOf.Middle;

        private static ApparelLayerDef OverheadLayer =>
            overheadLayer ??= ApparelLayerDefOf.Overhead;

        private static BodyPartGroupDef TorsoGroup =>
            torsoGroup ??= DefDatabase<BodyPartGroupDef>.GetNamedSilentFail("Torso");

        private static BodyPartGroupDef FullHeadGroup =>
            fullHeadGroup ??= DefDatabase<BodyPartGroupDef>.GetNamedSilentFail("FullHead");

        private static ThingCategoryDef GwBackpacksCategory =>
            gwBackpacksCategory ??= DefDatabase<ThingCategoryDef>.GetNamedSilentFail("GW_Backpacks");

        public static bool IsRestrictionEnabled
        {
            get
            {
                SettingsRecord_Patches settings = SettingsTabRecord_Patches.SettingsRecord;
                if (settings == null)
                    return SettingsRecord_Patches.DefaultEnabledForKey(PatchKey);
                return settings.Get(PatchKey);
            }
        }

        public static bool IsExcludedFromPowerArmorRestriction(ThingDef def)
        {
            if (def == null)
                return true;

            ApparelRestrictExtension ext = def.GetModExtension<ApparelRestrictExtension>();
            if (ext?.exemptFromPowerArmorRestriction == true)
                return true;

            if (def.defName != null && def.defName.StartsWith("GW_HOI_"))
                return true;

            if (def.tradeTags != null)
            {
                foreach (string tag in def.tradeTags)
                {
                    if (ExcludedTradeTags.Contains(tag))
                        return true;
                }
            }

            List<string> apparelTags = def.apparel?.tags;
            if (apparelTags != null)
            {
                foreach (string tag in apparelTags)
                {
                    if (tag == null)
                        continue;
                    if (ExcludedApparelTags.Contains(tag))
                        return true;
                    foreach (string prefix in ExcludedApparelTagPrefixes)
                    {
                        if (tag.StartsWith(prefix))
                            return true;
                    }
                }
            }

            return false;
        }

        public static bool IsShoulderAttachment(ApparelProperties ap)
        {
            if (ap?.layers == null || ShoulderLayer == null)
                return false;
            return ap.layers.Contains(ShoulderLayer);
        }

        public static bool IsShoulderAttachment(ThingDef def, ApparelProperties ap) =>
            !IsExcludedFromPowerArmorRestriction(def) && IsShoulderAttachment(ap);

        public static bool IsPowerBackpack(ThingDef def, ApparelProperties ap)
        {
            if (IsExcludedFromPowerArmorRestriction(def))
                return false;
            if (def == null || ap?.layers == null || BackpackLayer == null)
                return false;
            if (!ap.layers.Contains(BackpackLayer))
                return false;
            if (GwBackpacksCategory != null && def.IsWithinCategory(GwBackpacksCategory))
                return true;
            return def.tradeTags != null && def.tradeTags.Contains("GWBackpacks");
        }

        public static bool IsPowerHelmet(ThingDef def, ApparelProperties ap)
        {
            if (IsExcludedFromPowerArmorRestriction(def))
                return false;
            if (def == null || ap?.layers == null || ap.bodyPartGroups == null)
                return false;
            if (OverheadLayer == null || FullHeadGroup == null)
                return false;
            if (!ap.layers.Contains(OverheadLayer) || !ap.bodyPartGroups.Contains(FullHeadGroup))
                return false;

            if (GetSpecificGwSetTags(def).Count > 0)
                return true;

            return HasApparelTags(def, "Spacer", "EVA");
        }

        public static bool IsPowerArmorAttachment(ThingDef def)
        {
            if (def?.apparel == null)
                return false;
            ApparelProperties ap = def.apparel;
            return IsShoulderAttachment(def, ap)
                || IsPowerBackpack(def, ap)
                || IsPowerHelmet(def, ap);
        }

        public static bool IsPowerArmorTorso(ThingDef def, ApparelProperties ap) =>
            def != null && IsPowerArmorTorso(ap) && !IsExcludedFromPowerArmorRestriction(def);

        public static bool IsPowerArmorTorso(ApparelProperties ap)
        {
            if (ap?.layers == null || ap.bodyPartGroups == null || TorsoGroup == null)
                return false;
            if (!ap.bodyPartGroups.Contains(TorsoGroup))
                return false;
            return ap.layers.Contains(ShellLayer) && ap.layers.Contains(MiddleLayer);
        }

        public static bool PawnHasPowerArmorTorso(Pawn pawn) =>
            GetWornPowerArmorTorsoPieces(pawn).Any();

        public static IEnumerable<Apparel> GetWornPowerArmorTorsoPieces(Pawn pawn)
        {
            if (pawn?.apparel?.WornApparel == null)
                yield break;
            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (worn?.def != null && IsPowerArmorTorso(worn.def.apparel))
                    yield return worn;
            }
        }

        public static int GetArmorTier(ThingDef def) =>
            def?.GetModExtension<ApparelRestrictExtension>()?.armorTier ?? 0;

        public static bool IsFibrovest(ThingDef def)
        {
            if (def == null)
                return false;
            if (def.defName == "GW_SM_FibroVest")
                return true;
            return HasApparelTag(def, "GW_SM_FibroVest");
        }

        public static bool PawnWearingFibrovest(Pawn pawn)
        {
            if (pawn?.apparel?.WornApparel == null)
                return false;
            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (IsFibrovest(worn?.def))
                    return true;
            }
            return false;
        }

        public static bool PawnMeetsFibrovestOrGeneRequirement(Pawn pawn, ApparelRestrictExtension ext)
        {
            if (PawnWearingFibrovest(pawn))
                return true;
            return PawnHasBypassGeneForPowerArmor(pawn, ext);
        }

        public static bool PawnHasBypassGeneForPowerArmor(Pawn pawn, ApparelRestrictExtension ext)
        {
            if (pawn?.genes == null)
                return false;
            foreach (GeneDef gene in GetBypassGenes(ext))
            {
                if (gene != null && pawn.genes.HasActiveGene(gene))
                    return true;
            }
            return false;
        }

        public static IEnumerable<GeneDef> GetBypassGenes(ApparelRestrictExtension ext)
        {
            if (ext?.bypassGenesForFibrovestRequirement != null && ext.bypassGenesForFibrovestRequirement.Count > 0)
            {
                foreach (GeneDef gene in ext.bypassGenesForFibrovestRequirement)
                {
                    if (gene != null)
                        yield return gene;
                }
                yield break;
            }

            foreach (GeneDef gene in GetDefaultBypassGenes())
                yield return gene;
        }

        private static List<GeneDef> GetDefaultBypassGenes()
        {
            if (cachedDefaultBypassGenes != null)
                return cachedDefaultBypassGenes;

            cachedDefaultBypassGenes = new List<GeneDef>();
            foreach (string defName in DefaultBypassGeneDefNames)
            {
                GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
                if (gene != null)
                    cachedDefaultBypassGenes.Add(gene);
            }
            return cachedDefaultBypassGenes;
        }

        public static void DropPowerArmorAttachments(Pawn_ApparelTracker tracker)
        {
            if (tracker?.pawn == null)
                return;

            List<Apparel> toDrop = null;
            foreach (Apparel worn in tracker.WornApparel)
            {
                if (worn?.def == null)
                    continue;
                if (!IsPowerArmorAttachment(worn.def))
                    continue;
                toDrop ??= new List<Apparel>();
                toDrop.Add(worn);
            }

            if (toDrop == null)
                return;

            foreach (Apparel piece in toDrop)
                TryDropIfWorn(tracker, piece);
        }

        public static void DropFullPowerArmorKit(Pawn_ApparelTracker tracker)
        {
            if (tracker?.pawn == null)
                return;

            List<Apparel> toDrop = null;
            foreach (Apparel worn in tracker.WornApparel)
            {
                if (worn?.def?.apparel == null)
                    continue;
                if (!IsPowerArmorAttachment(worn.def)
                    && !IsPowerArmorTorso(worn.def, worn.def.apparel))
                    continue;
                toDrop ??= new List<Apparel>();
                toDrop.Add(worn);
            }

            if (toDrop == null)
                return;

            // Torso removal triggers attachment cascade; strip attachments first.
            toDrop.Sort((a, b) =>
            {
                bool aTorso = IsPowerArmorTorso(a.def, a.def.apparel);
                bool bTorso = IsPowerArmorTorso(b.def, b.def.apparel);
                return aTorso.CompareTo(bTorso);
            });

            foreach (Apparel piece in toDrop)
                TryDropIfWorn(tracker, piece);
        }

        private static void TryDropIfWorn(Pawn_ApparelTracker tracker, Apparel piece)
        {
            if (piece == null || tracker?.WornApparel == null)
                return;
            if (!tracker.WornApparel.Contains(piece))
                return;
            tracker.TryDrop(piece);
        }

        public static bool CanWearApparel(Pawn pawn, ThingDef def, out string reason)
        {
            reason = null;
            if (!IsRestrictionEnabled)
                return true;
            if (pawn == null || def == null || !def.IsApparel)
                return true;

            ApparelProperties ap = def.apparel;
            ApparelRestrictExtension ext = def.GetModExtension<ApparelRestrictExtension>();

            if (IsPowerArmorTorso(def, ap) && ext?.exemptFromFibrovestRequirement != true)
            {
                if (!PawnMeetsFibrovestOrGeneRequirement(pawn, ext))
                {
                    reason = "GW_PowerArmorRequiresFibrovestOrGene".Translate();
                    return false;
                }
            }

            bool isShoulder = IsShoulderAttachment(def, ap);
            bool isBackpack = IsPowerBackpack(def, ap);
            bool isHelmet = IsPowerHelmet(def, ap);

            if (!isShoulder && !isBackpack && !isHelmet && ext == null)
                return true;

            if (IsExcludedFromPowerArmorRestriction(def) && ext == null)
                return true;

            bool requiresTorso = (isShoulder && (ext == null || ext.requiresPowerArmorTorso))
                || isBackpack
                || isHelmet
                || (ext != null && ext.requiresPowerArmorTorso);

            if (!requiresTorso)
                return true;

            List<Apparel> torsoPieces = GetWornPowerArmorTorsoPieces(pawn).ToList();
            if (torsoPieces.Count == 0)
            {
                reason = "GW_PowerArmorAttachmentRequiresTorso".Translate();
                return false;
            }

            if ((isHelmet || isBackpack)
                && !torsoPieces.Any(t => HasMatchingSetTag(def, t.def)))
            {
                reason = "GW_PowerArmorAttachmentRequiresMatchingSet".Translate();
                return false;
            }

            if (ext == null)
                return true;

            if (!ext.requiredWornApparelTagsAny.NullOrEmpty()
                && !torsoPieces.Any(t => HasAnyTag(t.def, ext.requiredWornApparelTagsAny)))
            {
                reason = "GW_ShoulderRequiresPowerArmorTags".Translate();
                return false;
            }

            if (!string.IsNullOrEmpty(ext.apparelSetId)
                && !torsoPieces.Any(t => MatchesApparelSet(t.def, ext.apparelSetId)))
            {
                reason = "GW_ShoulderRequiresMatchingArmorSet".Translate();
                return false;
            }

            if (ext.requiredMinArmorTier > 0
                && !torsoPieces.Any(t => GetArmorTier(t.def) >= ext.requiredMinArmorTier))
            {
                reason = "GW_ShoulderRequiresPowerArmorTier".Translate(ext.requiredMinArmorTier);
                return false;
            }

            return true;
        }

        public static bool HasMatchingSetTag(ThingDef attachment, ThingDef torso)
        {
            List<string> attachmentTags = GetSpecificGwSetTags(attachment);
            if (attachmentTags.Count == 0)
                return true;

            List<string> torsoTags = GetSpecificGwSetTags(torso);
            foreach (string tag in attachmentTags)
            {
                if (torsoTags.Contains(tag))
                    return true;
            }
            return false;
        }

        private static List<string> GetSpecificGwSetTags(ThingDef def)
        {
            List<string> result = new List<string>();
            List<string> tags = def?.apparel?.tags;
            if (tags == null)
                return result;
            foreach (string tag in tags)
            {
                if (tag == null || !tag.StartsWith("GW_") || BroadGwSetTags.Contains(tag))
                    continue;
                result.Add(tag);
            }
            return result;
        }

        private static bool HasApparelTags(ThingDef def, params string[] required)
        {
            List<string> tags = def?.apparel?.tags;
            if (tags == null)
                return false;
            foreach (string req in required)
            {
                if (!tags.Contains(req))
                    return false;
            }
            return true;
        }

        private static bool HasApparelTag(ThingDef def, string tag) =>
            def?.apparel?.tags != null && def.apparel.tags.Contains(tag);

        private static bool HasAnyTag(ThingDef def, List<string> tags)
        {
            List<string> apparelTags = def?.apparel?.tags;
            if (apparelTags == null)
                return false;
            foreach (string tag in tags)
            {
                if (apparelTags.Contains(tag))
                    return true;
            }
            return false;
        }

        private static bool MatchesApparelSet(ThingDef def, string setId)
        {
            string wornSetId = def?.GetModExtension<ApparelRestrictExtension>()?.apparelSetId;
            return !string.IsNullOrEmpty(wornSetId) && wornSetId == setId;
        }
    }
}
