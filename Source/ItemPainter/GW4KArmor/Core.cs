using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GW4KArmor.Patches;
using GW4KArmor.UI;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GW4KArmor;

[StaticConstructorOnStartup]
public class Core : Mod
{
    public static HashSet<CompProperties_TriColorMask> AllMaskable = new ();

    public static Material MaskMaterial { get; private set; }
    public static Shader MaskShader { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    // Kept alive so Unity doesn't free the bundle's assets during UnloadUnusedAssets()
    private static AssetBundle _bundle;

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
    // Called by MaterialPool each time a new material instance is needed.
    // Rebuilds MaskMaterial from the global shader cache if it was destroyed between game cycles.
    public static Material GetOrRebuildMaskMaterial()
    {
        if (MaskMaterial != null)
            return MaskMaterial;

        Log("MaskMaterial was destroyed between game cycles — rebuilding from shader cache");
        var shader = MaskShader != null ? MaskShader : Shader.Find("Unlit/CutoffCustom");
        if (shader == null)
        {
            Error("Cannot rebuild MaskMaterial: shader not found in cache.");
            return null;
        }

        MaskMaterial = new Material(shader);
        MaskMaterial.hideFlags = HideFlags.DontUnloadUnusedAsset;
        MaskShader = MaskMaterial.shader;
        MaskShader.hideFlags   = HideFlags.DontUnloadUnusedAsset;
        GW4KArmor.UI.MaterialPool.StaticMask = new Material(MaskMaterial);
        GW4KArmor.UI.MaterialPool.StaticMask.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return MaskMaterial;
    }

    public static void Log(string msg)
    {
        Verse.Log.Message("<color=magenta>[GW4kArmor]</color> " + (msg ?? "<i><null></i>"));
    }

    public static void Error(string msg, Exception e = null)
    {
        Verse.Log.Error("<color=magenta>[GW4kArmor]</color> " + (msg ?? "<i><null></i>"));
        var flag = e != null;
        if (flag) Verse.Log.Error(e.ToString());
    }

    public Core(ModContentPack content) : base(content)
    {
        Log("Hello, world!");
        try
        {
            HarmonyInstance = new Harmony(Content.PackageId);
            HarmonyInstance.PatchAll();
        }
        catch (Exception e)
        {
            Error("Harmony patching failed:", e);
        }

        LongEventHandler.QueueLongEvent(FindMaskableThings, "GW4KArmor.LoadText", false, null);
        LongEventHandler.QueueLongEvent(LoadBundleContent, "GW4KArmor.LoadText", false, null);
    }

    private static void FindMaskableThings()
    {
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            CompProperties_TriColorMask compProperties = thingDef.GetCompProperties<CompProperties_TriColorMask>();
            bool flag = compProperties == null;
            if (!flag)
            {
                compProperties.Def = thingDef;
                AllMaskable.Add(compProperties);
            }
        }

        Log(string.Format("Found {0} maskable armor or weapons.", AllMaskable.Count));
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
            Error("Unknown platform! This mod will not work.");
            platform = null;
        }

        string[] possibleVersions = { "1.6", "1.5", "1.4" };         // changed 1.4 asset bundles to version independent method

        AssetBundle assetBundle = null;

        foreach (var version in possibleVersions)
        {
            string path = Path.Combine(Content.RootDir, version, "AssetBundles", platform, "gw4k");

            Log($"Trying asset bundle path: {path}");
            if (File.Exists(path))
            {
                assetBundle = AssetBundle.LoadFromFile(path);
                if (assetBundle != null)
                {
                    Log($"Loaded asset bundle from {version}");
                    break;
                }

                // LoadFromFile returns null when the bundle is already loaded by another system.
                // UnloadUnusedAssets() may have freed the instantiated Material and Shader objects
                // from that bundle before we get here. However, the bundle's raw file data is
                // still intact, so we can reload a fresh Shader directly from it, then build a
                // fresh Material — bypassing any zombie wrappers entirely.
                foreach (var loaded in AssetBundle.GetAllLoadedAssetBundles())
                {
                    if (!loaded.Contains("Assets/Material/CustomMaskMaterial.mat")) continue;

                    var freshShader = loaded.LoadAsset<Shader>("CutoffCustom");
                    if (freshShader != null)
                    {
                        Log($"Bundle pre-loaded (version {version}) — reloaded fresh shader, building new material");
                        MaskMaterial = new Material(freshShader);
                        MaskMaterial.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        MaskShader             = freshShader;
                        MaskShader.hideFlags   = HideFlags.DontUnloadUnusedAsset;
                        GW4KArmor.UI.MaterialPool.StaticMask = new Material(MaskMaterial);
                        GW4KArmor.UI.MaterialPool.StaticMask.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        _bundle = loaded;
                        return;
                    }

                    // Shader reload also failed — keep the bundle reference and let the normal
                    // LoadAsset path below attempt a full recovery.
                    assetBundle = loaded;
                    Log($"Recovered already-loaded bundle (version {version})");
                    break;
                }
                if (assetBundle != null) break;
            }
        }
        if (assetBundle == null)
        {
            Error("Asset bundle failed to load, so the shader cannot be used! This mod will not work.");
        }
        else
        {
            _bundle = assetBundle;
            MaskMaterial = assetBundle.LoadAsset<Material>("CustomMaskMaterial");

            // Bundle assets may have been freed by UnloadUnusedAssets() before we recovered the
            // bundle. Fall back to Shader.Find — Unity keeps compiled shaders in a global cache
            // even after their originating bundle assets are released.
            if (MaskMaterial == null)
            {
                Log("Material not found in bundle (assets may have been unloaded) — falling back to Shader.Find");
                var cachedShader = Shader.Find("Unlit/CutoffCustom");
                if (cachedShader != null)
                    MaskMaterial = new Material(cachedShader);
            }

            if (MaskMaterial == null)
                Error("Asset bundle was loaded but failed to find the mask material. This mod will not work.");
            else
            {
                MaskShader = MaskMaterial.shader;
                GW4KArmor.UI.MaterialPool.StaticMask = new Material(MaskMaterial);

                // Prevent Unity from unloading these during inter-cycle asset cleanup.
                // Without this flag, aggressive UnloadUnusedAssets() passes between devtest
                // cycles destroy the native GPU resources while C# wrappers remain, causing
                // a native crash on the second map gen.
                MaskMaterial.hideFlags = HideFlags.DontUnloadUnusedAsset;
                MaskShader.hideFlags   = HideFlags.DontUnloadUnusedAsset;
                GW4KArmor.UI.MaterialPool.StaticMask.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
        }
    }
}