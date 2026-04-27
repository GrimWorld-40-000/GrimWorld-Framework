using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildBundle
{
    [MenuItem("GW4k/Build Asset Bundle (Windows)")]
    static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "StandaloneWindows64");

    [MenuItem("GW4k/Build Asset Bundle (All Platforms)")]
    static void BuildAll()
    {
        Build(BuildTarget.StandaloneWindows64, "StandaloneWindows64");
        Build(BuildTarget.StandaloneLinux64,   "StandaloneLinux64");
        Build(BuildTarget.StandaloneOSX,       "StandaloneOSX");
    }

    static void Build(BuildTarget target, string platformFolder)
    {
        // Create material from shader
        var shader = Shader.Find("Unlit/CutoffCustom");
        if (shader == null)
        {
            Debug.LogError("[GW4k] CutoffCustom shader not found — make sure Assets/Shaders/CutoffCustom.shader is imported.");
            return;
        }

        const string matPath = "Assets/Material/CustomMaskMaterial.mat";
        if (!Directory.Exists("Assets/Material"))
            Directory.CreateDirectory("Assets/Material");

        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(shader) { name = "CustomMaskMaterial" };
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
        }

        // Label both assets for the "gw4k" bundle
        AssetImporter.GetAtPath("Assets/Shaders/CutoffCustom.shader").assetBundleName = "gw4k";
        AssetImporter.GetAtPath(matPath).assetBundleName = "gw4k";

        // Resolve output path: up from Assets/ → project/ → Source/ → Framework root → 1.6/AssetBundles/
        string projectRoot   = Path.GetDirectoryName(Application.dataPath);
        string frameworkRoot = Path.GetDirectoryName(Path.GetDirectoryName(projectRoot));
        string outputDir     = Path.Combine(frameworkRoot, "1.6", "AssetBundles", platformFolder);

        Directory.CreateDirectory(outputDir);

        var manifest = BuildPipeline.BuildAssetBundles(
            outputDir,
            BuildAssetBundleOptions.None,
            target
        );

        if (manifest == null)
        {
            Debug.LogError($"[GW4k] Bundle build FAILED for {platformFolder}.");
            return;
        }

        // Delete the root manifest bundle Unity auto-generates — it causes "already loaded" errors
        // in RimWorld's mod loader when it scans the folder. Keep only gw4k + gw4k.manifest.
        foreach (var extra in new[] { platformFolder, platformFolder + ".manifest" })
        {
            string extraPath = Path.Combine(outputDir, extra);
            if (File.Exists(extraPath))
                File.Delete(extraPath);
        }

        Debug.Log($"[GW4k] Bundle built → {Path.Combine(outputDir, "gw4k")}");
    }
}
