//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GrimworldItemLimit;
//using UnityEngine;
//using Verse;

//namespace GW_Frame.Settings
//{
//    public class SettingsRecord_WeaponTags : SettingsRecord
//    {
//        private Dictionary<string, bool> shieldTagsDict;
//        private Dictionary<string, bool> twoHandedTagsDict;

        
//        public SettingsRecord_WeaponTags()
//        {
            
//        }

//        public bool TryGetValueShield(ThingDef thingDef, out bool value)
//        {
//            if (shieldTagsDict.TryGetValue(thingDef.defName, out value))
//            {
//                return true;
//            }
//            else
//            {
//                bool newValue = IsThingShieldByDefault(thingDef);
//                shieldTagsDict.Add(thingDef.defName, newValue);
//                return newValue;
//            }
//        }
//        public bool TrySetValueShield(ThingDef thingDef, bool value)
//        {
//            if (shieldTagsDict.TryGetValue(thingDef.defName, out _))
//            {
//                shieldTagsDict[thingDef.defName] = value;
//            }
//            return false;
//        }
//        public bool TryGetValueTwoHanded(ThingDef thingDef, out bool value)
//        {
//            if (twoHandedTagsDict.TryGetValue(thingDef.defName, out value))
//            {
//                return true;
//            }
//            else
//            {
//                bool newValue = IsThingTwoHandedByDefault(thingDef);
//                twoHandedTagsDict.Add(thingDef.defName, newValue);
//                return newValue;
//            }
//        }
//        public bool TrySetValueTwoHanded(ThingDef thingDef, bool value)
//        {
//            if (twoHandedTagsDict.TryGetValue(thingDef.defName, out _))
//            {
//                twoHandedTagsDict[thingDef.defName] = value;
//            }
//            return false;
//        }
//        public override void CastChanges()
//        {
//            foreach(var pair in shieldTagsDict)
//            {
//                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
//                ChangeCategories(def, Grimworld_DefsOf.GW_Shield, pair.Value);
//            }
//            foreach (var pair in twoHandedTagsDict)
//            {
//                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
//                ChangeCategories(def, Grimworld_DefsOf.GW_TwoHanded, pair.Value);
//            }
//        }

//        private void ChangeCategories(ThingDef def, ThingCategoryDef thingCategoryDef, bool add)
//        {
//            if (def != null)
//            {
//                if (add)
//                {
//                    if (def.thingCategories == null)
//                    {
//                        def.thingCategories = new List<ThingCategoryDef>();
//                    }
//                    def.thingCategories.AddDistinct(thingCategoryDef);
//                    thingCategoryDef.childThingDefs.Add(def);
//                }
//                else
//                {
//                    if (def.thingCategories != null && def.thingCategories.Any(x => x == thingCategoryDef))
//                    {
//                        def.thingCategories.Remove(thingCategoryDef);
//                        thingCategoryDef.childThingDefs.Remove(def);
//                    }
//                }
//            }
//        }
//        public override void Reset()
//        {
//            shieldTagsDict = new Dictionary<string, bool>();
//            twoHandedTagsDict = new Dictionary<string, bool>();
//            Comp_ItemCraftingLimit.DisabledDefNames = new List<string>();
//            ResetToDefaultForDefs(DefDatabase<ThingDef>.AllDefs);
//        }

//        private static List<string> DisabledDefNames => Comp_ItemCraftingLimit.DisabledDefNames;
        
//        public bool IsLimitEnabled(ThingDef item)
//        {
//            return !DisabledDefNames.Contains(item.defName);
//        }

//        public void SetCraftingLimitEnabled(ThingDef thingDef, bool isEnabled)
//        {
//            if (isEnabled)
//            {
//                Comp_ItemCraftingLimit.DisabledDefNames.RemoveWhere(name => name.Equals(thingDef.defName));
//            }
//            else
//            {
//                DisabledDefNames.Add(thingDef.defName);
//            }
//        }
//        public void ResetToDefaultForDefs(IEnumerable<ThingDef> defsToReset)
//        {
//            var thingDefs = defsToReset as ThingDef[] ?? defsToReset.ToArray();
//            foreach (ThingDef def in thingDefs)
//            {
//                if (!def.IsApparel && !def.IsWeapon) continue;
//                shieldTagsDict.SetOrAdd(def.defName, IsThingShieldByDefault(def));
//            }

//            foreach (ThingDef def in thingDefs)
//            {
//                if (!def.IsWeapon) continue;
//                twoHandedTagsDict.SetOrAdd(def.defName, IsThingTwoHandedByDefault(def));
//            }
//        }

//        private bool IsThingTwoHandedByDefault(ThingDef thing)
//        {
//            return thing.thingCategories != null && (thing.thingCategories.Contains(Grimworld_DefsOf.GW_TwoHanded) ||
//                                                     thing.thingCategories.Contains(Grimworld_DefsOf.TwoHanded));
//        }
        
//        private bool IsThingShieldByDefault(ThingDef thing)
//        {
//            return thing.thingCategories != null && thing.thingCategories.Contains(Grimworld_DefsOf.GW_Shield);
//        }
        
//        public override void ExposeData()
//        {
//            Scribe_Collections.Look(ref shieldTagsDict, "shieldTagsDict", LookMode.Value, LookMode.Value);
//            Scribe_Collections.Look(ref twoHandedTagsDict, "twoHandedTagsDict", LookMode.Value, LookMode.Value);
//            Scribe_Collections.Look(ref Comp_ItemCraftingLimit.DisabledDefNames, "disabledDefNames");
//            if (DisabledDefNames == null)
//            {
//                Comp_ItemCraftingLimit.DisabledDefNames = new List<string>();
//            }
//        }

//    }
//}
