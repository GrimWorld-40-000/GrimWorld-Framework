using Verse;

namespace GrimworldItemLimit
{
    /// <summary>
    /// This comp limits an item to only be craftable a specific number of times
    /// </summary>
    public class CompProperties_ItemCraftingLimit: CompProperties
    {
        public int maxNumberCraftable = 1;
        public CompProperties_ItemCraftingLimit()
        {
            compClass = typeof(Comp_ItemCraftingLimit);
        }
    }
}