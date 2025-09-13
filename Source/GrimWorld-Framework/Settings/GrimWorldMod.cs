using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GW_Frame.AssetUtils;
using GW_Frame.Debugging;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GW_Frame.Settings
{
    public class GrimWorldMod : Mod
    {
        public List<SettingsTabRecord> Tabs => tabs;

        public static GrimWorldMod Mod;
        
        private Settings settings;
        private SettingsTabDef curTabInt;
        private List<SettingsTabRecord> tabs = [];
        
        public GrimWorldMod(ModContentPack content) : base(content)
        {
            Mod = this;
            settings = GetSettings<Settings>();
            
            Harmony harmony = new("Rimworld.Grimworld.Framework.assets");
            
            harmony.Patch(original: AccessTools.PropertyGetter(typeof(ShaderTypeDef), nameof(ShaderTypeDef.Shader)),
                prefix: new HarmonyMethod(typeof(GrimWorldMod),
                    nameof(ShaderFromAssetBundle)));
            
            harmony.PatchAll();
        }
        
        /// <summary>
        /// This patch needs to run in the mods' ctor, hence... here it is!
        /// This patch is necessary if you intend to use the new Cutout_LUT shader in XML.
        /// </summary>
        public static void ShaderFromAssetBundle(ShaderTypeDef __instance, ref Shader ___shaderInt)
        {
            if (__instance is not GWShaderTypeDef) 
                return;
            
            ___shaderInt = ContentDatabase.GWBundle.LoadAsset<Shader>(__instance.shaderPath);
            
            if (___shaderInt is null)
            {
                GWLog.Error($"Failed to load Shader from path <text>\"{__instance.shaderPath}\"</text>");
            }
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings ??= GetSettings<Settings>();
            
            if (tabs.NullOrEmpty())
            {
                foreach (SettingsTabDef tabDef in DefDatabase<SettingsTabDef>.AllDefs)
                {
                    object[] parms = new object[] {tabDef, tabDef.LabelCap.ToString(), new Action(delegate
                {
                    CurTab = tabDef;
                }), new Func<bool>(() => CurTab == tabDef)};
                    SettingsTabRecord record = Activator.CreateInstance(tabDef.settingsTabClass, parms) as SettingsTabRecord;
                    tabs.Add(record);
                }
            }
            
            if (tabs.NullOrEmpty()) 
                return;
            
            Rect drawTabsRect = new (inRect.x + 10, inRect.y + 40, inRect.width - 20, inRect.height - 40);
            Widgets.DrawMenuSection(drawTabsRect);
            TabDrawer.DrawTabs(drawTabsRect, tabs);
            CurTabRecord.OnGUI(drawTabsRect);
        }
        
        private SettingsTabDef CurTab
        {
            get => curTabInt;
            set
            {
                if (value != null && value != curTabInt)
                {
                    curTabInt = value;
                }
            }
        }
        
        private SettingsTabRecord CurTabRecord
        {
            get
            {
                SettingsTabRecord record = tabs
                    .Find(x => x.Selected);
                
                return record ?? tabs.First();
            }
        }
        
        public AssetBundle MainBundle
        {
            get
            {
                // all logging in this method is for debugging
                string text = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    text = "StandaloneOSX";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    text = "StandaloneWindows64";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    text = "StandaloneLinux64";
                }
                string bundlePath = Path.Combine(base.Content.RootDir, "Materials\\Bundles\\" + text + "\\grimworldframeworkbundle");
                //GWLog.Message("Bundle Path: " + bundlePath);

                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

                if (bundle == null)
                {
                    //GWLog.Error("Failed to load bundle at path: " + bundlePath);
                }

                foreach (string allAssetName in bundle.GetAllAssetNames())
                {
                    //GWLog.Message($"[{allAssetName}]");
                }

                return bundle;
            }
        }
        
        //public override void WriteSettings()
        //{
        //    base.WriteSettings();
        //    settings.CastChanges();
        //}
        
        public override string SettingsCategory()
        {
            // use a keyed language string so others can easily translate this
            return "GrimWorld";
        }
    }
}