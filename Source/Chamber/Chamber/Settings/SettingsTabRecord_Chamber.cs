using System;
using GW_Frame.Settings;
using UnityEngine;
using Verse;

namespace Chamber.Settings
{
    public class SettingsTabRecord_Chamber: SettingsTabRecord
    {
        private SettingsRecord_Chamber settingsRecord;
        public SettingsRecord_Chamber SettingsRecord
        {
            get
            {
                if (settingsRecord == null)
                {
                    GW_Frame.Settings.Settings.Instance.TryGetModSettings(typeof(SettingsRecord_Chamber), out SettingsRecord settingsRecord);
                    this.settingsRecord = settingsRecord as SettingsRecord_Chamber;
                }
                return settingsRecord;
            }
        }
        
        
        public SettingsTabRecord_Chamber(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected) : base(def, label, clickedAction, selected)
        {
            
        }


        public override void OnGUI(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Gap();

            float brainDamageChance = SettingsRecord_Chamber.brainDamageChance;
            int daysToFinish = SettingsRecord_Chamber.daysToFinish;
            
            //
            listing_Standard.Label($"Current brain damage chance (Default 60%): {brainDamageChance*100:0}%", -1f, null);
            SettingsRecord_Chamber.brainDamageChance = listing_Standard.Slider(brainDamageChance, 0.01f, 1.0f);
            listing_Standard.Gap();
            listing_Standard.Label($"Current days to finish chamber conversion (default 10): "+ daysToFinish, -1f, null);
            SettingsRecord_Chamber.daysToFinish = (int)listing_Standard.Slider(daysToFinish, 1, 60);
            //
            listing_Standard.End();
            Rect rect2 = inRect.BottomPart(0.1f).RightPart(0.1f);
            /*if (Widgets.ButtonText(rect2, "Reset Settings"))
            {
                SettingsRecord.Reset();
            }*/
        }
    }
}