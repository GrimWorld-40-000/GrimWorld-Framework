using GW_Frame.AssetUtils;
using GW_Frame.Debugging;
using UnityEngine;
using Verse;
using ShaderPropertyIDs = GW_Frame.AssetUtils.ShaderPropertyIDs;

namespace GW_Frame
{
    /// <summary>
    /// This class is purely for testing/debugging the Cutout_LUT shader.
    /// Keep for future devs though as a reference.
    /// </summary>
    public class ShaderTestThing : ThingWithComps
    {
        private Color[] _colors = new Color[6];
        private float _blendStrength = 0.5f;
        private Material _material;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            TryGetThingMaterial();
            TrySetShaderColors();
            TrySetShaderBlendStrength();
        }
        
        private void TryGetThingMaterial()
        {
            if (def.graphicData?.Graphic?.MatSingle != null)
            {
                _material = def.graphicData.Graphic.MatSingle;
                
                Texture2D randomLUTTex = GetRandomLUTTexture();
                _material.SetTexture(ShaderPropertyIDs.LUTTexID, randomLUTTex);
                
                GWLog.Message($"[ShaderTestThing] Assigned material: {_material.name}");
            }
            else
            {
                GWLog.Warning("[ShaderTestThing] Failed to retrieve material from graphicData.");
            }
        }
        
        private void TrySetShaderColors()
        {
            if (_material == null)
            {
                GWLog.Warning("[ShaderTestThing] Material is null, skipping shader color assignment.");
                return;
            }
            
            _colors = GetRandomColors(0f, 1f);
            
            Vector4[] colorVectors = new Vector4[_colors.Length];
            for (int i = 0; i < _colors.Length; i++)
            {
                colorVectors[i] = _colors[i]; // Convert Color to Vector4
            }
            
            _material.SetInt(ShaderPropertyIDs.ColorCountID, colorVectors.Length);
            _material.SetVectorArray(ShaderPropertyIDs.ColorsID, colorVectors);
            GWLog.Message("[ShaderTestThing] Shader colors set successfully.");
        }
        
        private void TrySetShaderBlendStrength()
        {
            if (_material == null)
            {
                GWLog.Warning("[ShaderTestThing] Material is null, skipping shader blend strength assignment.");
                return;
            }
            
            _material.SetFloat(ShaderPropertyIDs.BlendStrengthID, _blendStrength);
            GWLog.Message($"[ShaderTestThing] Shader blend strength set to: {_blendStrength}");
        }
        
        /// <summary>
        /// Selects a random LUT texture from TextureCache.
        /// </summary>
        private static Texture2D GetRandomLUTTexture()
        {
            Texture2D[] lutTextures =
            [
                TextureCache.Noise_008,
                TextureCache.Noise_018,
                TextureCache.Noise_019,
                TextureCache.Noise_030,
                TextureCache.Noise_065,
                TextureCache.Noise_077
            ];
            
            Texture2D selectedTexture = lutTextures[Random.Range(0, lutTextures.Length)];
            GWLog.Message($"[ShaderTestThing] Selected random LUT texture: {selectedTexture.name}");
            return selectedTexture;
        }
        
        /// <summary>
        /// Generates an array of 6 random colors with full RGB variation.
        /// </summary>
        private static Color[] GetRandomColors(float rangeMin, float rangeMax)
        {
            Color[] colors = new Color[6];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(
                    Random.Range(Mathf.Clamp01(rangeMin), Mathf.Clamp01(rangeMax)), // Random Red (0 - 1)
                    Random.Range(Mathf.Clamp01(rangeMin), Mathf.Clamp01(rangeMax)), // Random Green (0 - 1)
                    Random.Range(Mathf.Clamp01(rangeMin), Mathf.Clamp01(rangeMax))  // Random Blue (0 - 1)
                );
            }
            
            GWLog.Message("[ShaderTestThing] Generated new random shader colors with full RGB range.");
            return colors;
        }
    }
}