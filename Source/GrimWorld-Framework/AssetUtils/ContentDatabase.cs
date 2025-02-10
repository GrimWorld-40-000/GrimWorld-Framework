using System.Collections.Generic;
using System.IO;
using GW_Frame.Debugging;
using GW_Frame.Settings;
using UnityEngine;
using Verse;

namespace GW_Frame.AssetUtils
{
    [StaticConstructorOnStartup]
    public static class ContentDatabase
    {
        private static AssetBundle bundleInt;
        private static Dictionary<string, Shader> lookupShaders;
        
        public static readonly Shader Cutout_LUT = LoadShader(Path.Combine("Assets/Shaders", "Cutout_LUT.shader"));
        
        public static AssetBundle GWBundle
        {
            get
            {
                if (bundleInt != null) 
                    return bundleInt;
                
                bundleInt = GrimWorldMod.Mod.MainBundle;
                //GWLog.Message("bundleInt: " + bundleInt.name);
                
                return bundleInt;
            }
        }
        
        private static Shader LoadShader(string shaderName)
        {
            lookupShaders ??= new Dictionary<string, Shader>();
            
            if (!lookupShaders.ContainsKey(shaderName))
            {
                lookupShaders[shaderName] = GWBundle.LoadAsset<Shader>(shaderName);
            }
            
            Shader shader = lookupShaders[shaderName];
            if (shader == null)
            {
                GWLog.Warning($"Failed to load shader: {shaderName}");
                return ShaderDatabase.DefaultShader;
            }
            
            GWLog.Message($"Successfully loaded shader: {shaderName}");
            return shader;
        }
    }
}