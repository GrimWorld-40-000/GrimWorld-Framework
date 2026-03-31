using System;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class SettingsTabRecord_Patches : SettingsTabRecord
    {
        private static SettingsRecord_Patches settingsRecord;
        public static SettingsRecord_Patches SettingsRecord
        {
            get
            {
                if (settingsRecord == null)
                {
                    Settings.Instance.TryGetModSettings(typeof(SettingsRecord_Patches), out SettingsRecord record);
                    settingsRecord = record as SettingsRecord_Patches;
                }
                return settingsRecord;
            }
        }

        private Vector2 scrollPosition;
        private const float CheckboxSize = 24f;
        private const float LabelOffset = CheckboxSize + 6f;

        public SettingsTabRecord_Patches(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected)
            : base(def, label, clickedAction, selected) { }

        private static bool GWAny =>
            ModsConfig.IsActive("Grimworld.Core") ||
            ModsConfig.IsActive("Grimworld.AstraMilitarum") ||
            ModsConfig.IsActive("grimworld.talonOfTheEmperor") ||
            ModsConfig.IsActive("grimworld.HighestRulers");

        public override void OnGUI(Rect rect)
        {
            if (SettingsRecord == null) return;

            Rect inner = rect.ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, inner.width - 16f, 520f);
            Widgets.BeginScrollView(inner, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            bool gwAny = GWAny;

            // Master toggle — display reflects AnyEnabled; click sets all children
            DrawToggle(listing, "Enable GW: All Patches", "GWAll_Master", gwAny);

            // Individual GW patch toggles
            DrawToggle(listing, "    Enable GW: Build Menu Cleanup Patch",  "GWCat",      gwAny);
            // DrawToggle(listing, "    Enable GW: Corpse Starch Patch",        "GWCore",     gwAny); patch removed - integraged
            // DrawToggleAlwaysOff(listing, "    GW: Aspectus Imperialis Patch (empty)"); // patch removed — empty
            // DrawToggle(listing, "    Enable GW: Airtight Patch",             "GWGravship", gwAny);
            DrawToggle(listing, "    Enable GW: Clean Research Patch",       "GWResearch", gwAny);
            DrawToggle(listing, "    Enable GW: Research Balance Patch",     "GWBalance",  gwAny);
            DrawToggle(listing, "    Enable GW: Gene Seeding Patch",         "GWGene",     gwAny);
            DrawToggle(listing, "    Enable GW: Vanilla Expanded Hook",      "VEHook",     gwAny);

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawToggle(Listing_Standard list, string label, string key, bool active)
        {
            Rect r = list.GetRect(Text.LineHeight);

            if (key == "GWAll_Master")
            {
                bool val = SettingsRecord.AnyEnabled;
                if (!active)
                {
                    GUI.color = Color.gray;
                    Widgets.CheckboxDraw(r.x, r.y, false, true, CheckboxSize);
                    Widgets.Label(new Rect(r.x + LabelOffset, r.y, r.width, r.height), label + " (No GW mod active)");
                    GUI.color = Color.white;
                    return;
                }
                Widgets.CheckboxDraw(r.x, r.y, val, false, CheckboxSize);
                Widgets.Label(new Rect(r.x + LabelOffset, r.y, r.width, r.height), label);
                if (Widgets.ButtonInvisible(r))
                    SettingsRecord.SetAll(!val);
                return;
            }

            bool value = SettingsRecord.Get(key);
            if (!active)
            {
                GUI.color = Color.gray;
                Widgets.CheckboxDraw(r.x, r.y, false, true, CheckboxSize);
                Widgets.Label(new Rect(r.x + LabelOffset, r.y, r.width, r.height), label + " (Missing)");
                GUI.color = Color.white;
                return;
            }

            Widgets.CheckboxDraw(r.x, r.y, value, false, CheckboxSize);
            Widgets.Label(new Rect(r.x + LabelOffset, r.y, r.width, r.height), label);
            if (Widgets.ButtonInvisible(r))
                SettingsRecord.Set(key, !value);
        }

        private void DrawToggleAlwaysOff(Listing_Standard list, string label)
        {
            Rect r = list.GetRect(Text.LineHeight);
            GUI.color = Color.gray;
            Widgets.CheckboxDraw(r.x, r.y, false, true, CheckboxSize);
            Widgets.Label(new Rect(r.x + LabelOffset, r.y, r.width, r.height), label);
            GUI.color = Color.white;
        }
    }
}
