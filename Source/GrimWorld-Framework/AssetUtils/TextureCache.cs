using UnityEngine;
using Verse;

namespace GW_Frame.AssetUtils
{
    /// <summary>
    /// This class is purely for testing/debugging the Cutout_LUT shader.
    /// These textures are used to choose a random mask texture for each test thing spawned.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class TextureCache
    {
        public static readonly Texture2D Noise_008 = ContentFinder<Texture2D>.Get("Noise_008");
        public static readonly Texture2D Noise_018 = ContentFinder<Texture2D>.Get("Noise_018");
        public static readonly Texture2D Noise_019 = ContentFinder<Texture2D>.Get("Noise_019");
        public static readonly Texture2D Noise_030 = ContentFinder<Texture2D>.Get("Noise_030");
        public static readonly Texture2D Noise_065 = ContentFinder<Texture2D>.Get("Noise_065");
        public static readonly Texture2D Noise_077 = ContentFinder<Texture2D>.Get("Noise_077");
    }
}