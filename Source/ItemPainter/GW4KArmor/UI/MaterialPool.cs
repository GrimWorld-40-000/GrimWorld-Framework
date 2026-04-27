using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace GW4KArmor.UI
{
    [StaticConstructorOnStartup]
    public static class MaterialPool
    {
        internal static Material StaticMask;
        private static readonly Queue<Material> pool = new Queue<Material>();

        public static Material Get()
        {
            // Drain any materials destroyed between game cycles before returning one
            while (pool.TryDequeue(out var material))
            {
                if (material != null)
                    return material;
            }

            return new Material(Core.GetOrRebuildMaskMaterial());
        }

        public static void Return(Material mat)
        {
            pool.Enqueue(mat);
        }
    }
}
