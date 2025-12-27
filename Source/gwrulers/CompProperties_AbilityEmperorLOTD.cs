using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace gwrulers
{
    public class CompProperties_AbilityEmperorLOTD : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEmperorLOTD()
        {
            compClass = typeof(CompAbilityEffect_EmperorLOTD);
        }

        public float psychicSensitivityExtraSummmonFactor = 0.5f;

        //public string factionDef = "Empire";

        public List<PawnKindDef_Percentage> pawnKindDefs;
    }
}
