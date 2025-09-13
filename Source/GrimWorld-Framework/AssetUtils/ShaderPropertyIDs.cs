using UnityEngine;

namespace GW_Frame.AssetUtils
{
    /// <summary>
    /// A utility class for storing shader property IDs to improve performance and maintainability.
    /// </summary>
    /// <remarks>
    /// Using Shader.PropertyToID() prevents repeated string lookups, 
    /// improving performance when working with shaders.
    /// </remarks>
    public static class ShaderPropertyIDs
    {
        private static readonly string MainTexName = "_MainTex";
        private static readonly string LUTTexName = "_LUTTex";
        private static readonly string BlendStrengthName = "_BlendStrength";
        private static readonly string ColorCountName = "_ColorCount";
        private static readonly string ColorsName = "_Colors";
        
        public static int MainTexID = Shader.PropertyToID(MainTexName);
        public static int LUTTexID = Shader.PropertyToID(LUTTexName);
        public static int BlendStrengthID = Shader.PropertyToID(BlendStrengthName);
        public static int ColorCountID = Shader.PropertyToID(ColorCountName);
        public static int ColorsID = Shader.PropertyToID(ColorsName);
    }
}