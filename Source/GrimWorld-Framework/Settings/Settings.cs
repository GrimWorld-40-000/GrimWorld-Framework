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
    public class Settings : ModSettings
    {
        public static bool HaveTagsEverLoaded;
        private static Settings _cachedSettings;
        public static Settings Instance =>
        _cachedSettings ?? (_cachedSettings = LoadedModManager.GetMod<GrimWorldMod>().GetSettings<Settings>());

        private List<SettingsRecord> modSettings = new List<SettingsRecord>();
        
        
        public bool TryGetModSettings(Type type, out SettingsRecord settingsRecord)
        {
            if (modSettings == null || modSettings.Count == 0)
            {
                Reset();
            }

            
            if (!HaveTagsEverLoaded)
            {
                HaveTagsEverLoaded = true;
                Log.Message("Grimworld is loading its tag system for the first time! Setting default values");  
                Instance.Reset();
            }
            
            settingsRecord = modSettings.Find(x => x != null && x.GetType() == type);

            if (settingsRecord != null) return true;
            settingsRecord = Activator.CreateInstance(type) as SettingsRecord;
            if (settingsRecord == null) return false;
            settingsRecord.Reset();
            modSettings.Add(settingsRecord);


            return true;
        }
        
        
        public bool TryGetModSettings<T>(out T settingsRecord) where T : SettingsRecord
        {
            if (modSettings == null || modSettings.Count == 0)
            {
                Reset();
            }

            
            if (!HaveTagsEverLoaded)
            {
                HaveTagsEverLoaded = true;
                Log.Message("Grimworld is loading its tag system for the first time! Setting default values");  
                Instance.Reset();
            }
            
            settingsRecord = modSettings.Find(x => x != null && x.GetType() == typeof(T)) as T;

            if (settingsRecord != null) return true;

            settingsRecord = Activator.CreateInstance(typeof(T)) as T;
            if (settingsRecord == null) return false;
            settingsRecord.Reset();
            modSettings.Add(settingsRecord);

            return true;
        }
        public void CastChanges()
        {
            if (modSettings == null)
            {
                Reset();
            }
            
                foreach (var pair in modSettings)
                {
                    pair?.CastChanges();
                }
                
                modSettings.RemoveAll(record => record == null);

        }

        public void Reset()
        {
            if (modSettings == null)
                modSettings = new List<SettingsRecord>();

            modSettings.RemoveAll(x => x == null);


                foreach (SettingsRecord settingsRecord in modSettings)
                {
                    if (settingsRecord != null)
                        settingsRecord.Reset();
                        //Log.Message($"{settingsRecord.GetType().Name} reset");
                }


                foreach (SettingsTabDef settingsTabDef in DefDatabase<SettingsTabDef>.AllDefs)
                {
                    var settingsRecord = modSettings.Find(
                        x => x != null && x.GetType() == settingsTabDef.settingsRecordClass
                    );

                    if (settingsRecord == null)
                    {
                        settingsRecord = (SettingsRecord)Activator.CreateInstance(settingsTabDef.settingsRecordClass);
                        settingsRecord.Reset();
                        modSettings.Add(settingsRecord);
                    }
                }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref modSettings, "modSettings", LookMode.Deep);
            Scribe_Values.Look(ref HaveTagsEverLoaded, "GW_HaveTagsEverLoaded");
        }
    }
 
}
