using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class SettingsRecord_WeaponTags : SettingsRecord
    {
        private Dictionary<string, bool> shieldTagsDict;
        private Dictionary<string, bool> twoHandedTagsDict;

        public bool TryGetValueShield(ThingDef thingDef, out bool value)
        {
            if (shieldTagsDict.TryGetValue(thingDef.defName, out value))
            {
                return true;
            }
            return false;
        }
        public bool TrySetValueShield(ThingDef thingDef, bool value)
        {
            if (shieldTagsDict.TryGetValue(thingDef.defName, out _))
            {
                shieldTagsDict[thingDef.defName] = value;
            }
            return false;
        }
        public bool TryGetValueTwoHanded(ThingDef thingDef, out bool value)
        {
            if (twoHandedTagsDict.TryGetValue(thingDef.defName, out value))
            {
                return true;
            }
            return false;
        }
        public bool TrySetValueTwoHanded(ThingDef thingDef, bool value)
        {
            if (twoHandedTagsDict.TryGetValue(thingDef.defName, out _))
            {
                twoHandedTagsDict[thingDef.defName] = value;
            }
            return false;
        }
        public override void CastChanges()
        {
            foreach(var pair in shieldTagsDict)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
                ChangeCategories(def, ThingCategoryDef.Named("GW_Shield"), pair.Value);
            }
            foreach (var pair in twoHandedTagsDict)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
                ChangeCategories(def, ThingCategoryDef.Named("GW_TwoHanded"), pair.Value);
            }
        }

        private void ChangeCategories(ThingDef def, ThingCategoryDef thingCategoryDef, bool add)
        {
            if (def != null)
            {
                if (add)
                {
                    if (def.thingCategories == null)
                    {
                        def.thingCategories = new List<ThingCategoryDef>();
                    }
                    def.thingCategories.AddDistinct(thingCategoryDef);
                    thingCategoryDef.childThingDefs.Add(def);
                }
                else
                {
                    if (def.thingCategories != null && def.thingCategories.Any(x => x == thingCategoryDef))
                    {
                        def.thingCategories.Remove(thingCategoryDef);
                        thingCategoryDef.childThingDefs.Remove(def);
                    }
                }
            }
        }
        public override void Reset()
        {
            shieldTagsDict = new Dictionary<string, bool>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.IsApparel || def.IsWeapon)
                {
                    if (def.thingCategories != null && def.thingCategories.Any(x => x.defName == "GW_Shield"))
                    {
                        shieldTagsDict.Add(def.defName, true);
                    }
                    else
                    {
                        shieldTagsDict.Add(def.defName, false);
                    }
                }
            }

            twoHandedTagsDict = new Dictionary<string, bool>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.IsApparel || def.IsWeapon)
                {
                    if (def.thingCategories != null && def.thingCategories.Any(x => x.defName == "GW_TwoHanded"))
                    {
                        twoHandedTagsDict.Add(def.defName, true);
                    }
                    else
                    {
                        twoHandedTagsDict.Add(def.defName, false);
                    }
                }
            }
        }
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref shieldTagsDict, "shieldTagsDict", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref twoHandedTagsDict, "twoHandedTagsDict", LookMode.Value, LookMode.Value);
        }
    }
}
