using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GW_Frame
{
    public class ApparelRestrictExtension : DefModExtension
    {
        public bool requiresPowerArmorTorso = true;

        public bool exemptFromPowerArmorRestriction;

        public bool exemptFromFibrovestRequirement;

        public List<string> requiredWornApparelTagsAny;

        public List<string> forbiddenUnlessWornApparelTagsAny;

        public int requiredMinArmorTier;

        public int armorTier;

        public string apparelSetId;

        /// <summary>Genes that satisfy fibrovest requirement for this def when non-empty; otherwise framework defaults apply.</summary>
        public List<GeneDef> bypassGenesForFibrovestRequirement;
    }
}
