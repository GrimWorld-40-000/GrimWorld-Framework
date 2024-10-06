using System.Collections.Generic;
using Verse;

namespace GrimworldItemLimit
{
    public class Comp_ItemCraftingLimit : ThingComp
    {
        public static List<string> DisabledDefNames;



        public static bool IsEnabled(ThingDef def) => !DisabledDefNames?.Contains(def?.defName) ?? true;
    }
}