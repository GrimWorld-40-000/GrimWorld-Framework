using System.Collections.Generic;
using UnityEngine;

namespace GW4KArmor.UI
{
    public static class MaterialPool
    {
        internal static Material StaticMask = new Material(Core.MaskMaterial);
        private static readonly Queue<Material> pool = new Queue<Material>();

        public static Material Get()
        {
            Material result;
            if (pool.TryDequeue(out var material))
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