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

        private string searchTerm = string.Empty;
        private float listHeight;
        private float settingsAreaListHeight;
        private Vector2 scrollPositionList = Vector2.zero;
        private Vector2 scrollPositionTabs = Vector2.zero;
        public SettingsTabRecord_WeaponTags(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected)
            : base(def, label, clickedAction, selected)
        {

        }

        private static int _currentTabIndex;
        
        public override void OnGUI(Rect rect)
        {
            if (SettingsRecord == null) return;
            
            
            ListTabInfo[] tabs = GenerateTabs().ToArray();
            
            
                
            //Tabs section
            Rect tabSection = rect.LeftPart(0.3f).ContractedBy(5f);
            Rect vertTabRect = new Rect(0, 0, tabSection.width - 20, listHeight);
            Widgets.BeginScrollView(tabSection, ref scrollPositionTabs, vertTabRect);
            float tabListHeight = 0;




            int ind = 0;
            foreach (ListTabInfo tab in tabs)
            {
                Rect tabRect = new Rect(tabSection.x, tabListHeight, tabSection.width - 20, 80);

                Text.Anchor = TextAnchor.MiddleCenter;

                if (_currentTabIndex == ind)
                {
                    GUI.color = Color.grey;
                }
                
                if (Widgets.ButtonText(tabRect, tab.Label))
                {
                    _currentTabIndex = ind;
                }
                
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                tabListHeight += tabRect.height;
                ind++;
            }

            listHeight = tabListHeight;
            Widgets.EndScrollView();
                
                
                
            //Actual section
            Rect settingsSection = rect.RightPart(0.6f).ContractedBy(5f);
            Rect vertRect = new Rect(settingsSection.x, 0, settingsSection.width - 20, settingsAreaListHeight);
            Widgets.BeginScrollView(settingsSection, ref scrollPositionList, vertRect);
            float settingsListHeight = 0;
            
            
            Rect searchBarRect = new Rect(settingsSection.x - ((settingsSection.width - 20) / 8), settingsListHeight, (settingsSection.width - 20) / 2, 25);

            
            
            searchTerm = Widgets.TextEntryLabeled(searchBarRect, $"{"Filter".Translate()}: ", searchTerm);
            
            settingsListHeight += searchBarRect.height;

            List<ThingDef> defs = tabs[_currentTabIndex].Defs.ToList();

            //Filters out based on search term
            if (searchTerm != string.Empty)
            {
                defs.RemoveWhere(thingDef => !thingDef.label.ToLower().Contains(searchTerm.ToLower()));
            }

            foreach (ThingDef thingDef in defs)
            {
                //Create rects
                
                Rect defRect = new Rect(settingsSection.x, settingsListHeight, settingsSection.width - 20, 80);
                Rect iconRect = defRect.LeftPartPixels(defRect.height).ContractedBy(5f);
                Rect textRect = defRect.RightPartPixels(Math.Min(defRect.width - defRect.height, defRect.width / 3));
                textRect.x -= 10;
                Rect labelRect = textRect.TopHalf();
                Rect valueRect = textRect.BottomHalf();
                
                Widgets.DrawMenuSection(defRect);

                Widgets.DrawTextureFitted(iconRect, thingDef.uiIcon, 1);
                
                TooltipHandler.TipRegion(iconRect, thingDef.description);
                
                Widgets.Label(labelRect, thingDef.LabelCap);

                //Add the value checkbox
                switch (tabs[_currentTabIndex].ListTabType)
                {
                    case ListTabInfo.ListType.Apparel:
                        SettingsRecord.TryGetValueShield(thingDef, out bool shieldValue);
                        Widgets.CheckboxLabeled(valueRect, Grimworld_DefsOf.GW_Shield.LabelCap, ref shieldValue);
                        SettingsRecord.TrySetValueShield(thingDef, shieldValue);
                        break;
                    case ListTabInfo.ListType.Weapon:
                        SettingsRecord.TryGetValueTwoHanded(thingDef, out bool twoHandedValue);
                        Widgets.CheckboxLabeled(valueRect, Grimworld_DefsOf.GW_TwoHanded.LabelCap, ref twoHandedValue);
                        SettingsRecord.TrySetValueTwoHanded(thingDef, twoHandedValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                settingsListHeight += defRect.height;
            }

            settingsAreaListHeight = settingsListHeight;
            Widgets.EndScrollView();
        }

        private IEnumerable<ListTabInfo> GenerateTabs()
        {
            //All sub-categories of weapon, except the two-handed category
            foreach (var weaponsChildCategory in ThingCategoryDefOf.Weapons.childCategories.Where(weaponsChildCategory => weaponsChildCategory != Grimworld_DefsOf.GW_TwoHanded && weaponsChildCategory != Grimworld_DefsOf.GW_Shield))
            {
                yield return new ListTabInfo(delegate
                {
                    return DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(x => x.thingCategories?.Contains(weaponsChildCategory) ?? false)
                        .OrderBy(x => x.label);
                }, weaponsChildCategory.LabelCap, ListTabInfo.ListType.Weapon);
            }

            //Apparel tab
            yield return new ListTabInfo(delegate
            {
                return DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel)
                    .OrderBy(x => x.label);
            }, "Apparel".Translate(), ListTabInfo.ListType.Apparel);
        }
    }



    public class ListTabInfo
    {
        public ListTabInfo(Func<IEnumerable<ThingDef>> valueGetter, string label, ListType type)
        {
            ValueGetter = valueGetter;
            Label = label;
            ListTabType = type;
        }

        public ListType ListTabType;
        /// <summary>
        /// The list tabs use a function that may or may not be called later, so it's not getting the list every time.
        /// </summary>
        public Func<IEnumerable<ThingDef>> ValueGetter;
        public string Label;
        
        public enum ListType
        {
            Weapon,
            Apparel
        }

        public IEnumerable<ThingDef> Defs => ValueGetter.Invoke();
    }

}
