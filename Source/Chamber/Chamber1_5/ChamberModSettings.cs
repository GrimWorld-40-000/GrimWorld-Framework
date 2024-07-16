using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
namespace Chamber
{
    public class ChamberModSettings : ModSettings
    {
        public static float brainDamageChance;
        public static int daysToFinish;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref brainDamageChance, "brainDamageChance",0.6f,true);
            Scribe_Values.Look<int>(ref daysToFinish, "daysToFinish", forceSave: true);
        }

        public static void DoWindowWithSettings(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Gap();
            //
            listing_Standard.Label($"Current brain damage chance: {brainDamageChance*100:0}%", -1f, null);
            brainDamageChance = (listing_Standard.Slider(brainDamageChance, 0.01f, 1.0f));
            listing_Standard.Gap();
            listing_Standard.Label($"Current days to finish chamber conversion: "+ daysToFinish, -1f, null);
            daysToFinish = ((int)listing_Standard.Slider(daysToFinish, 1, 60));
            //
            listing_Standard.End();
            Rect rect = inRect.BottomPart(0.1f).LeftPart(0.1f);
            Rect rect2 = inRect.BottomPart(0.1f).RightPart(0.1f);
            bool flag = Widgets.ButtonText(rect, "Apply Settings", true, true, true);
            bool flag2 = flag;
            if (flag2)
            {
                ApplySettings();
            }
            bool flag3 = Widgets.ButtonText(rect2, "Reset Settings", true, true, true);
            bool flag4 = flag3;
            if (flag4)
            {
                ResetFactor();
            }
        }
        public static void ApplySettings()
        {
            Building_Chamber.daysToFinish = daysToFinish;
            Building_Chamber.brainDamageChance = brainDamageChance;
        }
        private static void ResetFactor()
        {
            daysToFinish = 10;
            brainDamageChance = 0.6f;
        }

    }
}
