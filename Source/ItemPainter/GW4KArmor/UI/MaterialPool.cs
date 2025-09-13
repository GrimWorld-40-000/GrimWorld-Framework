using System.Collections.Generic;
using UnityEngine;

namespace GW4KArmor.UI
{
    public static class MaterialPool
    {
        internal static Material StaticMask = new(Core.MaskMaterial);
        private static readonly Queue<Material> pool = new();

        public static Material Get()
        {
            Material result;
            if (pool.TryDequeue(out Material material))
            {
                result = material;
            }
            else
            {
                material = new Material(Core.MaskMaterial);
                result = material;
            }

            return result;
        }

        public static void Return(Material mat)
        {
            pool.Enqueue(mat);
        }
    }
}