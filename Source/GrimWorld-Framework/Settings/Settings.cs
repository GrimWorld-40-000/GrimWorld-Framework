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
        
        public static Settings Instance => LoadedModManager.GetMod<GrimWorldMod>().GetSettings<Settings>();

        private List<SettingsRecord> modSettings;

        public bool TryGetModSettings(Type type, out SettingsRecord settingsRecord)
        {
            if (modSettings.NullOrEmpty())
            {
                Reset();
            }

            
            if (!HaveTagsEverLoaded)
            {
                HaveTagsEverLoaded = true;
                Log.Message("Grimworld is loading it's tag system for the first time! Setting default values");  
                Instance.Reset();
            }
            
            
            settingsRecord = modSettings.Find(x => x.GetType() == type);
            return settingsRecord != null;
        }
        public void CastChanges()
        {
            if (!modSettings.NullOrEmpty())
            {
                foreach (var pair in modSettings)
                {
                    pair?.CastChanges();
                }
            }
            else
            {
                Reset();
            }
        }

        public void Reset()
        {
            if (!modSettings.NullOrEmpty())
            {
                foreach (SettingsRecord settingsRecord in modSettings)
                {
                    settingsRecord.Reset();
                    //Log.Message($"{settingsRecord.GetType().Name} reset");
                }
                foreach (SettingsTabDef settingsTabDef in DefDatabase<SettingsTabDef>.AllDefs)
                {
                    if (!TryGetModSettings(settingsTabDef.settingsRecordClass, out SettingsRecord settingsRecord))
                    {
                        settingsRecord = (SettingsRecord)Activator.CreateInstance(settingsTabDef.settingsRecordClass);
                        settingsRecord.Reset();
                        //Log.Message($"{settingsRecord.GetType().Name} reset");
                        modSettings.Add(settingsRecord);
                    }
                }
            }
            else
            {
                modSettings = new List<SettingsRecord>();
                foreach (SettingsTabDef settingsTabDef in DefDatabase<SettingsTabDef>.AllDefs)
                {
                    SettingsRecord settingsRecord = (SettingsRecord)Activator.CreateInstance(settingsTabDef.settingsRecordClass);
                    settingsRecord.Reset();
                    //Log.Message($"{settingsRecord.GetType().Name} reset");
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
