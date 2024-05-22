using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class SettingsTabRecord_WeaponTags : SettingsTabRecord
    {
        private SettingsRecord_WeaponTags settingsRecord;
        public SettingsRecord_WeaponTags SettingsRecord
        {
            get
            {
                if (settingsRecord == null)
                {
                    Settings.Instance.TryGetModSettings(typeof(SettingsRecord_WeaponTags), out SettingsRecord settingsRecord);
                    this.settingsRecord = settingsRecord as SettingsRecord_WeaponTags;
                }
                return settingsRecord;
            }
        }

        private float listHeight;
        private Vector2 scrollPositionList = Vector2.zero;
        public SettingsTabRecord_WeaponTags(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected)
            : base(def, label, clickedAction, selected)
        {

        }
        public override void OnGUI(Rect rect)
        {
            if (SettingsRecord != null)
            {
                Rect listRect = new Rect(rect.x + 10, rect.y + 10, rect.width / 2, rect.height - 20);
                Rect vertRect = new Rect(0, 0, listRect.width - 20, this.listHeight);
                Widgets.BeginScrollView(listRect, ref scrollPositionList, vertRect);
                float listHeight = 0;

                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel || x.IsWeapon).OrderBy(x => x.label))
                {
                    SettingsRecord.TryGetValueShield(thingDef, out bool shieldValue);
                    SettingsRecord.TryGetValueTwoHanded(thingDef, out bool twoHandedValue);

                    Rect defRect = new Rect(0, listHeight, listRect.width - 20, 80);
                    Widgets.DrawMenuSection(defRect);

                    Rect labelRect = new Rect(defRect.x + 10, defRect.y + 10, defRect.width / 2f - 15, 60);
                    Widgets.Label(labelRect, thingDef.LabelCap);

                    Rect shieldRect = new Rect(labelRect.x + labelRect.width + 10, labelRect.y, labelRect.width, 30);
                    Widgets.CheckboxLabeled(shieldRect, ThingCategoryDef.Named("GW_Shield").LabelCap, ref shieldValue);
                    SettingsRecord.TrySetValueShield(thingDef, shieldValue);

                    Rect twoHandedRect = new Rect(shieldRect.x, shieldRect.y + shieldRect.height, shieldRect.width, shieldRect.height);
                    Widgets.CheckboxLabeled(twoHandedRect, ThingCategoryDef.Named("GW_TwoHanded").LabelCap, ref twoHandedValue);
                    SettingsRecord.TrySetValueTwoHanded(thingDef, twoHandedValue);

                    listHeight += defRect.height;
                }

                this.listHeight = listHeight;
                Widgets.EndScrollView();
            }
        }
    }
}
