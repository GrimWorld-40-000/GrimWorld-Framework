using HarmonyLib;
using Verse;

namespace GW_Frame
{
    public class GrimWorld_FrameworkMod : Mod
    {
        private const string ModName = "GrimWorld_Framework.Mod";

        public GrimWorld_FrameworkMod(ModContentPack content) : base(content)
        {
            new Harmony(ModName).PatchAll();
        }
    }
}