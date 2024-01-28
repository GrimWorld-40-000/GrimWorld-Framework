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

public class Core : Mod
{
	public static HashSet<CompProperties_TriColorMask> AllMaskable = new();

	public static Material MaskMaterial { get; private set; }
	public static Shader MaskShader { get; private set; }
	public static Harmony HarmonyInstance { get; private set; }
	
	
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
		
		string assetPath = Path.Combine(Content.RootDir, "1.4", "AssetBundles", platform, "gw4k");
		AssetBundle assetBundle = AssetBundle.LoadFromFile(assetPath);
		if (assetBundle == null)
		{
			Error("Asset bundle failed to load, so the shader cannot be used! This mod will not work.");
		}
		else
		{
			MaskMaterial = assetBundle.LoadAsset<Material>("CustomMaskMaterial");
			if (MaskMaterial == null)
				Error(
					"Asset bundle was loaded but failed to find the mask material. Why!? This mod will not work.");
			else
				MaskShader = MaskMaterial.shader;
		}
	}
}