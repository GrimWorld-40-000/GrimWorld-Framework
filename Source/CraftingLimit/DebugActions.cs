using LudeonTK;

namespace GrimworldItemLimit
{
    public class DebugActions
    {
        [DebugAction("Grimworld Framework", "Reset all crafting limits", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetLimits()
        {
            ItemsCraftedLibrary.GetCurrentLibrary().Reset();
        }
    }
}