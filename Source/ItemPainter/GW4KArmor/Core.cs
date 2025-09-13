using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using GW4KArmor.Debugging;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GW4KArmor
{
    public class Core : Mod
    {
        public static HashSet<CompProperties_TriColorMask> AllMaskable = new ();

        public static Material MaskMaterial { get; private set; }
        public static Shader MaskShader { get; private set; }
        public static Harmony HarmonyInstance { get; private set; }

        /*
        [DebugAction("_GW", allowedGameStates = AllowedGameStates.Entry)]
        private static void OpenWindow()
        {
            using (new HarmonyPatches.ThingIDPatch.Scope())
            {
                ThingDef def = AllMaskable.RandomElement().Def;
                var thing = ThingMaker.MakeThing(def);
                Window_ItemPainter.OpenWindowFor(thing as ThingWithComps);
            }
        }

        [DebugAction("_GW", allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.ToolMap)]
        private static void OpenWindowFor()
        {
            foreach (var thing in Find.CurrentMap.thingGrid.ThingsAt(Verse.UI.MouseCell()))
            {
                var apparel = thing as Apparel;
                var flag = apparel == null;
                if (!flag)
                {
                    Comp_TriColorMask comp = apparel.GetComp<Comp_TriColorMask>();
                    bool flag2 = comp == null;
                    if (!flag2) Window_ItemPainter.OpenWindowFor(apparel);
                }
            }
        }
        */
        
        public Core(ModContentPack content) : base(content)
        {
            try
            {
                HarmonyInstance = new Harmony(Content.PackageId);
                HarmonyInstance.PatchAll();
            }
            catch (Exception e)
            {
                GW4KLog.Error("Harmony patching failed:" + e);
            }

            LongEventHandler.QueueLongEvent(FindMaskableThings,
                "GW4KArmor.LoadText", false, null);
            LongEventHandler.QueueLongEvent(LoadBundleContent, 
                "GW4KArmor.LoadText", false, null);
        }

        private static void FindMaskableThings()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                CompProperties_TriColorMask compProperties = thingDef
                    .GetCompProperties<CompProperties_TriColorMask>();
                bool flag = compProperties == null;

                if (flag) 
                    continue;
                
                compProperties.Def = thingDef;
                AllMaskable.Add(compProperties);
            }
            
            GW4KLog.Warning($"Found {AllMaskable.Count} maskable armor or weapons.");
        }

        private void LoadBundleContent()
        {
            string platform = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "StandaloneWindows64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = "StandaloneLinux64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "StandaloneOSX";
            }
            else
            {
                GW4KLog.Error("Unknown platform! This mod will not work.");
                platform = null;
            }
            
            // why the fuck is this locked to the 1.4 folder??? :sip:
            string assetPath = Path.Combine(Content.RootDir, 
                "1.4", "AssetBundles", platform, "gw4k");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetPath);
            
            if (assetBundle == null)
            {
                GW4KLog.Error("Asset bundle failed to load, " +
                              "so the shader cannot be used! This mod will not work.");
            }
            else
            {
                MaskMaterial = assetBundle.LoadAsset<Material>("CustomMaskMaterial");
                if (MaskMaterial == null)
                {
                    GW4KLog.Error("Asset bundle was loaded but failed to " +
                                  "find the mask material. Why!? This mod will not work.");
                }
                else
                {
                    MaskShader = MaskMaterial.shader;
                }
            }
        }
    }
}