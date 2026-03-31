using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class SettingsTabRecord_Main : SettingsTabRecord
    {
        private static readonly List<(string label, Action<Listing_Standard> drawer)> sections
            = new List<(string, Action<Listing_Standard>)>();

        public static void RegisterSection(string label, Action<Listing_Standard> drawer)
        {
            sections.Add((label, drawer));
        }

        public SettingsTabRecord_Main(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected)
            : base(def, label, clickedAction, selected) { }

        public override void OnGUI(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect.ContractedBy(10f));
            listing.Gap();

            foreach (var (sectionLabel, drawer) in sections)
            {
                listing.Label(sectionLabel);
                listing.GapLine();
                drawer(listing);
                listing.Gap();
            }

            listing.End();
        }
    }
}
