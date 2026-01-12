using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class GrimWorldMod : Mod
    {
        private Settings settings;

        private List<SettingsTabRecord> tabs = new List<SettingsTabRecord>();
        public List<SettingsTabRecord> Tabs => tabs;

        private SettingsTabDef curTabInt;
        public SettingsTabDef CurTab
        {
            get
            {
                return curTabInt;
            }
            set
            {
                if (value != curTabInt)
                {
                    curTabInt = value;
                }
            }
        }
        private SettingsTabRecord CurTabRecord
        {
            get
            {
                var record = tabs.Find(x => x.Selected);
                if (record == null)
                {
                    return tabs.First();
                }
                return record;
            }
        }

        public GrimWorldMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (settings == null)
            {
                settings = GetSettings<Settings>();
            }
            if (tabs.NullOrEmpty())
            {
                foreach (SettingsTabDef tabDef in DefDatabase<SettingsTabDef>.AllDefs)
                {
                    object[] parms = new object[] {tabDef, tabDef.LabelCap.ToString(), new Action(delegate
                {
                    CurTab = tabDef;
                }), new Func<bool>(delegate
                {
                    return CurTab == tabDef;
                })};
                    SettingsTabRecord record = Activator.CreateInstance(tabDef.settingsTabClass, parms) as SettingsTabRecord;
                    tabs.Add(record);
                }
            }

            if (!tabs.NullOrEmpty())
            {
                Rect drawTabsRect = new Rect(inRect.x + 10, inRect.y + 40, inRect.width - 20, inRect.height - 40);
                Widgets.DrawMenuSection(drawTabsRect);
                TabDrawer.DrawTabs(drawTabsRect, tabs);
                CurTabRecord.OnGUI(drawTabsRect);
            }
        }

        //public override void WriteSettings()
        //{
        //    base.WriteSettings();
        //    settings.CastChanges();
        //}
        public override string SettingsCategory()
        {
            return "GrimWorld";
        }
    }
}
